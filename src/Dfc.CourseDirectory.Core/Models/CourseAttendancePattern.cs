using System;

namespace Dfc.CourseDirectory.Core.Models
{
    public enum CourseAttendancePattern
    {
        Daytime = 1,
        Evening = 2,
        Weekend = 3,
        DayOrBlockRelease = 4
    }

    public static class CourseAttendancePatternExtensions
    {
        public static string ToDescription(this CourseAttendancePattern attendancePattern) =>
            attendancePattern switch
            {
                CourseAttendancePattern.Daytime => "Daytime",
                CourseAttendancePattern.Evening => "Evening",
                CourseAttendancePattern.Weekend => "Weekend",
                CourseAttendancePattern.DayOrBlockRelease => "Day/Block Release",
                _ => throw new NotSupportedException($"Unknown attendance pattern: '{attendancePattern}'.")
            };

        public static string ToDescription(this CourseAttendancePattern? attendancePattern) =>
            attendancePattern.HasValue ? attendancePattern.Value.ToDescription() : string.Empty;
    }
}
