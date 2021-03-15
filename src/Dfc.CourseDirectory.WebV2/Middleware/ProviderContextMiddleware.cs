using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Middleware
{
    public class ProviderContextMiddleware
    {
        public const string RouteValueKey = "providerId";

        private readonly RequestDelegate _next;

        public ProviderContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var providerInfoCache = context.RequestServices.GetRequiredService<IProviderInfoCache>();
            var currentUserProvider = context.RequestServices.GetRequiredService<ICurrentUserProvider>();
            var providerContextProvider = context.RequestServices.GetRequiredService<IProviderContextProvider>();

            bool runNext = true;

            await TryAssignFeature();

            if (runNext)
            {
                // As the layout includes links to pages that require the legacy context we need to always set this
                if (providerContextProvider.GetProviderContext() != null)
                {
                    providerContextProvider.AssignLegacyProviderContext();
                }

                await _next(context);
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

                bool isFallback = false;

                var requestProviderId = TryGetProviderIdFromRequest();

                if (!requestProviderId.HasValue)
                {
                    requestProviderId = await TryGetProviderIdFromLegacyContext();

                    if (requestProviderId.HasValue)
                    {
                        isFallback = true;
                    }
                }

                if (user.IsDeveloper || user.IsHelpdesk)
                {
                    if (!requestProviderId.HasValue)
                    {
                        return;
                    }
                    else
                    {
                        providerId = requestProviderId.Value;
                    }
                }
                else // user.IsProvider == true
                {
                    var usersOwnProviderId = user.CurrentProviderId.Value;

                    // Route param or session value, if specified, must match user's own org
                    if (requestProviderId.HasValue && requestProviderId.Value != usersOwnProviderId)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        runNext = false;
                        return;
                    }

                    providerId = usersOwnProviderId;
                    isFallback = false;
                }

                var providerInfo = await providerInfoCache.GetProviderInfo(providerId);
                if (providerInfo != null)
                {
                    var providerContext = new ProviderContext(providerInfo, isFallback);
                    providerContextProvider.SetProviderContext(providerContext);
                }
            }

            Guid? TryGetProviderIdFromRequest()
            {
                var routeValueProvider = new RouteValueProvider(
                    BindingSource.Path,
                    context.GetRouteData().Values);

                var queryStringValueProvider = new QueryStringValueProvider(
                    BindingSource.Query,
                    context.Request.Query,
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

            async Task<Guid?> TryGetProviderIdFromLegacyContext()
            {
                var ukprn = context.Session.GetInt32("UKPRN");

                return ukprn.HasValue ? await providerInfoCache.GetProviderIdForUkprn(ukprn.Value) : null;
            }
        }
    }
}
