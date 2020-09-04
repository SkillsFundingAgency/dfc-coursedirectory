using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.Web.Helpers.Attributes
{
    public class SelectedProviderNeededAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Controller controller = context.Controller as Controller;
            int qsUKPRN;
            Int32.TryParse(context.HttpContext.Request.Query["UKPRN"].ToString(), out qsUKPRN);
            var session = context.HttpContext.Session.GetInt32("UKPRN");
            if (session == null)
            {
                context.HttpContext.Session.SetInt32("UKPRN", qsUKPRN);
            }

            var ukprnClaim = context.HttpContext.User.Claims.Where(x => x.Type == "UKPRN");
            if (controller != null && session == null && qsUKPRN == 0)
                {
                    var enumerable = ukprnClaim.ToArray();
                    if (enumerable.Any() && !string.IsNullOrEmpty(enumerable.FirstOrDefault()?.Value))
                    {
                        if(context.ActionArguments.TryGetValue("UKPRN", out object value))
                        {
                            var ukprn = value.ToString();
                            context.HttpContext.Session.SetInt32("UKPRN", int.Parse(ukprn));
                        }
                        else
                        {
                            context.HttpContext.Session.SetInt32("UKPRN", int.Parse(enumerable.FirstOrDefault()?.Value));
                        }
                        
                    }
                    else
                    {

                        context.Result = controller.RedirectToAction("Index", "Home",
                            new {errmsg = "Please select a Provider."});
                    }
                }
            base.OnActionExecuting(context);
        }
    }
}
