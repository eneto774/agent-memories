using AgentService.Domain.Interfaces.Services;
using AgentService.Infrastructure.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AgentService.Infrastructure.Services;

public class SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    : IEmailService
{
    private readonly EmailSettings _settings = settings.Value;

    public async Task SendMagicLinkAsync(
        string toEmail,
        string magicLinkUrl,
        CancellationToken ct = default
    )
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_settings.From));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Seu link de acesso";

        var body = new BodyBuilder
        {
            HtmlBody = $"""
                <div style="font-family:sans-serif;max-width:480px;margin:0 auto;padding:32px">
                  <h2 style="color:#1a1a1a">Acesse sua conta</h2>
                  <p style="color:#555">Clique no botão abaixo para entrar. O link expira em 15 minutos.</p>
                  <a href="{magicLinkUrl}"
                     style="display:inline-block;margin-top:16px;padding:12px 24px;background:#4f46e5;
                            color:#fff;text-decoration:none;border-radius:8px;font-weight:600">
                    Entrar agora
                  </a>
                  <p style="margin-top:24px;font-size:12px;color:#999">
                    Se você não solicitou este link, ignore este email.
                  </p>
                </div>
                """,
        };

        message.Body = body.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _settings.SmtpHost,
            _settings.SmtpPort,
            _settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto,
            ct
        );
        await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        logger.LogInformation("Magic link sent to {Email}", toEmail);
    }
}
