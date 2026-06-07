using System.Reflection;
using EsportHub.Endpoints;
using EsportHub.Endpoints.Filters;

namespace EsportHub.Configuration;

public static class EndpointsConfiguration
{
    extension(WebApplication app)
    {
        public void MapEndpoints()
        {
            var api = app.MapGroup("/api");
            api.AddEndpointFilter<ArdalisResultMapper>();
            api.DisableAntiforgery();

            var endpointGroups = Assembly.GetExecutingAssembly()
                .DefinedTypes
                .Where(t => t.IsAssignableTo(typeof(IEndpointGroup)) && !t.IsInterface && !t.IsAbstract);

            foreach (var group in endpointGroups)
            {
                var instance = Activator.CreateInstance(group) as IEndpointGroup;
                instance?.MapEndpoints(api);
            }
        }
    }
}
