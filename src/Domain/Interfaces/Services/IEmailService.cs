namespace AgentService.Domain.Interfaces.Services;

public interface IEmailService
{
    Task SendMagicLinkAsync(string toEmail, string magicLinkUrl, CancellationToken ct = default);
}
