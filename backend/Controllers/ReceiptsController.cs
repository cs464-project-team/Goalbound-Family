using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoalboundFamily.Api.Controllers;

/// <summary>
/// Controller for receipt OCR and management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReceiptsController : ControllerBase
{
    private readonly IReceiptService _receiptService;
    private readonly ILogger<ReceiptsController> _logger;

    public ReceiptsController(
        IReceiptService receiptService,
        ILogger<ReceiptsController> logger)
    {
        _receiptService = receiptService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a receipt image for OCR processing
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="householdId">Optional Household ID</param>
    /// <param name="image">Receipt image file</param>
    /// <returns>Processed receipt with extracted items</returns>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ReceiptResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReceiptResponseDto>> UploadReceipt(
        [FromForm] Guid userId,
        [FromForm] Guid? householdId,
        [FromForm] IFormFile image)
    {
        try
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest(new { message = "No image file provided" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var extension = Path.GetExtension(image.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file type. Allowed: JPG, PNG, PDF" });
            }

            // Validate file size (max 10MB)
            if (image.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new { message = "File size exceeds 10MB limit" });
            }

            var uploadDto = new ReceiptUploadDto
            {
                UserId = userId,
                HouseholdId = householdId,
                Image = image
            };

            var result = await _receiptService.UploadReceiptAsync(uploadDto);

            _logger.LogInformation("Receipt uploaded successfully: {ReceiptId}", result.Id);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading receipt");
            return StatusCode(500, new { message = "Internal server error during receipt upload" });
        }
    }

    /// <summary>
    /// Get a specific receipt by ID
    /// </summary>
    /// <param name="receiptId">Receipt ID</param>
    /// <returns>Receipt with items</returns>
    [HttpGet("{receiptId}")]
    [ProducesResponseType(typeof(ReceiptResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReceiptResponseDto>> GetReceipt(Guid receiptId)
    {
        try
        {
            var receipt = await _receiptService.GetReceiptAsync(receiptId);

            if (receipt == null)
            {
                return NotFound(new { message = $"Receipt {receiptId} not found" });
            }

            return Ok(receipt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting receipt {ReceiptId}", receiptId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all receipts for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of receipts</returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<ReceiptResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReceiptResponseDto>>> GetUserReceipts(Guid userId)
    {
        try
        {
            var receipts = await _receiptService.GetUserReceiptsAsync(userId);
            return Ok(receipts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting receipts for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Add a manual item to a receipt (user-added item)
    /// </summary>
    /// <param name="addItemDto">Item details</param>
    /// <returns>Created item</returns>
    [HttpPost("items")]
    [ProducesResponseType(typeof(ReceiptItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReceiptItemDto>> AddItem([FromBody] AddReceiptItemDto addItemDto)
    {
        try
        {
            var item = await _receiptService.AddItemToReceiptAsync(addItemDto);
            return CreatedAtAction(nameof(GetReceipt), new { receiptId = addItemDto.ReceiptId }, item);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to receipt");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Confirm receipt after user review
    /// Updates items and marks receipt as confirmed
    /// </summary>
    /// <param name="confirmDto">Confirmed receipt data</param>
    /// <returns>Updated receipt</returns>
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(ReceiptResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReceiptResponseDto>> ConfirmReceipt([FromBody] ConfirmReceiptDto confirmDto)
    {
        try
        {
            var receipt = await _receiptService.ConfirmReceiptAsync(confirmDto);
            return Ok(receipt);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming receipt");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Assign receipt items to household members with GST/Service Charge calculations
    /// </summary>
    /// <param name="assignDto">Assignment details</param>
    /// <returns>Updated receipt with assignments</returns>
    [HttpPost("assign")]
    [ProducesResponseType(typeof(ReceiptResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReceiptResponseDto>> AssignItemsToMembers([FromBody] AssignReceiptItemsDto assignDto)
    {
        try
        {
            var receipt = await _receiptService.AssignItemsToMembersAsync(assignDto);
            return Ok(receipt);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning receipt items");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
