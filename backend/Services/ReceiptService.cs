using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services.Interfaces;

namespace GoalboundFamily.Api.Services;

/// <summary>
/// Service implementation for receipt operations
/// </summary>
public class ReceiptService : IReceiptService
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IOcrService _ocrService;
    private readonly IReceiptParserService _parserService;
    private readonly ISupabaseStorageService _storageService;
    private readonly ILogger<ReceiptService> _logger;

    public ReceiptService(
        IReceiptRepository receiptRepository,
        IOcrService ocrService,
        IReceiptParserService parserService,
        ISupabaseStorageService storageService,
        ILogger<ReceiptService> logger)
    {
        _receiptRepository = receiptRepository;
        _ocrService = ocrService;
        _parserService = parserService;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<ReceiptResponseDto> UploadReceiptAsync(ReceiptUploadDto uploadDto)
    {
        _logger.LogInformation("Processing receipt upload for user {UserId}", uploadDto.UserId);

        // Step 1: Upload the image to Supabase Storage
        var imagePath = await SaveImageAsync(uploadDto.UserId, uploadDto.Image);

        // Step 2: Create receipt record
        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = uploadDto.UserId,
            ImagePath = imagePath,
            OriginalFileName = uploadDto.Image.FileName,
            Status = ReceiptStatus.Processing,
            UploadedAt = DateTime.UtcNow
        };

        await _receiptRepository.AddAsync(receipt);

        try
        {
            // Step 3: Perform OCR
            using var imageStream = uploadDto.Image.OpenReadStream();
            var ocrResult = await _ocrService.ProcessImageAsync(imageStream);

            if (!ocrResult.Success)
            {
                receipt.Status = ReceiptStatus.Failed;
                receipt.ErrorMessage = ocrResult.ErrorMessage;
                await _receiptRepository.UpdateAsync(receipt);
                return MapToDto(receipt);
            }

            receipt.RawOcrText = ocrResult.Text;
            receipt.OcrConfidence = ocrResult.Confidence;

            // Step 4: Parse receipt
            var parsedReceipt = await _parserService.ParseReceiptAsync(ocrResult);

            receipt.MerchantName = parsedReceipt.MerchantName;
            receipt.ReceiptDate = parsedReceipt.ReceiptDate;
            receipt.TotalAmount = parsedReceipt.TotalAmount;
            receipt.Status = ReceiptStatus.ReviewRequired;

            // Step 5: Add line items
            var items = parsedReceipt.Items.Select(item => new ReceiptItem
            {
                Id = Guid.NewGuid(),
                ReceiptId = receipt.Id,
                ItemName = item.ItemName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
                LineNumber = item.LineNumber,
                IsManuallyAdded = false,
                OcrConfidence = item.Confidence,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            receipt.Items = items;

            await _receiptRepository.UpdateAsync(receipt);

            _logger.LogInformation("Receipt {ReceiptId} processed successfully with {ItemCount} items",
                receipt.Id, items.Count);

            return MapToDto(receipt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing receipt {ReceiptId}", receipt.Id);
            receipt.Status = ReceiptStatus.Failed;
            receipt.ErrorMessage = $"Processing error: {ex.Message}";
            await _receiptRepository.UpdateAsync(receipt);
            return MapToDto(receipt);
        }
    }

    public async Task<ReceiptResponseDto?> GetReceiptAsync(Guid receiptId)
    {
        var receipt = await _receiptRepository.GetByIdWithItemsAsync(receiptId);
        return receipt != null ? MapToDto(receipt) : null;
    }

    public async Task<IEnumerable<ReceiptResponseDto>> GetUserReceiptsAsync(Guid userId)
    {
        var receipts = await _receiptRepository.GetByUserIdAsync(userId);
        return receipts.Select(MapToDto);
    }

    public async Task<ReceiptItemDto> AddItemToReceiptAsync(AddReceiptItemDto addItemDto)
    {
        var receipt = await _receiptRepository.GetByIdWithItemsAsync(addItemDto.ReceiptId);
        if (receipt == null)
        {
            throw new InvalidOperationException($"Receipt {addItemDto.ReceiptId} not found");
        }

        var nextLineNumber = receipt.Items.Any() ? receipt.Items.Max(i => i.LineNumber) + 1 : 0;

        var item = new ReceiptItem
        {
            Id = Guid.NewGuid(),
            ReceiptId = addItemDto.ReceiptId,
            ItemName = addItemDto.ItemName,
            Quantity = addItemDto.Quantity,
            UnitPrice = addItemDto.UnitPrice,
            TotalPrice = addItemDto.TotalPrice,
            LineNumber = nextLineNumber,
            IsManuallyAdded = true,
            CreatedAt = DateTime.UtcNow
        };

        await _receiptRepository.AddItemToReceiptAsync(item);

        _logger.LogInformation("Added manual item to receipt {ReceiptId}", addItemDto.ReceiptId);

        return MapItemToDto(item);
    }

    public async Task<ReceiptResponseDto> ConfirmReceiptAsync(ConfirmReceiptDto confirmDto)
    {
        var receipt = await _receiptRepository.GetByIdWithItemsAsync(confirmDto.ReceiptId);
        if (receipt == null)
        {
            throw new InvalidOperationException($"Receipt {confirmDto.ReceiptId} not found");
        }

        // Update items with user modifications
        var updatedItems = confirmDto.Items.Select(dto => new ReceiptItem
        {
            Id = dto.Id ?? Guid.NewGuid(),
            ReceiptId = receipt.Id,
            ItemName = dto.ItemName,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            TotalPrice = dto.TotalPrice,
            LineNumber = dto.LineNumber,
            IsManuallyAdded = dto.IsManuallyAdded,
            OcrConfidence = dto.OcrConfidence,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await _receiptRepository.UpdateReceiptItemsAsync(receipt.Id, updatedItems);

        // Update receipt status
        receipt.Status = ReceiptStatus.Confirmed;
        receipt.UpdatedAt = DateTime.UtcNow;
        await _receiptRepository.UpdateAsync(receipt);

        _logger.LogInformation("Receipt {ReceiptId} confirmed with {ItemCount} items",
            receipt.Id, updatedItems.Count);

        // Reload to get updated items
        var updatedReceipt = await _receiptRepository.GetByIdWithItemsAsync(receipt.Id);
        return MapToDto(updatedReceipt!);
    }

    private async Task<string> SaveImageAsync(Guid userId, IFormFile image)
    {
        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";

        // Get content type from file extension
        var contentType = image.ContentType ?? "application/octet-stream";

        // Upload to Supabase Storage
        using var stream = image.OpenReadStream();
        var imageUrl = await _storageService.UploadFileAsync(userId, fileName, stream, contentType);

        _logger.LogInformation("Uploaded receipt image to Supabase Storage: {Url}", imageUrl);

        return imageUrl;
    }

    private ReceiptResponseDto MapToDto(Receipt receipt)
    {
        return new ReceiptResponseDto
        {
            Id = receipt.Id,
            UserId = receipt.UserId,
            Status = receipt.Status,
            MerchantName = receipt.MerchantName,
            ReceiptDate = receipt.ReceiptDate,
            TotalAmount = receipt.TotalAmount,
            OcrConfidence = receipt.OcrConfidence,
            ErrorMessage = receipt.ErrorMessage,
            UploadedAt = receipt.UploadedAt,
            Items = receipt.Items.Select(MapItemToDto).ToList()
        };
    }

    private ReceiptItemDto MapItemToDto(ReceiptItem item)
    {
        return new ReceiptItemDto
        {
            Id = item.Id,
            ItemName = item.ItemName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.TotalPrice,
            LineNumber = item.LineNumber,
            IsManuallyAdded = item.IsManuallyAdded,
            OcrConfidence = item.OcrConfidence
        };
    }
}
