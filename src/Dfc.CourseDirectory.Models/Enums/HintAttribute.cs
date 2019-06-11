using System;

namespace Dfc.CourseDirectory.Models.Enums
{
    [AttributeUsage(AttributeTargets.All)]
    public class HintAttribute : Attribute
    {
        public HintAttribute(string hint)
        {
            Hint = hint;
        }

        public string Hint { get; set; }
    }
}
