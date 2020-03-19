using System;

namespace Dfc.CourseDirectory.WebV2
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateLocalUrlAttribute : Attribute
    {
    }
}
