using Microsoft.AspNetCore.Routing;

namespace Dfc.CourseDirectory.WebV2
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder MapV2Hubs(this IEndpointRouteBuilder endpoints)
        {
            return endpoints;
        }
    }
}
