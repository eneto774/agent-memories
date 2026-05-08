using AgentService.Domain.Common;

namespace AgentService.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<MagicLinkToken> MagicLinkTokens { get; set; } = [];
    public ICollection<Conversation> Conversations { get; set; } = [];
    public ICollection<UserMemory> Memories { get; set; } = [];
}
