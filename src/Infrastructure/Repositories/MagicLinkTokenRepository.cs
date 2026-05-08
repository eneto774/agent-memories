using AgentService.Domain.Entities;
using AgentService.Domain.Interfaces.Repositories;
using AgentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgentService.Infrastructure.Repositories;

public class MagicLinkTokenRepository(AppDbContext db) : IMagicLinkTokenRepository
{
    public Task<MagicLinkToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken ct = default
    ) =>
        db
            .MagicLinkTokens.Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task CreateAsync(MagicLinkToken token, CancellationToken ct = default)
    {
        db.MagicLinkTokens.Add(token);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(MagicLinkToken token, CancellationToken ct = default)
    {
        db.MagicLinkTokens.Update(token);
        await db.SaveChangesAsync(ct);
    }
}
