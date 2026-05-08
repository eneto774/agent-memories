namespace AgentService.Domain.Interfaces.Services;

public interface IVectorUserMemoryService
{
    Task<IEnumerable<string>> SearchMemoriesByVectorAsync(Guid userId, float[] queryVector, int limit = 5, CancellationToken ct = default);
    Task UpsertMemoryVectorAsync(Guid userId, Guid memoryId, float[] vector, string content, string memoryType, CancellationToken ct = default);
}
