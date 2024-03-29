﻿using System;

namespace Dfc.CourseDirectory.Core.Models
{
    public enum CourseType
    {
        EssentialSkills = 1,
        TLevels,
        HTQs,
        FreeCoursesForJobs,
        Multiply,
        SkillsBootcamp
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
                _ => throw new NotImplementedException($"Unknown value: '{courseType}'.")
            };
        }
    }
}
