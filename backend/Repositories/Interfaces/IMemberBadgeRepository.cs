using GoalboundFamily.Api.Models;

namespace GoalboundFamily.Api.Repositories.Interfaces;

public interface IMemberBadgeRepository : IRepository<MemberBadge>
{
    Task<bool> ExistsAsync(Guid memberId, Guid badgeId);
}