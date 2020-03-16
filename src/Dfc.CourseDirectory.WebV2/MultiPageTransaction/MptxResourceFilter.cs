using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxResourceFilter : IResourceFilter
    {
        public const string InstanceIdQueryParameter = "ffiid";

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var mptxActionAttribute = (context.ActionDescriptor as ControllerActionDescriptor)
                .MethodInfo
                .GetCustomAttribute<MptxActionAttribute>();

            if (mptxActionAttribute == null)
            {
                return;
            }

            var stateProvider = context.HttpContext.RequestServices.GetRequiredService<IMptxStateProvider>();

            var request = context.HttpContext.Request;

            var flowName = mptxActionAttribute.FlowName;
            var canStartFlow = mptxActionAttribute is StartsMptxAttribute;

            var ffiid = request.Query[InstanceIdQueryParameter];
            MptxInstance mptxInstance = null;

            if (ffiid.Count == 0)
            {
                if (canStartFlow)
                {
                    // Begin a new flow
                    var contextItems = GetContextItemsFromCaptures();
                    mptxInstance = stateProvider.CreateInstance(flowName, contextItems);

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

            IReadOnlyDictionary<string, object> GetContextItemsFromCaptures()
            {
                var dict = new Dictionary<string, object>();

                var captures = (mptxActionAttribute as StartsMptxAttribute).CapturesQueryParams;

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
