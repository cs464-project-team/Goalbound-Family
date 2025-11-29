using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GoalboundFamily.Api.Repositories;

public class HouseholdRepository : Repository<Household>, IHouseholdRepository
{
    public HouseholdRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Household?> GetWithMembersAsync(Guid id)
    {
        return await _dbSet
            .Include(h => h.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(h => h.Id == id);
    }
}