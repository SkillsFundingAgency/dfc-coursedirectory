using Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Dfc.CourseDirectory.WebV2
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder MapV2Hubs(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHub<VenuesDataManagementHub>("/data-upload/venues/hub");

            return endpoints;
        }
    }
}
