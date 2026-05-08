namespace AgentService.Infrastructure.Configuration;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "agent-service";
    public string Audience { get; set; } = "agent-service-clients";
    public int ExpiryDays { get; set; } = 7;
}
