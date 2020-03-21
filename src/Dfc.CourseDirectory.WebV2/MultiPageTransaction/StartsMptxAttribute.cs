using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class StartsMptxAttribute : MptxActionAttribute
    {
        public StartsMptxAttribute(
            string flowName,
            params string[] capturesQueryParams)
            : base(flowName)
        {
            CapturesQueryParams = capturesQueryParams;
        }

        public IReadOnlyCollection<string> CapturesQueryParams { get; }
    }
}
