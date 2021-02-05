using Dfc.CourseDirectory.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class RequireFeatureFlagAttribute : ActionFilterAttribute
    {
        public RequireFeatureFlagAttribute(string feature)
        {
            Feature = feature;
        }

        public string Feature { get; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var featureFlagProvider = context.HttpContext.RequestServices.GetRequiredService<IFeatureFlagProvider>();
            if (!featureFlagProvider.HaveFeature(Feature))
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}
