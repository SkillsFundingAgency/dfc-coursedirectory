using System;

namespace Dfc.CourseDirectory.Core.Models
{
    public enum CourseType
    {
        EssentialSkills = 1,
        TLevels,
        HTQs,
        FreeCoursesForJobs,
        Multiply,
        SkillsBootcamp,
        GCEASLevel,
        GCEALevel,
        RegulatedQualificationFramework,
        VocationalRegulatedQualifications,
        Degree
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
                CourseType.FreeCoursesForJobs => "Free Courses for Jobs",                
                CourseType.Multiply => "Multiply",
                CourseType.SkillsBootcamp => "Skills Bootcamp",
                CourseType.GCEASLevel => "AS  Levels",
                CourseType.GCEALevel => "A  Levels",
                CourseType.RegulatedQualificationFramework => "Regulated Qualification Framework (RQF)",
                CourseType.VocationalRegulatedQualifications => "Vocational Regulated Qualifications (VRQ)",
                CourseType.Degree => "Degree",
                _ => throw new NotImplementedException($"Unknown value: '{courseType}'.")
            };
        }
    }
}
