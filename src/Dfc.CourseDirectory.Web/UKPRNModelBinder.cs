using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfc.CourseDirectory.Web
{
    public class UKPRNModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(int))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            var ukprn = bindingContext.HttpContext.Session.GetInt32("UKPRN");
            
            if (ukprn.HasValue)
            {
                bindingContext.Result = ModelBindingResult.Success(ukprn.Value);
            }
            else
            {
                bindingContext.ModelState.AddModelError(bindingContext.FieldName, "UKPRN missing");
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }

    public class UKPRNAttribute : ModelBinderAttribute
    {
        public UKPRNAttribute()
        {
            BinderType = typeof(UKPRNModelBinder);
        }
    }

    public class RedirectOnMissingUKPRNActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var ukprnParams = context.ActionDescriptor.Parameters
                .Where(p => p.BindingInfo?.BinderType == typeof(UKPRNModelBinder));

            foreach (var p in ukprnParams)
            {
                if (context.ModelState[p.Name].Errors.Where(e => e?.ErrorMessage == "UKPRN missing").Any())
                {
                    context.Result = new RedirectToActionResult("Index", "Home", new { errmsg = "Please select a Provider." });

                    break;
                }
            }
        }
    }
}
