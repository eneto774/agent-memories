using AgentService.Domain.Entities;

namespace AgentService.Domain.Interfaces.Repositories;

public interface IUserMemoryRepository
{
    Task<UserMemory> CreateAsync(UserMemory memory, CancellationToken ct = default);
    Task<IEnumerable<UserMemory>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
