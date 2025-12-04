using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GoalboundFamily.Api.Repositories;

public class MemberQuestRepository : Repository<MemberQuest>, IMemberQuestRepository
{
    public MemberQuestRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<MemberQuest>> GetByMemberIdAsync(Guid memberId)
    {
        return await _dbSet
            .Include(mq => mq.Quest)
            .Where(mq => mq.HouseholdMemberId == memberId)
            .ToListAsync();
    }

    public async Task<MemberQuest?> GetAsync(Guid memberId, Guid questId)
    {
        return await _dbSet
            .Include(mq => mq.Quest)
            .FirstOrDefaultAsync(mq =>
                mq.HouseholdMemberId == memberId &&
                mq.QuestId == questId
            );
    }

    /// <summary>
    /// Get all active quests for a member and household filtered by category
    /// </summary>
    public async Task<IEnumerable<MemberQuest>> GetActiveQuestsAsync(Guid memberId, string category)
    {
        return await _dbSet
            .Include(mq => mq.Quest) // include quest template
            .Where(mq => mq.HouseholdMemberId == memberId
                      && mq.Quest.Category == category
                      && mq.Status == "in progress") // only active quests
            .ToListAsync();
    }

}
