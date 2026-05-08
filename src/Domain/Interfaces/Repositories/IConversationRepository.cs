using AgentService.Domain.Entities;

namespace AgentService.Domain.Interfaces.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Conversation>> GetByUserIdAsync(Guid userId, int limit = 20, CancellationToken ct = default);
    Task<Conversation> CreateAsync(Conversation conversation, CancellationToken ct = default);
    Task AddMessageAsync(Message message, CancellationToken ct = default);
    Task<IEnumerable<Message>> GetMessagesAsync(Guid conversationId, int limit = 50, CancellationToken ct = default);
    Task TouchUpdatedAtAsync(Guid conversationId, CancellationToken ct = default);
}
