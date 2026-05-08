using AgentService.Domain.Entities;
using AgentService.Domain.Interfaces.Repositories;
using AgentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgentService.Infrastructure.Repositories;

public class ConversationRepository(AppDbContext db) : IConversationRepository
{
    public Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db
            .Conversations.Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IEnumerable<Conversation>> GetByUserIdAsync(
        Guid userId,
        int limit = 20,
        CancellationToken ct = default
    ) =>
        await db
            .Conversations.Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<Conversation> CreateAsync(
        Conversation conversation,
        CancellationToken ct = default
    )
    {
        db.Conversations.Add(conversation);
        await db.SaveChangesAsync(ct);
        return conversation;
    }

    public async Task AddMessageAsync(Message message, CancellationToken ct = default)
    {
        db.Messages.Add(message);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Message>> GetMessagesAsync(
        Guid conversationId,
        int limit = 50,
        CancellationToken ct = default
    ) =>
        await db
            .Messages.Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task TouchUpdatedAtAsync(Guid conversationId, CancellationToken ct = default)
    {
        await db
            .Conversations.Where(c => c.Id == conversationId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.UpdatedAt, DateTime.UtcNow), ct);
    }
}
