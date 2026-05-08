namespace AgentService.Domain.Interfaces.Services;

public interface IUserMemoryService
{
    Task<IEnumerable<string>> SearchMemoriesAsync(Guid userId, string query, int limit = 5, CancellationToken ct = default);
    Task SaveMemoryAsync(Guid userId, string content, string memoryType, CancellationToken ct = default);
}
