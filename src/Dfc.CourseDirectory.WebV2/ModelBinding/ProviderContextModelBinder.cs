using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.ModelBinding
{
    public class ProviderContextModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(ProviderInfo))
            {
                return new CurrentProviderModelBinder();
            }

            return null;
        }
    }

    public class CurrentProviderModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(ProviderInfo))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var providerContextProvider = bindingContext.HttpContext.RequestServices.GetRequiredService<IProviderContextProvider>();
            var providerInfo = await providerContextProvider.GetProviderContext();

            if (providerInfo == null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Success(providerInfo);
            }

            return;
        }
    }
}
