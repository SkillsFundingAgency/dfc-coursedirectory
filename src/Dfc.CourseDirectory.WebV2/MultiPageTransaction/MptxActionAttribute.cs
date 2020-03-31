using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class MptxActionAttribute : Attribute, IActionConstraint
    {
        public MptxActionAttribute(string flowName)
        {
            FlowName = flowName ?? throw new ArgumentNullException(nameof(flowName));
        }

        public string FlowName { get; }

        public int Order => 0;

        public bool Accept(ActionConstraintContext context)
        {
            var request = context.RouteContext.HttpContext.Request;
            var gotInstanceId = request.Query[Constants.InstanceIdQueryParameter].Count > 0;

            return gotInstanceId;
        }
    }
}
