using AgentService.Application.Configuration;
using AgentService.Application.Services;
using AgentService.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AgentService.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<MagicLinkSettings>(builder.Configuration.GetSection("MagicLink"));
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IAgentChatService, AgentChatService>();
        builder.Services.AddScoped<IKernelFactory, KernelFactory>();
    }
}
