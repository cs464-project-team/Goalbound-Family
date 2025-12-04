using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GoalboundFamily.Api.Repositories;

public class BadgeRepository : Repository<Badge>, IBadgeRepository
{
    public BadgeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Badge?> GetByNameAsync(string name) =>
        await _dbSet.FirstOrDefaultAsync(b => b.Name == name);

}