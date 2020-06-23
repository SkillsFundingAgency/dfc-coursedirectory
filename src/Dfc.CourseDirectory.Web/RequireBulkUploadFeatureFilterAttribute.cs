using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Web
{
    public sealed class RequireBulkUploadFeatureFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var featureFlagProvider = context.HttpContext.RequestServices.GetRequiredService<IFeatureFlagProvider>();

            if (!featureFlagProvider.HaveFeature(FeatureFlags.BulkUpload))
            {
                context.Result = new ViewResult()
                {
                    ViewName = "BulkUploadDisabled"
                };
            }
        }
    }
}
