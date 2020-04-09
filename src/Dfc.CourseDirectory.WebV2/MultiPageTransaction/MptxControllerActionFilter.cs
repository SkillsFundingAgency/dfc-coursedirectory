using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxControllerActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller;

            var mptxControllerClosedTypes = IntrospectionExtensions.GetTypeInfo(controller.GetType())
                .ImplementedInterfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMptxController<>))
                .ToList();

            if (mptxControllerClosedTypes.Count == 0)
            {
                return;
            }

            var instanceContextProvider = context.HttpContext.RequestServices
                    .GetRequiredService<MptxInstanceContextProvider>();

            var instance = instanceContextProvider.GetContext();

            if (instance == null)
            {
                return;
            }

            // Look for an implementation of IMptxController<T> where T matches state type
            foreach (var t in mptxControllerClosedTypes)
            {
                var closedStateType = t.GetGenericArguments()[0];

                if (closedStateType == instance.StateType)
                {
                    var setInstanceMethod = t.GetProperty("Flow").SetMethod;
                    setInstanceMethod.Invoke(controller, new[] { instance });

                    return;
                }
            }

            // No implementation of IMptxController<T> for given state type - error
            context.Result = new StatusCodeResult(400);
        }
    }
}
