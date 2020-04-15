using System;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class StartsMptxAttribute : Attribute
    {
    }
}
