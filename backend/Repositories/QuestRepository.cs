using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.Models;
using GoalboundFamily.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GoalboundFamily.Api.Repositories;

public class QuestRepository : IQuestRepository
{
    private readonly ApplicationDbContext _context;

    public QuestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Quest?> GetByIdAsync(Guid questId)
    {
        return await _context.Quests
            .FirstOrDefaultAsync(q => q.Id == questId);
    }

    public async Task<bool> ExistsAsync(Guid questId)
    {
        return await _context.Quests
            .AnyAsync(q => q.Id == questId);
    }

    public async Task<IEnumerable<Quest>> GetAllAsync()
    {
        return await _context.Quests.ToListAsync();
    }
}
