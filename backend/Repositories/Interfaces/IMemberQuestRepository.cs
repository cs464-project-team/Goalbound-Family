using GoalboundFamily.Api.Models;

namespace GoalboundFamily.Api.Repositories.Interfaces;

public interface IMemberQuestRepository : IRepository<MemberQuest>
{
    Task<IEnumerable<MemberQuest>> GetByMemberIdAsync(Guid memberId);
    Task<MemberQuest?> GetAsync(Guid memberId, Guid questId);
    Task<IEnumerable<MemberQuest>> GetActiveQuestsAsync(Guid memberId, string category);
}

