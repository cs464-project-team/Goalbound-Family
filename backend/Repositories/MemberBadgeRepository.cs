using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GoalboundFamily.Api.Repositories;

public class MemberBadgeRepository : Repository<MemberBadge>, IMemberBadgeRepository
{
    public MemberBadgeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> ExistsAsync(Guid memberId, Guid badgeId) =>
        await _dbSet.AnyAsync(mb => mb.HouseholdMemberId == memberId && mb.BadgeId == badgeId);

}