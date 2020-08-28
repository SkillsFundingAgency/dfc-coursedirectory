using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class ProviderContextResourceFilter : IAsyncResourceFilter
    {
        public const string RouteValueKey = "providerId";

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var providerInfoCache = context.HttpContext.RequestServices.GetRequiredService<IProviderInfoCache>();
            var currentUserProvider = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserProvider>();
            var providerContextProvider = context.HttpContext.RequestServices.GetRequiredService<IProviderContextProvider>();

            await TryAssignFeature();

            if (context.Result == null)
            {
                await next();
            }

            async Task TryAssignFeature()
            {
                // For Provider {Super}Users the provider comes from their identity token.
                // For Admin or Helpdesk users there should be a route value indicating the provider.
                // If Provider {Super}Users specify the route value it's ignored.

                var user = currentUserProvider.GetCurrentUser();

                if (user == null)
                {
                    return;
                }

                Guid providerId;

                var routeProviderId = TryGetProviderIdFromRequest();

                if (user.IsDeveloper || user.IsHelpdesk)
                {
                    if (!routeProviderId.HasValue)
                    {
                        return;
                    }
                    else
                    {
                        providerId = routeProviderId.Value;
                    }
                }
                else // user.IsProvider == true
                {
                    var usersOwnProviderId = user.CurrentProviderId.Value;

                    // Route param, if specified, must match user's own org
                    if (routeProviderId.HasValue && routeProviderId.Value != usersOwnProviderId)
                    {
                        context.Result = new ForbidResult();
                        return;
                    }

                    providerId = usersOwnProviderId;
                }

                var providerInfo = await providerInfoCache.GetProviderInfo(providerId);
                if (providerInfo != null)
                {
                    var providerContext = new ProviderContext(providerInfo);
                    providerContextProvider.SetProviderContext(providerContext);
                }
            }

            Guid? TryGetProviderIdFromRequest()
            {
                var routeValueProvider = new RouteValueProvider(
                    BindingSource.Path,
                    context.RouteData.Values);

                var queryStringValueProvider = new QueryStringValueProvider(
                    BindingSource.Query,
                    context.HttpContext.Request.Query,
                    CultureInfo.InvariantCulture);

                var matches = routeValueProvider.GetValue(RouteValueKey).Values
                    .Concat(queryStringValueProvider.GetValue(RouteValueKey).Values)
                    .Distinct()
                    .ToList();

                if (matches.Count == 1 && Guid.TryParse(matches.Single(), out var providerId))
                {
                    return providerId;
                }

                return null;
            }
        }
    }
}
