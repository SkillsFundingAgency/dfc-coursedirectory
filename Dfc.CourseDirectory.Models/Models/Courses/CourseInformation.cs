using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class CourseInformation : ValueObject<CourseInformation>, ICourseInformation
    {
        public DateTime[] CourseDates { get; }
        public string StudyMode { get; }
        public string Attendance { get; }
        public string CourseID { get; }
        public string CourseURL { get; }
        public string Pattern { get; }
        public string Requirements { get; }

        public CourseInformation(
            DateTime[] courseDates,
            string studyMode,
            string attendance,
            string courseID,
            string courseURL,
            string pattern,
            string requirements)
        {
            Throw.IfNullOrEmpty(courseDates, nameof(courseDates));
            Throw.IfNullOrWhiteSpace(studyMode, nameof(studyMode));
            Throw.IfNullOrWhiteSpace(attendance, nameof(attendance));
            Throw.IfNullOrWhiteSpace(courseID, nameof(courseID));
            Throw.IfNullOrWhiteSpace(courseURL, nameof(courseURL));
            Throw.IfNullOrWhiteSpace(pattern, nameof(pattern));
            Throw.IfNullOrWhiteSpace(requirements, nameof(requirements));

            CourseDates = courseDates;
            StudyMode = studyMode;
            Attendance = attendance;
            CourseID = courseID;
            CourseURL = courseURL;
            Pattern = pattern;
            Requirements = requirements;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return CourseDates;
            yield return StudyMode;
            yield return Attendance;
            yield return CourseID;
            yield return CourseURL;
            yield return Pattern;
            yield return Requirements;
        }
    }
}