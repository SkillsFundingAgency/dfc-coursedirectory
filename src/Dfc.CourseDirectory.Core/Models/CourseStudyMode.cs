﻿using System;

namespace Dfc.CourseDirectory.Core.Models
{
    public enum CourseStudyMode
    {
        FullTime = 1,
        PartTime = 2,
        Flexible = 3
    }

    public static class CourseStudyModeExtensions
    {
        public static string ToDescription(this CourseStudyMode studyMode) => studyMode switch
        {
            CourseStudyMode.FullTime => "Full-time",
            CourseStudyMode.PartTime => "Part-time",
            CourseStudyMode.Flexible => "Flexible",
            _ => throw new NotSupportedException($"Unknown study mode: '{studyMode}'.")
        };

        public static string ToDescription(this CourseStudyMode? studyMode) =>
            studyMode.HasValue ? studyMode.Value.ToDescription() : string.Empty;
    }
}
