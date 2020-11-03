using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseSearchResult : ICourseSearchResult
    {
        public IEnumerable<ICourseSearchOuterGrouping> Value { get; set; }

        public CourseSearchResult(
            IEnumerable<IEnumerable<IEnumerable<Course>>> courses)
        {
            Throw.IfNull(courses, nameof(courses));

            Value = courses.Select(c => new CourseSearchOuterGrouping(c));
        }

        public CourseSearchResult(
            IEnumerable<ICourseSearchOuterGrouping> courses)
        {
            Throw.IfNull(courses, nameof(courses));

            Value = courses.Select(c => new CourseSearchOuterGrouping(c.Value, c.Level));
        }
    }

    public class CourseSearchOuterGrouping : ICourseSearchOuterGrouping
    {
        public string QualType { get; set; }
        public string Level { get; set; }
        public IEnumerable<ICourseSearchInnerGrouping> Value { get; set; }

        public CourseSearchOuterGrouping(
            IEnumerable<ICourseSearchInnerGrouping> courses, 
            string level)
        {
            Throw.IfNullOrEmpty(level, nameof(level));
            Throw.IfNull(courses, nameof(courses));

            //QualType = qualType;
            Level = level;
            Value = courses.Select(c => new CourseSearchInnerResultGrouping(c.LARSRef));
        }

        public CourseSearchOuterGrouping(
            IEnumerable<IEnumerable<Course>> courses)
            //bool PopulateChildren = true)
        {
            Throw.IfNull(courses, nameof(courses));

            Level = courses?.FirstOrDefault()?.FirstOrDefault()?.NotionalNVQLevelv2;
            Value = courses.Select(c => new CourseSearchInnerResultGrouping(c));
        }
    }

    public class CourseSearchInnerResultGrouping : ICourseSearchInnerGrouping
    {
        public string LARSRef { get; set; }
        public IEnumerable<Course> Value { get; set; }

        public CourseSearchInnerResultGrouping(string larsRef)
        {
            Throw.IfNullOrEmpty(larsRef, nameof(larsRef));

            LARSRef = larsRef;
            Value = new List<Course>();
        }

        public CourseSearchInnerResultGrouping(
            IEnumerable<Course> value)
        {
            Throw.IfNull(value, nameof(value));

            Value = value;
            LARSRef = value?.FirstOrDefault()?.LearnAimRef;
        }
    }
}
