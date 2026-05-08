using AgentService.Domain.Entities;
using AgentService.Domain.Interfaces.Repositories;
using AgentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgentService.Infrastructure.Repositories;

public class UserMemoryRepository(AppDbContext db) : IUserMemoryRepository
{
    public async Task<UserMemory> CreateAsync(UserMemory memory, CancellationToken ct = default)
    {
        db.UserMemories.Add(memory);
        await db.SaveChangesAsync(ct);
        return memory;
    }

    public async Task<IEnumerable<UserMemory>> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default
    ) =>
        await db
            .UserMemories.Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
}
