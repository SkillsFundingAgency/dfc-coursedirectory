using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class RequireValidModelStateAttribute : ActionFilterAttribute
    {
        public RequireValidModelStateAttribute()
        {
        }

        public RequireValidModelStateAttribute(string forKey)
        {
            ForKey = forKey ?? throw new ArgumentNullException(nameof(forKey));
        }

        public string ForKey { get; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (ForKey != null)
            {
                if (context.ModelState.TryGetValue(ForKey, out var entry) &&
                    entry.Errors?.Count > 0)
                {
                    context.Result = new BadRequestResult();
                }
            }
            else if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestResult();
            }
        }
    }
}
