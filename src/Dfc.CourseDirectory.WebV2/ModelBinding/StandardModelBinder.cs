using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.ModelBinding
{
    public class StandardModelBinderProvider : IModelBinderProvider
    {
        public StandardModelBinderProvider()
        {
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(Standard))
            {
                var standardsAndFrameworksCache = context.Services.GetRequiredService<IStandardsAndFrameworksCache>();
                return new StandardModelBinder(standardsAndFrameworksCache);
            }
            else
            {
                return null;
            }
        }
    }

    public class StandardModelBinder : IModelBinder
    {
        private readonly IStandardsAndFrameworksCache _standardsAndFrameworksCache;

        public StandardModelBinder(IStandardsAndFrameworksCache standardsAndFrameworksCache)
        {
            _standardsAndFrameworksCache = standardsAndFrameworksCache;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(Standard))
            {
                return;
            }

            var modelName = bindingContext.ModelName;
            var valueProvider = bindingContext.ValueProvider;

            if (int.TryParse(valueProvider.GetValue("StandardCode").FirstValue, out var standardCode) &&
                int.TryParse(valueProvider.GetValue("Version").FirstValue, out var version))
            {
                var standard = await _standardsAndFrameworksCache.GetStandard(standardCode, version);

                bindingContext.Result = standard != null ?
                    ModelBindingResult.Success(standard) :
                    ModelBindingResult.Failed();
            }
            else
            {
                bindingContext.ModelState.AddModelError(modelName, "Failed to bind Standard.");
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
    }
}
