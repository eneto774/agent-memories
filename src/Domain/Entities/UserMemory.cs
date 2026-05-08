using AgentService.Domain.Common;

namespace AgentService.Domain.Entities;

public class UserMemory : BaseEntity
{
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string MemoryType { get; set; } = "fact";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
}
