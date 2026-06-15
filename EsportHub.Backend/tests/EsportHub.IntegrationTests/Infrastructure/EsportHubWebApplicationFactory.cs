using EsportHub.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EsportHub.IntegrationTests.Infrastructure;

public class EsportHubWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18-alpine").Build();

    Task IAsyncLifetime.InitializeAsync() => _dbContainer.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        Dispose();
        await _dbContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:EsportHubDb"] = _dbContainer.GetConnectionString()
            });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EsportHubDbContext>();
        await db.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""Tournaments"" CASCADE;");
    }

    public async Task<T> ExecuteDbContextAsync<T>(Func<EsportHubDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EsportHubDbContext>();
        return await action(db);
    }

    public async Task ExecuteDbContextAsync(Func<EsportHubDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EsportHubDbContext>();
        await action(db);
    }
}
