using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class StartsMptxAttribute : MptxActionAttribute
    {
        public StartsMptxAttribute(
            string flowName,
            Type stateType,
            params string[] capturesQueryParams)
            : base(flowName)
        {
            CapturesQueryParams = capturesQueryParams;
            StateType = stateType ?? throw new ArgumentNullException(nameof(stateType));
        }

        public IReadOnlyCollection<string> CapturesQueryParams { get; }

        public Type StateType { get; }
    }
}
