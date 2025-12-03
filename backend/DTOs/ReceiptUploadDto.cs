using System.ComponentModel.DataAnnotations;

namespace GoalboundFamily.Api.DTOs;

/// <summary>
/// DTO for uploading a receipt image
/// </summary>
public class ReceiptUploadDto
{
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Optional household ID to associate this receipt with
    /// </summary>
    public Guid? HouseholdId { get; set; }

    [Required]
    public IFormFile Image { get; set; } = null!;
}
