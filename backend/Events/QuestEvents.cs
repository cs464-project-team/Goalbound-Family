using MediatR;

namespace GoalboundFamily.Api.Events
{
    public record ReceiptScannedEvent(Guid UserId, Guid HouseholdId) : INotification;
    public record ExpenseLoggedEvent(Guid UserId, Guid HouseholdId, string Category) : INotification;

}
