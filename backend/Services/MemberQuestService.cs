using GoalboundFamily.Api.DTOs;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services.Interfaces;

namespace GoalboundFamily.Api.Services;

public class MemberQuestService : IMemberQuestService
{
    private readonly IMemberQuestRepository _repo;

    public MemberQuestService(IMemberQuestRepository repo)
    {
        _repo = repo;
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

        await _repo.UpdateAsync(mq);
        await _repo.SaveChangesAsync();

        return true;
    }
}
