using System.Security.Cryptography;
using AgentService.Application.Configuration;
using AgentService.Domain.Entities;
using AgentService.Domain.Interfaces.Repositories;
using AgentService.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgentService.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IMagicLinkTokenRepository tokenRepository,
    IEmailService emailService,
    IJwtTokenService jwtService,
    IOptions<MagicLinkSettings> magicLinkSettings,
    IOptions<EmailSettings> emailSettings,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly MagicLinkSettings _magicLinkSettings = magicLinkSettings.Value;
    private readonly EmailSettings _emailSettings = emailSettings.Value;

    public async Task RequestMagicLinkAsync(string email, CancellationToken ct = default)
    {
        email = email.ToLowerInvariant().Trim();

        var user = await userRepository.GetByEmailAsync(email, ct)
                   ?? await userRepository.CreateAsync(new User { Email = email }, ct);

        var rawToken = GenerateSecureToken();
        var tokenHash = HashToken(rawToken);

        await tokenRepository.CreateAsync(new MagicLinkToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_magicLinkSettings.ExpiryMinutes),
        }, ct);

        var magicUrl = $"{_emailSettings.MagicLinkBaseUrl}?token={rawToken}";
        await emailService.SendMagicLinkAsync(email, magicUrl, ct);

        logger.LogInformation("Magic link sent to {Email}", email);
    }

    public async Task<string> VerifyMagicLinkAsync(string rawToken, CancellationToken ct = default)
    {
        var tokenHash = HashToken(rawToken);
        var record = await tokenRepository.GetByTokenHashAsync(tokenHash, ct)
                     ?? throw new InvalidOperationException("Token not found.");

        if (record.IsUsed)
            throw new InvalidOperationException("Token has already been used.");
        if (record.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Token has expired.");

        record.IsUsed = true;
        await tokenRepository.UpdateAsync(record, ct);

        var jwt = jwtService.Generate(record.UserId, record.User.Email);
        logger.LogInformation("User {UserId} authenticated via magic link", record.UserId);

        return jwt;
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static string HashToken(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}
