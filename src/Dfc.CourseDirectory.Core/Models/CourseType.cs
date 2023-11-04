using System;
using System.Linq;

namespace Dfc.CourseDirectory.Core.Models
{
    [Flags]
    public enum CourseType
    {
        EssentialSkills = 0,
        TLevels = 6,
        HTQs = 55,
        FreeCourseForJobs = 56,
        SkillsBootcamps = 62,
        Multiply = 63
    }

    public static class CourseTypeExtensions
    {
        public static string ToDescription(this CourseType? courseType) => GetCourseTypeDescription(courseType);        

        public static string ToDescription(this CourseType courseType) => GetCourseTypeDescription(courseType);

        private static string GetCourseTypeDescription(CourseType? courseType)
        {
            return courseType switch
            {
                null => "",
                CourseType.EssentialSkills => "Essential Skills",
                CourseType.TLevels => "T Levels",
                CourseType.HTQs => "HTQs",
                CourseType.FreeCourseForJobs => "Free Courses for Jobs",
                CourseType.SkillsBootcamps => "Skills Bootcamps",
                CourseType.Multiply => "Multiply",
                _ => throw new NotImplementedException($"Unknown value: '{courseType}'.")
            };
        }
    }
}
