using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseSearchResult
    {
        public IEnumerable<CourseSearchOuterGrouping> Value { get; set; }

        public CourseSearchResult(
            IEnumerable<IEnumerable<IEnumerable<Course>>> courses)
        {
            Throw.IfNull(courses, nameof(courses));

            Value = courses.Select(c => new CourseSearchOuterGrouping(c));
        }

        public CourseSearchResult(
            IEnumerable<CourseSearchOuterGrouping> courses)
        {
            Throw.IfNull(courses, nameof(courses));

            Value = courses.Select(c => new CourseSearchOuterGrouping(c.Value, c.Level));
        }
    }

    public class CourseSearchOuterGrouping
    {
        public string QualType { get; set; }
        public string Level { get; set; }
        public IEnumerable<CourseSearchInnerGrouping> Value { get; set; }

        public CourseSearchOuterGrouping(
            IEnumerable<CourseSearchInnerGrouping> courses, 
            string level)
        {
            Throw.IfNullOrEmpty(level, nameof(level));
            Throw.IfNull(courses, nameof(courses));

            Level = level;
            Value = courses.Select(c => new CourseSearchInnerGrouping(c.LARSRef));
        }

        public CourseSearchOuterGrouping(
            IEnumerable<IEnumerable<Course>> courses)
        {
            Throw.IfNull(courses, nameof(courses));

            Level = courses?.FirstOrDefault()?.FirstOrDefault()?.NotionalNVQLevelv2;
            Value = courses.Select(c => new CourseSearchInnerGrouping(c));
        }
    }

    public class CourseSearchInnerGrouping
    {
        public string LARSRef { get; set; }
        public IEnumerable<Course> Value { get; set; }

        public CourseSearchInnerGrouping(string larsRef)
        {
            Throw.IfNullOrEmpty(larsRef, nameof(larsRef));

            LARSRef = larsRef;
            Value = new List<Course>();
        }

        public CourseSearchInnerGrouping(
            IEnumerable<Course> value)
        {
            Throw.IfNull(value, nameof(value));

            Value = value;
            LARSRef = value?.FirstOrDefault()?.LearnAimRef;
        }
    }
}
