using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Configuration;

public static class StartupConfiguration
{
    extension(WebApplication app)
    {
        public void ConfigureStartup()
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EsportHubDbContext>();
            db.Database.Migrate();
        }
    }
}
