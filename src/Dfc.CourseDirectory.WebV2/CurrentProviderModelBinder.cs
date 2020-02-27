using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2
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
        public const string QueryParameterName = "ukprn";

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
            // For Admin or Helpdesk users there should be a query param indicating the provider.
            // If Provider {Super}Users specify the query param it's ignored.

            var user = bindingContext.HttpContext.User;
            var role = user.FindFirst(ClaimTypes.Role).Value;

            int ukprn;

            var queryStringUkprn = TryGetUkprnFromQueryString();

            if (role == RoleNames.Developer || role == RoleNames.Helpdesk)
            {
                if (!queryStringUkprn.HasValue)
                {
                    bindingContext.Result = ModelBindingResult.Failed();
                    return;
                }
                else
                {
                    ukprn = queryStringUkprn.Value;
                }
            }
            else if (role == RoleNames.ProviderSuperUser || role == RoleNames.ProviderUser)
            {
                var usersOwnUkprn = int.Parse(user.FindFirst("UKPRN").Value);

                // Query param, if specified, must match user's own org
                if (queryStringUkprn.HasValue && queryStringUkprn.Value != usersOwnUkprn)
                {
                    bindingContext.ModelState.AddModelError(
                        bindingContext.FieldName,
                        new NotAuthorizedException(),
                        bindingContext.ModelMetadata);
                    bindingContext.Result = ModelBindingResult.Failed();
                    return;
                }

                ukprn = usersOwnUkprn;
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var providerInfo = await _providerInfoCache.GetProviderInfo(ukprn);
            if (providerInfo == null)
            {
                bindingContext.ModelState.AddModelError(
                    bindingContext.FieldName,
                    new ResourceDoesNotExistException(ResourceType.Provider),
                    bindingContext.ModelMetadata);
                bindingContext.Result = ModelBindingResult.Failed();
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Success(providerInfo);
            }

            int? TryGetUkprnFromQueryString()
            {
                var valueProvider = new QueryStringValueProvider(
                    BindingSource.Query,
                    bindingContext.HttpContext.Request.Query,
                    CultureInfo.InvariantCulture);

                var specifiedProviderIdBindingResult = valueProvider.GetValue(QueryParameterName);

                if (specifiedProviderIdBindingResult.Length == 0)
                {
                    return null;
                }
                else
                {
                    if (!int.TryParse(specifiedProviderIdBindingResult.FirstValue, out ukprn))
                    {
                        return null;
                    }
                }

                return ukprn;
            }
        }
    }
}
