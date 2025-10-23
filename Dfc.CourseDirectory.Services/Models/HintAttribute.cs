using System;

namespace Dfc.CourseDirectory.Services.Models
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HintAttribute : Attribute
    {
        public HintAttribute(string hint)
        {
            Hint = hint;
        }

        public string Hint { get; set; }
    }
}
