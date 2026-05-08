namespace AgentService.Domain.Interfaces.Services;

public interface IAuthService
{
    Task RequestMagicLinkAsync(string email, CancellationToken ct = default);
    Task<string> VerifyMagicLinkAsync(string token, CancellationToken ct = default);
}
