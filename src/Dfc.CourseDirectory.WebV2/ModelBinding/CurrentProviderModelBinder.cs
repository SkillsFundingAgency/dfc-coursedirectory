using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.ModelBinding
{
    public class CurrentProviderModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(ProviderInfo))
            {
                var providerInfoCache = context.Services.GetRequiredService<IProviderInfoCache>();
                return new CurrentProviderModelBinder(providerInfoCache);
            }

            return null;
        }
    }

    public class CurrentProviderModelBinder : IModelBinder
    {
        public const string RouteValueKey = "providerId";

        private readonly IProviderInfoCache _providerInfoCache;

        public CurrentProviderModelBinder(IProviderInfoCache providerInfoCache)
        {
            _providerInfoCache = providerInfoCache;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(ProviderInfo))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            // For Provider {Super}Users the provider comes from their identity token.
            // For Admin or Helpdesk users there should be a route value indicating the provider.
            // If Provider {Super}Users specify the route value it's ignored.

            var user = bindingContext.HttpContext.User;
            var role = user.FindFirst(ClaimTypes.Role).Value;

            Guid providerId;

            var routeProviderId = TryGetProviderIdFromRouteValues();

            if (role == RoleNames.Developer || role == RoleNames.Helpdesk)
            {
                if (!routeProviderId.HasValue)
                {
                    bindingContext.Result = ModelBindingResult.Failed();
                    return;
                }
                else
                {
                    providerId = routeProviderId.Value;
                }
            }
            else if (role == RoleNames.ProviderSuperUser || role == RoleNames.ProviderUser)
            {
                var usersOwnProviderId = Guid.Parse(user.FindFirst("ProviderId").Value);

                // Route param, if specified, must match user's own org
                if (routeProviderId.HasValue && routeProviderId.Value != usersOwnProviderId)
                {
                    bindingContext.ModelState.AddModelError(
                        bindingContext.FieldName,
                        new NotAuthorizedException(),
                        bindingContext.ModelMetadata);
                    bindingContext.Result = ModelBindingResult.Failed();
                    return;
                }

                providerId = usersOwnProviderId;
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var providerInfo = await _providerInfoCache.GetProviderInfo(providerId);
            if (providerInfo == null)
            {
                bindingContext.ModelState.AddModelError(
                    bindingContext.FieldName,
                    new ResourceDoesNotExistException(ResourceType.Provider, providerId),
                    bindingContext.ModelMetadata);
                bindingContext.Result = ModelBindingResult.Failed();
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Success(providerInfo);
                bindingContext.HttpContext.Features.Set(new CurrentProviderFeature(providerInfo));
            }

            Guid? TryGetProviderIdFromRouteValues()
            {
                var routeValueProvider = new RouteValueProvider(
                    BindingSource.Path,
                    bindingContext.ActionContext.RouteData.Values);

                var queryStringValueProvider = new QueryStringValueProvider(
                    BindingSource.Query,
                    bindingContext.HttpContext.Request.Query,
                    CultureInfo.InvariantCulture);

                var matches = routeValueProvider.GetValue(RouteValueKey).Values
                    .Concat(queryStringValueProvider.GetValue(RouteValueKey).Values)
                    .Distinct()
                    .ToList();

                if (matches.Count == 1)
                {
                    if (!Guid.TryParse(matches.Single(), out providerId))
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }

                return providerId;
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
