using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfc.CourseDirectory.WebV2.ModelBinding
{
    public class CurrentProviderModelBinderProvider : IModelBinderProvider
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
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(ProviderInfo))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            var feature = bindingContext.HttpContext.Features.Get<ProviderContextFeature>();

            if (feature == null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Success(feature.ProviderInfo);
            }

            return Task.CompletedTask;
        }
    }
}
