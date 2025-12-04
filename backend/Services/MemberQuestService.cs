using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services.Interfaces;

namespace GoalboundFamily.Api.Services;

public class MemberQuestService : IMemberQuestService
{
    private readonly IMemberQuestRepository _repo;
    private readonly IHouseholdMemberRepository _memberRepo;
    private readonly IQuestRepository _questRepo;

    public MemberQuestService(
        IMemberQuestRepository repo, 
        IHouseholdMemberRepository memberRepo,
        IQuestRepository questRepo)
    {
        _repo = repo;
        _memberRepo = memberRepo;
        _questRepo = questRepo;
    }

    private static MemberQuestDto ToDto(MemberQuest mq)
    {
        return new MemberQuestDto
        {
            HouseholdMemberId = mq.HouseholdMemberId,
            QuestId = mq.QuestId,
            Status = mq.Status,
            Progress = mq.Progress,
            AssignedAt = mq.AssignedAt,
            StartTime = mq.StartTime,
            CompletedAt = mq.CompletedAt,
            ClaimedAt = mq.ClaimedAt,

            Title = mq.Quest?.Title ?? "",
            Description = mq.Quest?.Description ?? "",
            XpReward = mq.Quest?.XpReward ?? 0,
            Category = mq.Quest?.Category ?? "",
            Type = mq.Quest?.Type ?? "",
            Difficulty = mq.Quest?.Difficulty ?? "",
            Target = mq.Quest?.Target ?? 0
        };
    }

    public async Task<IEnumerable<MemberQuestDto>> GetQuestsForMemberAsync(Guid memberId)
    {
        var quests = await _repo.GetByMemberIdAsync(memberId);
        return quests.Select(ToDto);
    }

    public async Task<MemberQuestDto?> GetAsync(Guid memberId, Guid questId)
    {
        var mq = await _repo.GetAsync(memberId, questId);
        if (mq == null) return null;

        return ToDto(mq);
    }

    public async Task<MemberQuestDto> AssignQuestAsync(Guid memberId, Guid questId)
    {
        // Prevent duplicates
        if (await _repo.GetAsync(memberId, questId) != null)
            throw new InvalidOperationException("Quest already assigned.");

        var entity = new MemberQuest
        {
            HouseholdMemberId = memberId,
            QuestId = questId,
            Status = "in-progress",
            AssignedAt = DateTime.UtcNow,
            Progress = 0
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        return ToDto(entity);
    }

    public async Task<bool> UpdateProgressAsync(Guid memberId, Guid questId, int progress)
    {
        var mq = await _repo.GetAsync(memberId, questId);
        if (mq == null) return false;

        mq.Progress = progress;

        await _repo.UpdateAsync(mq);
        await _repo.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CompleteQuestAsync(Guid memberId, Guid questId)
    {
        var mq = await _repo.GetAsync(memberId, questId);
        if (mq == null) return false;

        mq.Status = "completed";
        mq.CompletedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(mq);
        await _repo.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ClaimQuestAsync(Guid memberId, Guid questId)
    {
        var mq = await _repo.GetAsync(memberId, questId);
        if (mq == null) return false;

        if (mq.Status != "completed")
            throw new InvalidOperationException("Quest must be completed first.");

        mq.Status = "claimed";
        mq.ClaimedAt = DateTime.UtcNow;

        // Add XP to household member
        var member = await _memberRepo.GetByIdAsync(memberId);

        if (member == null)
            throw new InvalidOperationException("Household member not found.");

        // Ensure the quest info is loaded (if not eager loaded)
        if (mq.Quest == null)
        {
            mq.Quest = await _questRepo.GetByIdAsync(questId);
            if (mq.Quest == null)
                throw new InvalidOperationException("Quest not found.");
        }

        member.Xp += mq.Quest.XpReward;

        // Update repo
        await _repo.UpdateAsync(mq);
        await _memberRepo.UpdateAsync(member);
        await _repo.SaveChangesAsync();

        return true;
    }
}
