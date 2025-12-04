using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services.Interfaces;
using GoalboundFamily.Api.Data;
using Microsoft.EntityFrameworkCore;

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
    private readonly ApplicationDbContext _dbContext;
    private readonly IHouseholdAuthorizationService _authService;
    private readonly ILogger<ReceiptService> _logger;
    private readonly IQuestProgressService _questService;

    public ReceiptService(
        IReceiptRepository receiptRepository,
        IOcrService ocrService,
        IReceiptParserService parserService,
        ISupabaseStorageService storageService,
        ApplicationDbContext dbContext,
        IHouseholdAuthorizationService authService,
        ILogger<ReceiptService> logger,
        IQuestProgressService questService)
    {
        _receiptRepository = receiptRepository;
        _ocrService = ocrService;
        _parserService = parserService;
        _storageService = storageService;
        _dbContext = dbContext;
        _authService = authService;
        _logger = logger;
        _questService = questService;
    }

    public async Task<ReceiptResponseDto> UploadReceiptAsync(ReceiptUploadDto uploadDto, Guid requestingUserId)
    {
        // Verify the requesting user matches the user in the upload DTO
        if (uploadDto.UserId != requestingUserId)
        {
            throw new UnauthorizedAccessException("Cannot upload receipt for another user");
        }

        // If household ID is provided, verify user is in the household
        if (uploadDto.HouseholdId.HasValue)
        {
            await _authService.ValidateHouseholdAccessAsync(requestingUserId, uploadDto.HouseholdId.Value);
        }

        _logger.LogInformation("Processing receipt upload for user {UserId}", uploadDto.UserId);

        // Step 1: Upload the image to Supabase Storage
        var imagePath = await SaveImageAsync(uploadDto.UserId, uploadDto.Image);

        // Step 2: Create receipt record
        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = uploadDto.UserId,
            HouseholdId = uploadDto.HouseholdId,
            ImagePath = imagePath,
            OriginalFileName = uploadDto.Image.FileName,
            Status = ReceiptStatus.Processing,
            UploadedAt = DateTime.UtcNow
        };

        await _receiptRepository.AddAsync(receipt);
        await _receiptRepository.SaveChangesAsync();

        try
        {
            // Step 3: Perform OCR
            using var imageStream = uploadDto.Image.OpenReadStream();
            var ocrResult = await _ocrService.ProcessImageAsync(imageStream);

            if (!ocrResult.Success)
            {
                receipt.Status = ReceiptStatus.Failed;
                receipt.ErrorMessage = ocrResult.ErrorMessage;
                await _dbContext.SaveChangesAsync();

                // Reload receipt with household members for response
                var failedReceipt = await _dbContext.Receipts
                    .Include(r => r.Household)
                        .ThenInclude(h => h.Members)
                            .ThenInclude(m => m.User)
                    .FirstOrDefaultAsync(r => r.Id == receipt.Id);

                return MapToDto(failedReceipt ?? receipt);
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

            await _dbContext.ReceiptItems.AddRangeAsync(items);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Receipt {ReceiptId} processed successfully with {ItemCount} items",
                receipt.Id, items.Count);

            if (receipt.HouseholdId.HasValue)
            {
                await _questService.HandleReceiptScanned(receipt.UserId, receipt.HouseholdId.Value);
            }

            // Reload receipt with household members for response
            var receiptWithMembers = await _dbContext.Receipts
                .Include(r => r.Items)
                .Include(r => r.Household)
                    .ThenInclude(h => h.Members)
                        .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(r => r.Id == receipt.Id);

            return MapToDto(receiptWithMembers ?? receipt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing receipt {ReceiptId}", receipt.Id);
            receipt.Status = ReceiptStatus.Failed;
            receipt.ErrorMessage = $"Processing error: {ex.Message}";
            await _dbContext.SaveChangesAsync();

            // Reload receipt with household members for response
            var errorReceipt = await _dbContext.Receipts
                .Include(r => r.Household)
                    .ThenInclude(h => h.Members)
                        .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(r => r.Id == receipt.Id);

            return MapToDto(errorReceipt ?? receipt);
        }
    }

    public async Task<ProcessReceiptOcrResponseDto> ProcessReceiptOcrOnlyAsync(ReceiptUploadDto uploadDto, Guid requestingUserId)
    {
        // Verify the requesting user matches the user in the upload DTO
        if (uploadDto.UserId != requestingUserId)
        {
            throw new UnauthorizedAccessException("Cannot process receipt for another user");
        }

        // If household ID is provided, verify user is in the household
        if (uploadDto.HouseholdId.HasValue)
        {
            await _authService.ValidateHouseholdAccessAsync(requestingUserId, uploadDto.HouseholdId.Value);
        }

        _logger.LogInformation("Processing receipt OCR without saving for user {UserId}", uploadDto.UserId);

        var response = new ProcessReceiptOcrResponseDto
        {
            Success = false
        };

        try
        {
            // Step 1: Upload the image to Supabase Storage
            var imagePath = await SaveImageAsync(uploadDto.UserId, uploadDto.Image);
            response.ImagePath = imagePath;
            response.OriginalFileName = uploadDto.Image.FileName;

            // Step 2: Perform OCR
            using var imageStream = uploadDto.Image.OpenReadStream();
            var ocrResult = await _ocrService.ProcessImageAsync(imageStream);

            if (!ocrResult.Success)
            {
                response.ErrorMessage = ocrResult.ErrorMessage;
                return response;
            }

            response.RawOcrText = ocrResult.Text;
            response.OcrConfidence = ocrResult.Confidence;

            // Step 3: Parse receipt
            var parsedReceipt = await _parserService.ParseReceiptAsync(ocrResult);

            response.MerchantName = parsedReceipt.MerchantName;
            response.ReceiptDate = parsedReceipt.ReceiptDate;
            response.TotalAmount = parsedReceipt.TotalAmount;

            // Step 4: Map items to DTO
            response.Items = parsedReceipt.Items.Select(item => new ParsedReceiptItemDto
            {
                TempId = Guid.NewGuid().ToString(),
                ItemName = item.ItemName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
                LineNumber = item.LineNumber,
                IsManuallyAdded = false,
                OcrConfidence = item.Confidence
            }).ToList();

            response.Success = true;

            // Step 5: Load household members if household ID provided
            if (uploadDto.HouseholdId.HasValue)
            {
                var household = await _dbContext.Households
                    .Include(h => h.Members)
                        .ThenInclude(m => m.User)
                    .FirstOrDefaultAsync(h => h.Id == uploadDto.HouseholdId.Value);

                if (household?.Members != null)
                {
                    response.HouseholdMembers = household.Members
                        .Select(m => new HouseholdMemberSummaryDto
                        {
                            Id = m.Id,
                            UserId = m.UserId,
                            UserName = m.User != null ? $"{m.User.FirstName} {m.User.LastName}" : "Unknown",
                            Role = m.Role
                        })
                        .ToList();
                }
            }

            _logger.LogInformation("OCR processing successful with {ItemCount} items (not saved to DB)", response.Items.Count);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing receipt OCR");
            response.Success = false;
            response.ErrorMessage = $"Processing error: {ex.Message}";
            return response;
        }
    }

    public async Task<ReceiptResponseDto?> GetReceiptAsync(Guid receiptId, Guid requestingUserId)
    {
        // Verify user has access to this receipt
        await _authService.ValidateReceiptAccessAsync(requestingUserId, receiptId);

        // Load receipt with all needed relationships
        var receipt = await _dbContext.Receipts
            .Include(r => r.Items)
                .ThenInclude(i => i.Assignments)
                    .ThenInclude(a => a.HouseholdMember)
                        .ThenInclude(hm => hm.User)
            .Include(r => r.Household)
                .ThenInclude(h => h.Members)
                    .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(r => r.Id == receiptId);

        return receipt != null ? MapToDto(receipt) : null;
    }

    public async Task<IEnumerable<ReceiptResponseDto>> GetUserReceiptsAsync(Guid userId, Guid requestingUserId)
    {
        // Users can only query their own receipts
        if (userId != requestingUserId)
        {
            throw new UnauthorizedAccessException("Cannot access another user's receipts");
        }

        var receipts = await _receiptRepository.GetByUserIdAsync(userId);
        return receipts.Select(MapToDto);
    }

    public async Task<IEnumerable<ReceiptResponseDto>> GetHouseholdReceiptsAsync(Guid householdId, Guid requestingUserId)
    {
        // Verify user is in the household
        await _authService.ValidateHouseholdAccessAsync(requestingUserId, householdId);


        var receipts = await _dbContext.Receipts
            .Include(r => r.Items)
                .ThenInclude(i => i.Assignments)
                    .ThenInclude(a => a.HouseholdMember)
                        .ThenInclude(hm => hm.User)
            .Include(r => r.Household)
                .ThenInclude(h => h.Members)
                    .ThenInclude(m => m.User)
            .Where(r => r.HouseholdId == householdId)
            .OrderByDescending(r => r.ReceiptDate ?? r.UploadedAt)
            .ToListAsync();

        return receipts.Select(MapToDto);
    }

    public async Task<ReceiptItemDto> AddItemToReceiptAsync(AddReceiptItemDto addItemDto, Guid requestingUserId)
    {
        // Verify user has access to this receipt
        await _authService.ValidateReceiptAccessAsync(requestingUserId, addItemDto.ReceiptId);

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

    public async Task<ReceiptResponseDto> ConfirmReceiptAsync(ConfirmReceiptDto confirmDto, Guid requestingUserId)
    {
        // Verify user has access to this receipt
        await _authService.ValidateReceiptAccessAsync(requestingUserId, confirmDto.ReceiptId);

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

    public async Task<ReceiptResponseDto> AssignItemsToMembersAsync(AssignReceiptItemsDto assignDto, Guid requestingUserId)
    {
        // Verify user is in the household
        await _authService.ValidateHouseholdAccessAsync(requestingUserId, assignDto.HouseholdId);

        Receipt receipt;

        // Check if this is for an existing receipt or a new one
        if (assignDto.ReceiptId.HasValue)
        {
            // Additional check: verify user has access to the existing receipt
            await _authService.ValidateReceiptAccessAsync(requestingUserId, assignDto.ReceiptId.Value);

            // Load existing receipt
            receipt = await _dbContext.Receipts
                .Include(r => r.Items)
                    .ThenInclude(i => i.Assignments)
                        .ThenInclude(a => a.HouseholdMember)
                            .ThenInclude(hm => hm.User)
                .Include(r => r.Household)
                    .ThenInclude(h => h.Members)
                        .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(r => r.Id == assignDto.ReceiptId.Value);

            if (receipt == null)
            {
                throw new InvalidOperationException($"Receipt {assignDto.ReceiptId} not found");
            }
        }
        else
        {
            // Create new receipt
            if (!assignDto.UserId.HasValue)
            {
                throw new InvalidOperationException("UserId is required when creating a new receipt");
            }
            if (string.IsNullOrEmpty(assignDto.ImagePath))
            {
                throw new InvalidOperationException("ImagePath is required when creating a new receipt");
            }

            receipt = new Receipt
            {
                Id = Guid.NewGuid(),
                UserId = assignDto.UserId.Value,
                HouseholdId = assignDto.HouseholdId,
                ImagePath = assignDto.ImagePath,
                OriginalFileName = assignDto.OriginalFileName ?? "unknown",
                MerchantName = assignDto.MerchantName,
                ReceiptDate = assignDto.ReceiptDate,
                TotalAmount = assignDto.TotalAmount,
                RawOcrText = assignDto.RawOcrText,
                OcrConfidence = assignDto.OcrConfidence,
                Status = ReceiptStatus.Confirmed, // Directly confirmed
                UploadedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.Receipts.AddAsync(receipt);
        }

        // Verify household
        if (receipt.HouseholdId != assignDto.HouseholdId)
        {
            receipt.HouseholdId = assignDto.HouseholdId;
        }

        // For existing receipts: clear existing assignments and items (if not creating new)
        if (assignDto.ReceiptId.HasValue)
        {
            var existingAssignments = await _dbContext.ReceiptItemAssignments
                .Where(a => receipt.Items.Select(i => i.Id).Contains(a.ReceiptItemId))
                .ToListAsync();
            _dbContext.ReceiptItemAssignments.RemoveRange(existingAssignments);
        }

        // Create or reference receipt items and their assignments
        var newAssignments = new List<ReceiptItemAssignment>();
        var memberExpenditures = new Dictionary<Guid, decimal>();
        var createdItems = new List<ReceiptItem>();

        foreach (var itemAssignment in assignDto.ItemAssignments)
        {
            ReceiptItem receiptItem;

            // Check if this is a reference to an existing item or a new item
            if (itemAssignment.ReceiptItemId.HasValue)
            {
                // Existing item
                receiptItem = receipt.Items.FirstOrDefault(i => i.Id == itemAssignment.ReceiptItemId.Value);
                if (receiptItem == null)
                {
                    _logger.LogWarning("Receipt item {ItemId} not found, skipping", itemAssignment.ReceiptItemId);
                    continue;
                }
            }
            else
            {
                // New item - validate required fields
                if (string.IsNullOrEmpty(itemAssignment.ItemName) ||
                    !itemAssignment.Quantity.HasValue ||
                    !itemAssignment.TotalPrice.HasValue)
                {
                    _logger.LogWarning("Item assignment missing required fields, skipping");
                    continue;
                }

                // Create new item
                receiptItem = new ReceiptItem
                {
                    Id = Guid.NewGuid(),
                    ReceiptId = receipt.Id,
                    ItemName = itemAssignment.ItemName,
                    Quantity = itemAssignment.Quantity.Value,
                    UnitPrice = itemAssignment.UnitPrice ?? (itemAssignment.TotalPrice.Value / itemAssignment.Quantity.Value),
                    TotalPrice = itemAssignment.TotalPrice.Value,
                    LineNumber = itemAssignment.LineNumber ?? 0,
                    IsManuallyAdded = itemAssignment.IsManuallyAdded ?? false,
                    OcrConfidence = itemAssignment.OcrConfidence ?? 0,
                    CreatedAt = DateTime.UtcNow
                };

                createdItems.Add(receiptItem);
            }

            var unitPrice = receiptItem.TotalPrice / receiptItem.Quantity;

            foreach (var memberAssignment in itemAssignment.MemberAssignments)
            {
                // Calculate base amount for this member's portion
                var baseAmount = unitPrice * memberAssignment.AssignedQuantity;

                // Apply service charge to base amount
                var serviceChargeAmount = assignDto.ApplyServiceCharge ? baseAmount * 0.10m : 0;
                var amountAfterService = baseAmount + serviceChargeAmount;

                // Apply GST to (base + service charge)
                var gstAmount = assignDto.ApplyGst ? amountAfterService * 0.09m : 0;

                // Total for this member's portion
                var totalAmount = amountAfterService + gstAmount;

                var assignment = new ReceiptItemAssignment
                {
                    Id = Guid.NewGuid(),
                    ReceiptItemId = receiptItem.Id, // Use the actual item ID
                    HouseholdMemberId = memberAssignment.HouseholdMemberId,
                    AssignedQuantity = memberAssignment.AssignedQuantity,
                    BaseAmount = baseAmount,
                    ServiceChargeAmount = serviceChargeAmount,
                    GstAmount = gstAmount,
                    TotalAmount = totalAmount,
                    CreatedAt = DateTime.UtcNow
                };

                newAssignments.Add(assignment);

                // Track total expenditure per member
                if (!memberExpenditures.ContainsKey(memberAssignment.HouseholdMemberId))
                {
                    memberExpenditures[memberAssignment.HouseholdMemberId] = 0;
                }
                memberExpenditures[memberAssignment.HouseholdMemberId] += totalAmount;
            }
        }

        // Add new items to database if any were created
        if (createdItems.Any())
        {
            await _dbContext.ReceiptItems.AddRangeAsync(createdItems);
            _logger.LogInformation("Created {Count} new receipt items", createdItems.Count);
        }

        await _dbContext.ReceiptItemAssignments.AddRangeAsync(newAssignments);

        // Update household member expenditures and create Expense entries
        var now = DateTime.UtcNow;
        var expenseDate = receipt.ReceiptDate ?? now.Date;

        foreach (var (memberId, expenditure) in memberExpenditures)
        {
            var member = await _dbContext.HouseholdMembers.FindAsync(memberId);
            if (member != null)
            {
                member.MonthlyExpenditure += expenditure;
                member.LifetimeExpenditure += expenditure;
                member.LastExpenditureUpdate = now;

                // Create an Expense entry for this member's share
                var totalItemCount = (receipt.Items?.Count ?? 0) + createdItems.Count;
                var expense = new Expense
                {
                    Id = Guid.NewGuid(),
                    HouseholdId = assignDto.HouseholdId,
                    UserId = member.UserId,
                    CategoryId = assignDto.CategoryId,
                    Amount = expenditure,
                    Date = expenseDate,
                    Description = $"Receipt from {receipt.MerchantName ?? "Unknown"} - {totalItemCount} item(s)",
                    ReceiptId = receipt.Id
                };

                await _dbContext.Expenses.AddAsync(expense);
            }
        }

        // Mark receipt as confirmed
        receipt.Status = ReceiptStatus.Confirmed;
        receipt.UpdatedAt = now;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Assigned receipt {ReceiptId} items to {MemberCount} household members",
            receipt.Id, memberExpenditures.Count);

        // Reload with all relationships
        receipt = await _dbContext.Receipts
            .Include(r => r.Items)
                .ThenInclude(i => i.Assignments)
                    .ThenInclude(a => a.HouseholdMember)
                        .ThenInclude(hm => hm.User)
            .Include(r => r.Household)
                .ThenInclude(h => h.Members)
                    .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(r => r.Id == receipt.Id);

        return MapToDto(receipt!);
    }

    private ReceiptResponseDto MapToDto(Receipt receipt)
    {
        var dto = new ReceiptResponseDto
        {
            Id = receipt.Id,
            UserId = receipt.UserId,
            HouseholdId = receipt.HouseholdId,
            Status = receipt.Status,
            MerchantName = receipt.MerchantName,
            ReceiptDate = receipt.ReceiptDate,
            TotalAmount = receipt.TotalAmount,
            OcrConfidence = receipt.OcrConfidence,
            ErrorMessage = receipt.ErrorMessage,
            UploadedAt = receipt.UploadedAt,
            ImagePath = receipt.ImagePath,
            OriginalFileName = receipt.OriginalFileName,
            Items = receipt.Items.Select(MapItemToDto).ToList()
        };

        // Add household members if household is loaded
        if (receipt.Household?.Members != null)
        {
            dto.HouseholdMembers = receipt.Household.Members
                .Select(m => new HouseholdMemberSummaryDto
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    UserName = m.User != null ? $"{m.User.FirstName} {m.User.LastName}" : "Unknown",
                    Role = m.Role
                })
                .ToList();
        }

        return dto;
    }

    private ReceiptItemDto MapItemToDto(ReceiptItem item)
    {
        var dto = new ReceiptItemDto
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

        // Add assignments if loaded
        if (item.Assignments != null && item.Assignments.Any())
        {
            dto.Assignments = item.Assignments
                .Select(a => new ReceiptItemAssignmentDto
                {
                    Id = a.Id,
                    HouseholdMemberId = a.HouseholdMemberId,
                    HouseholdMemberName = a.HouseholdMember?.User != null
                        ? $"{a.HouseholdMember.User.FirstName} {a.HouseholdMember.User.LastName}"
                        : "Unknown",
                    AssignedQuantity = a.AssignedQuantity,
                    BaseAmount = a.BaseAmount,
                    ServiceChargeAmount = a.ServiceChargeAmount,
                    GstAmount = a.GstAmount,
                    TotalAmount = a.TotalAmount
                })
                .ToList();
        }

        return dto;
    }
}
