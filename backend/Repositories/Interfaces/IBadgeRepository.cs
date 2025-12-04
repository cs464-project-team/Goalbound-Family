using GoalboundFamily.Api.Models;

namespace GoalboundFamily.Api.Repositories.Interfaces;

public interface IBadgeRepository : IRepository<Badge>
{
    Task<Badge?> GetByNameAsync(string name);
}