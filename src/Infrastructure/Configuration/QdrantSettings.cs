namespace AgentService.Infrastructure.Configuration;

public class QdrantSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6334;
    public string? ApiKey { get; set; }
    public int VectorSize { get; set; } = 1536;
}
