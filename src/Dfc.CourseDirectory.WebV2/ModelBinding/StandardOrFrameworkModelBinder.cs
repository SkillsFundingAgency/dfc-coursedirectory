using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.ModelBinding
{
    public class StandardOrFrameworkModelBinderProvider : IModelBinderProvider
    {
        public StandardOrFrameworkModelBinderProvider()
        {
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(StandardOrFramework))
            {
                var standardsAndFrameworksCache = context.Services.GetRequiredService<IStandardsAndFrameworksCache>();
                return new StandardOrFrameworkModelBinder(standardsAndFrameworksCache);
            }
            else
            {
                return null;
            }
        }
    }

    public class StandardOrFrameworkModelBinder : IModelBinder
    {
        private readonly IStandardsAndFrameworksCache _standardsAndFrameworksCache;

        public StandardOrFrameworkModelBinder(IStandardsAndFrameworksCache standardsAndFrameworksCache)
        {
            _standardsAndFrameworksCache = standardsAndFrameworksCache;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(StandardOrFramework))
            {
                return;
            }

            var modelName = bindingContext.ModelName;
            var valueProvider = bindingContext.ValueProvider;

            var type = valueProvider.GetValue("StandardOrFrameworkType").FirstValue ?? string.Empty;

            if (type.Equals("standard", StringComparison.OrdinalIgnoreCase))
            {
                await TryBindStandard();
            }
            else if (type.Equals("framework", StringComparison.OrdinalIgnoreCase))
            {
                await TryBindFramework();
            }
            else
            {
                bindingContext.ModelState.AddModelError(modelName, "No StandardOrFrameworkType found.");
                bindingContext.Result = ModelBindingResult.Failed();
            }

            async Task TryBindStandard()
            {
                if (int.TryParse(valueProvider.GetValue("StandardCode").FirstValue, out var standardCode) &&
                    int.TryParse(valueProvider.GetValue("Version").FirstValue, out var version))
                {
                    var standard = await _standardsAndFrameworksCache.GetStandard(standardCode, version);

                    bindingContext.Result = standard != null ?
                        ModelBindingResult.Success(new StandardOrFramework(standard)) :
                        ModelBindingResult.Failed();
                }
                else
                {
                    bindingContext.ModelState.AddModelError(modelName, "Failed to bind Standard.");
                    bindingContext.Result = ModelBindingResult.Failed();
                }
            }

            async Task TryBindFramework()
            {
                if (int.TryParse(valueProvider.GetValue("FrameworkCode").FirstValue, out var frameworkCode) &&
                    int.TryParse(valueProvider.GetValue("ProgType").FirstValue, out var progType) &&
                    int.TryParse(valueProvider.GetValue("PathwayCode").FirstValue, out var pathwayCode))
                {
                    var framework = await _standardsAndFrameworksCache.GetFramework(
                        frameworkCode,
                        progType,
                        pathwayCode);

                    bindingContext.Result = framework != null ?
                        ModelBindingResult.Success(new StandardOrFramework(framework)) :
                        ModelBindingResult.Failed();
                }
                else
                {
                    bindingContext.ModelState.AddModelError(modelName, "Failed to bind Framework.");
                    bindingContext.Result = ModelBindingResult.Failed();
                }
            }
        }
    }
}
