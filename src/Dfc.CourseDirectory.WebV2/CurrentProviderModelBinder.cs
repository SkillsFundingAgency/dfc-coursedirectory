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
        private const string QueryParameterName = "ukprn";

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

            if (role == RoleNames.Developer || role == RoleNames.Helpdesk)
            {
                var valueProvider = new QueryStringValueProvider(
                    BindingSource.Query,
                    bindingContext.HttpContext.Request.Query,
                    CultureInfo.InvariantCulture);

                var specifiedProviderIdBindingResult = valueProvider.GetValue(QueryParameterName);

                if (specifiedProviderIdBindingResult.Length == 0)
                {
                    OnBindingFailed();
                    return;
                }
                else
                {
                    if (!int.TryParse(specifiedProviderIdBindingResult.FirstValue, out ukprn))
                    {
                        OnBindingFailed();
                        return;
                    }
                }
            }
            else if (role == RoleNames.ProviderSuperUser || role == RoleNames.ProviderUser)
            {
                ukprn = int.Parse(user.FindFirst("UKPRN").Value);
            }
            else
            {
                OnBindingFailed();
                return;
            }

            var providerInfo = await _providerInfoCache.GetProviderInfo(ukprn);
            bindingContext.Result = ModelBindingResult.Success(providerInfo);

            void OnBindingFailed()
            {
                bindingContext.ModelState.AddModelError(bindingContext.FieldName, "Failed to determine current provider.");
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
    }
}
