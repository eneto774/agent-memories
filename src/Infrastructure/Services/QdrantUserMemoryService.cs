using AgentService.Domain.Interfaces.Services;
using AgentService.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace AgentService.Infrastructure.Services;

public class QdrantUserMemoryService(
    QdrantClient qdrantClient,
    IOptions<QdrantSettings> settings,
    ILogger<QdrantUserMemoryService> logger
) : IVectorUserMemoryService
{
    private readonly QdrantSettings _settings = settings.Value;
    private readonly HashSet<string> _ensuredCollections = [];

    private static string CollectionName(Guid userId) => $"user_memories_{userId:N}";

    public async Task<IEnumerable<string>> SearchMemoriesByVectorAsync(
        Guid userId,
        float[] queryVector,
        int limit = 5,
        CancellationToken ct = default
    )
    {
        var collection = CollectionName(userId);
        var exists = await EnsureCollectionExistsAsync(collection, ct);
        if (!exists)
            return [];

        var results = await qdrantClient.SearchAsync(
            collectionName: collection,
            vector: queryVector,
            limit: (ulong)limit,
            scoreThreshold: 0.6f,
            cancellationToken: ct
        );

        return results
            .Where(r => r.Payload.ContainsKey("content"))
            .Select(r => r.Payload["content"].StringValue)
            .ToList();
    }

    public async Task UpsertMemoryVectorAsync(
        Guid userId,
        Guid memoryId,
        float[] vector,
        string content,
        string memoryType,
        CancellationToken ct = default
    )
    {
        var collection = CollectionName(userId);
        await EnsureCollectionExistsAsync(collection, ct);

        var point = new PointStruct
        {
            Id = new PointId { Uuid = memoryId.ToString() },
            Vectors = vector,
        };
        point.Payload["content"] = content;
        point.Payload["type"] = memoryType;
        point.Payload["userId"] = userId.ToString();
        point.Payload["createdAt"] = DateTime.UtcNow.ToString("O");

        await qdrantClient.UpsertAsync(collection, [point], cancellationToken: ct);
        logger.LogInformation("Memory upserted for user {UserId}", userId);
    }

    private async Task<bool> EnsureCollectionExistsAsync(string collection, CancellationToken ct)
    {
        if (_ensuredCollections.Contains(collection))
            return true;

        try
        {
            var collections = await qdrantClient.ListCollectionsAsync(ct);
            if (!collections.Any(c => c == collection))
            {
                await qdrantClient.CreateCollectionAsync(
                    collection,
                    new VectorParams
                    {
                        Size = (ulong)_settings.VectorSize,
                        Distance = Distance.Cosine,
                    },
                    cancellationToken: ct
                );

                logger.LogInformation("Created Qdrant collection {Collection}", collection);
            }

            _ensuredCollections.Add(collection);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure Qdrant collection {Collection}", collection);
            return false;
        }
    }
}
