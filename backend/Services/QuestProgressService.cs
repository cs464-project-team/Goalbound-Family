using GoalboundFamily.Api.Events;
using GoalboundFamily.Api.Services.Interfaces;
using GoalboundFamily.Api.Repositories.Interfaces;
using MediatR;

namespace GoalboundFamily.Api.Services;

public class QuestProgressService : IQuestProgressService
{
    private readonly IMediator _mediator;
    private readonly IMemberQuestService _questService;
    private readonly IExpenseService _expenseService;
    private readonly IReceiptRepository _receiptRepo;

    public QuestProgressService(IMediator mediator, IExpenseService expenseService, IMemberQuestService questService, IReceiptRepository receiptRepo)
    {
        _mediator = mediator;
        _questService = questService;
        _expenseService = expenseService;
        _receiptRepo = receiptRepo;
        _householdMemberRepo = householdMemberRepo;

    }

    public async Task HandleReceiptScanned(Guid userId, Guid householdId)
    {
        // 1. Get total receipts scanned
        int totalReceipts = await _receiptRepo.GetReceiptCountByUserAndHouseholdAsync(userId, householdId);

        string householdMemberId = await _householdMemberRepo.GetByUserIdAsync(userId);

        // 2. Get all active quests for this member in "receipt" category
        var activeQuests = await _memberQuestRepository.GetActiveQuestsByMemberAndCategory(householdMemberId, "receipt");

        // 3. Update progress
        foreach (var mq in activeQuests)
        {
            mq.Progress = max(mq.Quest.Target, totalReceipts); // set progress to actual total
            if (mq.Progress >= mq.Quest.Target)
            {
                mq.IsCompleted = true;
                mq.CompletedAt = DateTime.UtcNow;
            }

            await _memberQuestRepository.UpdateAsync(mq);
        }
    }

    public async Task HandleExpenseLogged(Guid userId, Guid householdId, string category)
    {
        int year = DateTime.UtcNow.Year;
        int month = DateTime.UtcNow.Month;

        // 1. Get total expenses in this category
        int totalExpenses = await _expenseRepository.GetCountByUserMonthAsync(userId, year, month);

        string householdMemberId = await _householdMemberRepo.GetByUserIdAsync(userId);

        // 2. Get all active quests for this member in "expense" category
        var activeQuests = await _memberQuestRepository.GetActiveQuestsByMemberAndCategory(memberId, "expense");

        // 3. Update progress
        foreach (var mq in activeQuests)
        {
            mq.Progress = max(mq.Quest.Target, totalExpenses); // set progress to actual total
            if (mq.Progress >= mq.Quest.Target)
            {
                mq.IsCompleted = true;
                mq.CompletedAt = DateTime.UtcNow;
            }

            await _memberQuestRepository.UpdateAsync(mq);
        }
    }

}