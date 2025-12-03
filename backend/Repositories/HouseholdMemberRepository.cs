using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
            .Include(m => m.User) 
            .Include(m => m.Household)
            .ThenInclude(h => h.Members)
            .Where(m => m.UserId == userId)
            .ToListAsync();
    }

    public async Task<HouseholdMember?> GetByUserAndHouseholdAsync(Guid userId, Guid householdId)
    {
        return await _dbSet
            .Include(m => m.User)                
            .Include(m => m.Household)
            .FirstOrDefaultAsync(m => m.UserId == userId && m.HouseholdId == householdId);
    }

    public async Task<IEnumerable<HouseholdMember>> FindAsync(Expression<Func<HouseholdMember, bool>> predicate)
    {
        return await _dbSet
            .Include(m => m.User)                // Include User info
            .Include(m => m.Household)
            .Where(predicate)
            .ToListAsync();
    }
}