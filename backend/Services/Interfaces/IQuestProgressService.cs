using System;
using System.Threading.Tasks;

namespace GoalboundFamily.Api.Services.Interfaces;

public interface IQuestProgressService
{
    Task HandleReceiptScanned(Guid userId, Guid householdId);
    Task HandleExpenseLogged(Guid userId, Guid householdId, string category);
}
