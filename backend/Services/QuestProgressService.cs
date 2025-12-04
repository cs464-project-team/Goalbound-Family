using GoalboundFamily.Api.Events;
using GoalboundFamily.Api.Services.Interfaces;
using GoalboundFamily.Api.Repositories.Interfaces;

namespace GoalboundFamily.Api.Services;

public class QuestProgressService : IQuestProgressService
{
    private readonly IMemberQuestService _questService;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IReceiptRepository _receiptRepo;
    private readonly IHouseholdMemberRepository _householdMemberRepo;
    private readonly IMemberQuestRepository _memberQuestRepository;

    public QuestProgressService(IExpenseRepository expenseRepository, IMemberQuestService questService, IReceiptRepository receiptRepo, IHouseholdMemberRepository householdMemberRepo, IMemberQuestRepository memberQuestRepository)
    {
        _questService = questService;
        _expenseRepository = expenseRepository;
        _receiptRepo = receiptRepo;
        _householdMemberRepo = householdMemberRepo;
        _memberQuestRepository = memberQuestRepository;
    }

    public async Task HandleReceiptScanned(Guid userId, Guid householdId)
    {
        // 1. Get total receipts scanned
        int totalReceipts = await _receiptRepo.GetReceiptCountByUserAndHouseholdAsync(userId, householdId);

        HouseholdMember? householdMember = await _householdMemberRepo.GetByUserIdAsync(userId);
        if (householdMember == null)
        {
            throw new InvalidOperationException($"User {userId} is not a member of any household.");
        }

        // 2. Get all active quests for this member in "receipt" category
        var activeQuests = await _memberQuestRepository.GetActiveQuestsAsync(householdMember.Id, "receipt");

        // 3. Update progress
        foreach (var mq in activeQuests)
        {
            mq.Progress = Math.max(mq.Quest.Target, totalReceipts); // set progress to actual total
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

        HouseholdMember? householdMember = await _householdMemberRepo.GetByUserIdAsync(userId);
        if (householdMember == null)
        {
            throw new InvalidOperationException($"User {userId} is not a member of any household.");
        }

        // 2. Get all active quests for this member in "expense" category
        var activeQuests = await _memberQuestRepository.GetActiveQuestsAsync(householdMember.Id, "expense");

        // 3. Update progress
        foreach (var mq in activeQuests)
        {
            mq.Progress = Math.max(mq.Quest.Target, totalExpenses); // set progress to actual total
            if (mq.Progress >= mq.Quest.Target)
            {
                mq.IsCompleted = true;
                mq.CompletedAt = DateTime.UtcNow;
            }

            await _memberQuestRepository.UpdateAsync(mq);
        }
    }

}