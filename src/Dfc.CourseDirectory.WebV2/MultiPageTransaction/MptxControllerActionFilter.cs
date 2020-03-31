using System.Linq;
using System.Reflection;
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

            if (mptxControllerClosedTypes.Count == 1)
            {
                var instanceContextProvider = context.HttpContext.RequestServices
                    .GetRequiredService<MptxInstanceContextProvider>();

                var instance = instanceContextProvider.GetContext();

                var setInstanceMethod = mptxControllerClosedTypes.Single().GetProperty("Flow").SetMethod;
                setInstanceMethod.Invoke(controller, new[] { instance });
            }
        }
    }
}
