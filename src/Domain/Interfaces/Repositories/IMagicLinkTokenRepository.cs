using AgentService.Domain.Entities;

namespace AgentService.Domain.Interfaces.Repositories;

public interface IMagicLinkTokenRepository
{
    Task<MagicLinkToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task CreateAsync(MagicLinkToken token, CancellationToken ct = default);
    Task UpdateAsync(MagicLinkToken token, CancellationToken ct = default);
}
