using System.ComponentModel.DataAnnotations;

namespace GoalboundFamily.Api.DTOs;

/// <summary>
/// DTO for confirming a receipt after review
/// </summary>
public class ConfirmReceiptDto
{
    [Required]
    public Guid ReceiptId { get; set; }

    /// <summary>
    /// Updated items after user review (can include edits and additions)
    /// </summary>
    public List<ReceiptItemDto> Items { get; set; } = new();
}
