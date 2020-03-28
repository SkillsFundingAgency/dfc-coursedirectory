using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
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

            var mptxManager = context.HttpContext.RequestServices.GetRequiredService<MptxManager>();

            var request = context.HttpContext.Request;

            var flowName = mptxActionAttribute.FlowName;
            var startsAttribute = mptxActionAttribute as StartsMptxAttribute;
            var canStartFlow = startsAttribute != null;

            var ffiid = request.Query[Constants.InstanceIdQueryParameter];
            MptxInstanceContext mptxInstanceContext = null;

            if (ffiid.Count == 0)
            {
                if (canStartFlow)
                {
                    mptxInstanceContext = await mptxManager.CreateInstance(
                        startsAttribute.FlowName,
                        startsAttribute.StateType,
                        request,
                        startsAttribute.CapturesQueryParams);

                    // Redirect, appending the new instance ID
                    var currentUrlWithInstanceParam = QueryHelpers.AddQueryString(
                        request.Path + request.QueryString,
                        Constants.InstanceIdQueryParameter,
                        mptxInstanceContext.InstanceId);

                    context.Result = new LocalRedirectResult(
                        currentUrlWithInstanceParam,
                        permanent: false,
                        preserveMethod: true);
                    return;
                }
            }
            else
            {
                mptxInstanceContext = mptxManager.GetInstance(ffiid);
            }

            if (mptxInstanceContext == null)
            {
                context.Result = new BadRequestResult();
                return;
            }
            else
            {
                // Verify this state is for this flow
                if (mptxInstanceContext.FlowName != flowName)
                {
                    context.Result = new BadRequestResult();
                    return;
                }
            }

            // All good - add the feature
            var feature = new MptxInstanceContextFeature(mptxInstanceContext);
            context.HttpContext.Features.Set(feature);

            await next();            
        }
    }
}
