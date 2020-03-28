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
            var mptxActionAttribute = (context.ActionDescriptor as ControllerActionDescriptor)
                .MethodInfo
                .GetCustomAttribute<MptxActionAttribute>();

            if (mptxActionAttribute == null)
            {
                await next();
                return;
            }

            var request = context.HttpContext.Request;
            var ffiid = request.Query[Constants.InstanceIdQueryParameter];

            if (ffiid.Count > 0)
            {
                var flowName = mptxActionAttribute.FlowName;

                var mptxManager = context.HttpContext.RequestServices.GetRequiredService<MptxManager>();
                var instanceContext = mptxManager.GetInstance(ffiid);

                if (instanceContext != null && instanceContext.FlowName == flowName)
                {
                    var feature = new MptxInstanceContextFeature(instanceContext);
                    context.HttpContext.Features.Set(feature);
                }
                else
                {
                    context.Result = new BadRequestResult();
                    return;
                }
            }

            await next();            
        }
    }
}
