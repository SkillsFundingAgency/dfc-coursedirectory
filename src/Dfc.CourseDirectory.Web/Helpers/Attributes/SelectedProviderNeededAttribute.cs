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
            var session = context.HttpContext.Session.Get("UKPRN");
                if (controller != null && session == null)
                {
                    context.Result = controller.RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
                }
            base.OnActionExecuting(context);
        }
    }
}
