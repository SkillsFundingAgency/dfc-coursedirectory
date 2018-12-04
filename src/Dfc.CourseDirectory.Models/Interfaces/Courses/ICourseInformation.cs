using System;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICourseInformation
    {
        DateTime[] CourseDates { get; }
        string StudyMode { get; }
        string Attendance { get; }
        string CourseID { get; }
        string CourseURL { get; }
        string Pattern { get; }
        string Requirements { get; }
    }
}