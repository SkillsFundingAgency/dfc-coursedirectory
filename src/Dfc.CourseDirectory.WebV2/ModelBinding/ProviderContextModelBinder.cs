using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.ModelBinding
{
    public class ProviderContextModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(ProviderContext))
            {
                return new CurrentProviderModelBinder();
            }

            return null;
        }
    }

    public class CurrentProviderModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(ProviderContext))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            var providerContextProvider = bindingContext.HttpContext.RequestServices.GetRequiredService<IProviderContextProvider>();
            var providerInfo = providerContextProvider.GetProviderContext();

            if (providerInfo == null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Success(providerInfo);
            }

            return Task.CompletedTask;
        }
    }
}
