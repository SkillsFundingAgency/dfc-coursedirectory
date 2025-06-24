using System;

namespace Dfc.CourseDirectory.Core.MultiPageTransaction
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class StartsMptxAttribute : Attribute
    {
    }
}
