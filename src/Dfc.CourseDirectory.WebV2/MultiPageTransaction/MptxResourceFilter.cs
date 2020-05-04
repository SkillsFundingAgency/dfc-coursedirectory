using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxResourceFilter : IAsyncResourceFilter
    {
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;

            var mptxActionAttribute = controllerActionDescriptor
                .MethodInfo
                .GetCustomAttribute<MptxActionAttribute>();

            var startsMptxAttribute = controllerActionDescriptor
                .MethodInfo
                .GetCustomAttribute<StartsMptxAttribute>();

            if (mptxActionAttribute == null && startsMptxAttribute == null)
            {
                await next();
                return;
            }

            var request = context.HttpContext.Request;
            var ffiid = request.Query[Constants.InstanceIdQueryParameter];

            if (ffiid.Count > 0)
            {
                var stateProvider = context.HttpContext.RequestServices.GetRequiredService<IMptxStateProvider>();
                var instance = stateProvider.GetInstance(ffiid);

                if (instance == null)
                {
                    context.Result = new ViewResult()
                    {
                        ViewName = "GenericError",
                        StatusCode = 404
                    };
                    return;
                }
                
                var feature = new MptxInstanceFeature(instance);
                context.HttpContext.Features.Set(feature);
            }

            await next();            
        }
    }
}
