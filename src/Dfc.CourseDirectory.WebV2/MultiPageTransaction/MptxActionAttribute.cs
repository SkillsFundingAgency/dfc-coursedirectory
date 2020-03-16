using System;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MptxActionAttribute : Attribute
    {
        public MptxActionAttribute(string flowName)
        {
            FlowName = flowName;
        }

        public string FlowName { get; }
    }
}
