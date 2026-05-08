namespace AgentService.Domain.Interfaces.Services;

public interface IJwtTokenService
{
    string Generate(Guid userId, string email);
}
