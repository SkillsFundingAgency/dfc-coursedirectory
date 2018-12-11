using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Venues;
using System;

namespace Dfc.CourseDirectory.Models.Interfaces.Courses
{
    public interface ICourseRun
    {
        Venue Venue { get; }
        string Price { get; }
        string Duration { get; }
        string StudyMode { get; }
        string Attendance { get; }
        Guid CourseID { get; }
        string CourseURL { get; }
        string Pattern { get; }
        string Requirements { get; }

    }
}