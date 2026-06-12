using System.Net.Http.Headers;
using EsportHub.Infrastructure.Twitch;
using EsportHub.Middleware;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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

            services.AddMediatR(cfg =>
            {
                cfg.LicenseKey = builder.Configuration.GetRequiredSection("MediatR:LicenseKey").Value;
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            });

            services.AddMemoryCache();
            
            services.AddTransient<TwitchAuthHandler>();

            services.Configure<TwitchOptions>(builder.Configuration.GetSection(TwitchOptions.SectionName));

            services.AddHttpClient<IStreamingSiteService, TwitchService>((sp, client) =>
            {
                var config = sp.GetRequiredService<IOptions<TwitchOptions>>().Value;
                client.BaseAddress = new Uri("https://api.twitch.tv/helix/");
                client.DefaultRequestHeaders.Add("Client-Id", config.ClientId);
            })
            .AddHttpMessageHandler<TwitchAuthHandler>();
        }
    }
}
