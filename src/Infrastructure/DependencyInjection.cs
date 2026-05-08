using System.Text;
using AgentService.Domain.Interfaces.Services;
using AgentService.Infrastructure.Configuration;
using AgentService.Infrastructure.Data;
using AgentService.Infrastructure.Repositories;
using AgentService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Qdrant.Client;

namespace AgentService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var cfg = builder.Configuration;

        // EF Core + Postgres
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(cfg.GetConnectionString("DefaultConnection"))
        );

        // Configuration
        builder.Services.Configure<QdrantSettings>(cfg.GetSection("Qdrant"));
        builder.Services.Configure<JwtSettings>(cfg.GetSection("Jwt"));
        builder.Services.Configure<EmailSettings>(cfg.GetSection("Email"));

        // Qdrant
        builder.Services.AddSingleton(sp =>
        {
            var s = sp.GetRequiredService<IOptions<QdrantSettings>>().Value;
            return new QdrantClient(s.Host, s.Port, false, s.ApiKey);
        });

        // Repositories
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IMagicLinkTokenRepository, MagicLinkTokenRepository>();
        builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
        builder.Services.AddScoped<IUserMemoryRepository, UserMemoryRepository>();

        // Services
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
        builder.Services.AddScoped<IEmailService, SmtpEmailService>();
        builder.Services.AddSingleton<IVectorUserMemoryService, QdrantUserMemoryService>();

        // JWT Authentication
        var jwtSettings = cfg.GetSection("Jwt").Get<JwtSettings>()!;
        builder
            .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.MapInboundClaims = false; // keep original claim names (sub, email, jti)
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
                    ),
                    NameClaimType = "sub",
                };
            });

        builder.Services.AddAuthorization();
    }
}
