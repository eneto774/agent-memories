using AgentService.Domain.Common;

namespace AgentService.Domain.Entities;

public class Conversation : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = "Nova conversa";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = [];
}
