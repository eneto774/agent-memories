using AgentService.Domain.Common;

namespace AgentService.Domain.Entities;

public class Message : BaseEntity
{
    public Guid ConversationId { get; set; }
    public string Role { get; set; } = string.Empty; // "user" | "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Conversation Conversation { get; set; } = null!;
}
