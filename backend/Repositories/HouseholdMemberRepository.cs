using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GoalboundFamily.Api.Repositories;

public class HouseholdMemberRepository
    : Repository<HouseholdMember>, IHouseholdMemberRepository
{
    public HouseholdMemberRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> IsUserInHouseholdAsync(Guid userId, Guid householdId)
    {
        return await _dbSet.AnyAsync(m => m.UserId == userId && m.HouseholdId == householdId);
    }

    public async Task<IEnumerable<HouseholdMember>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(m => m.Household)
            .Where(m => m.UserId == userId)
            .ToListAsync();
    }
}