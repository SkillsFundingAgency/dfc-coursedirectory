using System.Collections.Generic;
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
        public const string InstanceIdQueryParameter = "ffiid";

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

            var stateProvider = context.HttpContext.RequestServices.GetRequiredService<IMptxStateProvider>();

            var request = context.HttpContext.Request;

            var flowName = mptxActionAttribute.FlowName;
            var startsAttribute = mptxActionAttribute as StartsMptxAttribute;
            var canStartFlow = startsAttribute != null;

            var ffiid = request.Query[InstanceIdQueryParameter];
            MptxInstance mptxInstance = null;

            if (ffiid.Count == 0)
            {
                if (canStartFlow)
                {
                    // Begin a new flow
                    var newState = await CreateNewState(startsAttribute);
                    var contextItems = GetContextItemsFromCaptures(startsAttribute);
                    mptxInstance = stateProvider.CreateInstance(flowName, contextItems, newState);

                    // Redirect, appending the new instance ID
                    var currentUrlWithInstanceParam = QueryHelpers.AddQueryString(
                        request.Path + request.QueryString,
                        InstanceIdQueryParameter,
                        mptxInstance.InstanceId);

                    context.Result = new LocalRedirectResult(
                        currentUrlWithInstanceParam,
                        permanent: false,
                        preserveMethod: true);
                    return;
                }
            }
            else
            {
                mptxInstance = stateProvider.GetInstance(ffiid);
            }

            if (mptxInstance == null)
            {
                context.Result = new BadRequestResult();
                return;
            }
            else
            {
                // Verify this state is for this flow
                if (mptxInstance.FlowName != flowName)
                {
                    context.Result = new BadRequestResult();
                    return;
                }
            }

            // All good - add the feature
            var feature = new MptxInstanceFeature(mptxInstance);
            context.HttpContext.Features.Set(feature);

            await next();

            async Task<object> CreateNewState(StartsMptxAttribute startsAttribute)
            {
                var stateType = startsAttribute.StateType;
                var services = context.HttpContext.RequestServices;

                var state = ActivatorUtilities.CreateInstance(services, stateType);

                var initializerType = typeof(IInitializeMptxState<>).MakeGenericType(stateType);
                var initializer = services.GetService(initializerType);

                if (initializer != null)
                {
                    await (Task)initializerType.GetMethod("Initialize").Invoke(initializer, new object[] { state });
                }

                return state;
            }

            IReadOnlyDictionary<string, object> GetContextItemsFromCaptures(StartsMptxAttribute startsAttribute)
            {
                var dict = new Dictionary<string, object>();

                var captures = startsAttribute.CapturesQueryParams;

                foreach (var capture in captures)
                {
                    var val = (string)request.Query[capture];
                    dict.Add(capture, val);
                }

                return dict;
            }
        }
    }
}
