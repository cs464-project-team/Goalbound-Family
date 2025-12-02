namespace GoalboundFamily.Api.DTOs;

/// <summary>
/// DTO for receipt item assignment to a household member
/// </summary>
public class ReceiptItemAssignmentDto
{
    public Guid? Id { get; set; }
    public Guid HouseholdMemberId { get; set; }
    public string HouseholdMemberName { get; set; } = string.Empty;
    public decimal AssignedQuantity { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal ServiceChargeAmount { get; set; }
    public decimal GstAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
