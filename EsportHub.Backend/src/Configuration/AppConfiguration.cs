namespace EsportHub.Configuration;

public static class AppConfiguration
{
    extension (WebApplication app)
    {
        public void ConfigureApp()
        {
            if (app.Environment.IsDevelopment())
                app.MapOpenApi();

            if (app.Environment.IsProduction())
                app.UseExceptionHandler();
            
            app.UseHttpsRedirection();
            app.MapHealthChecks("/healthz");
            app.MapEndpoints();
        }
    }
}
