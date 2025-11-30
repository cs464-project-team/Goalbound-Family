using System.ComponentModel.DataAnnotations;

namespace GoalboundFamily.Api.DTOs;

/// <summary>
/// DTO for manually adding a receipt item
/// </summary>
public class AddReceiptItemDto
{
    [Required]
    public Guid ReceiptId { get; set; }

    [Required]
    [MaxLength(500)]
    public string ItemName { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int Quantity { get; set; } = 1;

    public decimal? UnitPrice { get; set; }

    [Required]
    [Range(0.01, 1000000)]
    public decimal TotalPrice { get; set; }
}
