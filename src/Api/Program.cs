using AgentService.Api;
using AgentService.Application;
using AgentService.Infrastructure;
using AgentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructureServices();
builder.AddApplicationServices();
builder.Services.AddWebServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseWebServices();
app.Run();

namespace AgentService.Api
{
    public partial class Program { }
}
