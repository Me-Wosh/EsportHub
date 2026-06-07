using EsportHub.Middleware;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Configuration;

public static class ServicesConfiguration
{
    extension(IServiceCollection services)
    {
        public void AddServices(WebApplicationBuilder builder)
        {
            if (builder.Environment.IsProduction())
            {
                services.AddExceptionHandler<GlobalExceptionHandler>();
                services.AddProblemDetails();
            }

            services.AddOpenApi();
            services.AddValidation();
            services.AddHealthChecks();

            services.AddDbContext<EsportHubDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("EsportHubDb"));
            });
        }
    }
}
