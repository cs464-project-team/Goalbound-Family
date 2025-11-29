using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GoalboundFamily.Api.Repositories;

public class InvitationRepository
    : Repository<Invitation>, IInvitationRepository
{
    public InvitationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Invitation?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .Include(i => i.Household)
            .Include(i => i.InvitedByUser)
            .FirstOrDefaultAsync(i => i.Token == token);
    }
}