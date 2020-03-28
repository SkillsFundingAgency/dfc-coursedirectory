using System;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class StartsMptxAttribute : MptxActionAttribute
    {
        public StartsMptxAttribute(string flowName, Type stateType)
            : base(flowName)
        {
            StateType = stateType ?? throw new ArgumentNullException(nameof(stateType));
        }

        public Type StateType { get; }
    }
}
