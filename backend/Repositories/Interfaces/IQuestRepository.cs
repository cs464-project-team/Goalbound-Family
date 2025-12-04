using GoalboundFamily.Api.Models;

namespace GoalboundFamily.Api.Repositories.Interfaces;

public interface IQuestRepository
{
    Task<Quest?> GetByIdAsync(Guid questId);
    Task<bool> ExistsAsync(Guid questId);
    Task<IEnumerable<Quest>> GetAllAsync();
}
