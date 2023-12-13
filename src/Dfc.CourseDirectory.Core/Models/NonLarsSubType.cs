using System;
using System.Linq;

namespace Dfc.CourseDirectory.Core.Models
{
    [Flags]
    public enum NonLarsSubType
    {
        SkillsBootcamp = 1
    }

    public static class NonLarsSubTypeExtensions
    {
        public static string ToDescription(this NonLarsSubType nonLarsSubType) => nonLarsSubType switch
        {
            0 => "Undefined",
            NonLarsSubType.SkillsBootcamp => "Skills Bootcamp",
            _ => throw new NotSupportedException($"Unknown non lars sub type: '{nonLarsSubType}'.")
        };

    }
}
