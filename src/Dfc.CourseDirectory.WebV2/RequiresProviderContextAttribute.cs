using System;

namespace Dfc.CourseDirectory.WebV2
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RequiresProviderContextAttribute : Attribute
    {
    }
}
