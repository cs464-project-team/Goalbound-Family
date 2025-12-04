using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Services.Interfaces;
using GoalboundFamily.Api.Events;
using MediatR;

namespace GoalboundFamily.Api.Handlers;

public class ExpenseLoggedEventHandler : INotificationHandler<ExpenseLoggedEvent>
{
    private readonly IQuestProgressService _questService;

    public ExpenseLoggedEventHandler(IQuestProgressService questService)
    {
        _questService = questService;
    }

    public async Task Handle(ExpenseLoggedEvent notification, CancellationToken cancellationToken)
    {
        await _questService.HandleExpenseLogged(notification.UserId, notification.HouseholdId, notification.Category);
    }
}
