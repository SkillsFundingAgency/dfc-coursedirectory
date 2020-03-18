﻿using System;
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
    public class CurrentProviderResourceFilter : IAsyncResourceFilter
    {
        public const string RouteValueKey = "providerId";

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            await TryAssignFeature();

            if (context.Result == null)
            {
                await next();
            }

            async Task TryAssignFeature()
            {
                var providerInfoCache = context.HttpContext.RequestServices.GetRequiredService<IProviderInfoCache>();
                var currentUserProvider = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserProvider>();

                // For Provider {Super}Users the provider comes from their identity token.
                // For Admin or Helpdesk users there should be a route value indicating the provider.
                // If Provider {Super}Users specify the route value it's ignored.

                var user = currentUserProvider.GetCurrentUser();

                if (user == null)
                {
                    return;
                }

                Guid providerId;

                var routeProviderId = TryGetProviderIdFromRouteValues();

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
                    var usersOwnProviderId = user.ProviderId.Value;

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
                    context.HttpContext.Features.Set(new CurrentProviderFeature(providerInfo));
                }
            }

            Guid? TryGetProviderIdFromRouteValues()
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

                if (matches.Count == 1)
                {
                    if (Guid.TryParse(matches.Single(), out var providerId))
                    {
                        return providerId;
                    }
                }

                return null;
            }
        }
    }

    public class CurrentProviderFeature
    {
        public CurrentProviderFeature(ProviderInfo providerInfo)
        {
            ProviderInfo = providerInfo;
        }

        public ProviderInfo ProviderInfo { get; }
    }
}
