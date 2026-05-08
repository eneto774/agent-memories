namespace AgentService.Domain.Dtos.Auth;

public record AuthTokenDto(string Token, string TokenType, DateTime ExpiresAt, Guid UserId, string Email);
