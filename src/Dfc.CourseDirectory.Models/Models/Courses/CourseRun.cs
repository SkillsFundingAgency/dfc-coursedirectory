using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models.Venues;
using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Models.Courses
{
    public class CourseRun : ValueObject<CourseRun>, ICourseRun
    {
        public Venue Venue { get; }
        public string Price { get; }
        public string Duration { get; }
        public string StudyMode { get; }
        public string Attendance { get; }
        public Guid CourseID { get; }
        public string CourseURL { get; }
        public string Pattern { get; }
        public string Requirements { get; }

        public CourseRun(
            Venue venue,
            string price,
            string duration,
            string studyMode,
            string attendance,
            Guid courseID,
            string courseURL,
            string pattern,
            string requirements)
        {
            Throw.IfNull(venue, nameof(venue));
            Throw.IfNullOrWhiteSpace(price, nameof(price));
            Throw.IfNullOrWhiteSpace(duration, nameof(duration));
            Throw.IfNullOrWhiteSpace(studyMode, nameof(studyMode));
            Throw.IfNullOrWhiteSpace(attendance, nameof(attendance));
            Throw.IfNull(courseID, nameof(courseID));
            Throw.IfNullOrWhiteSpace(courseURL, nameof(courseURL));
            Throw.IfNullOrWhiteSpace(pattern, nameof(pattern));
            Throw.IfNullOrWhiteSpace(requirements, nameof(requirements));

            Venue = venue;
            Price = price;
            Duration = duration;
            StudyMode = studyMode;
            Attendance = attendance;
            CourseID = courseID;
            CourseURL = courseURL;
            Pattern = pattern;
            Requirements = requirements;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Venue;
            yield return Price;
            yield return Duration;
            yield return StudyMode;
            yield return Attendance;
            yield return CourseID;
            yield return CourseURL;
            yield return Pattern;
            yield return Requirements;
        }
    }
}