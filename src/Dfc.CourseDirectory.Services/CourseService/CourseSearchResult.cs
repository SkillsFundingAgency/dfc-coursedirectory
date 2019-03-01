
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;


namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseSearchResult : ValueObject<CourseSearchResult>, ICourseSearchResult
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

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    public class CourseSearchOuterGrouping : ValueObject<CourseSearchOuterGrouping>, ICourseSearchOuterGrouping
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

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Level;
            yield return Value;
        }
    }

    public class CourseSearchInnerResultGrouping : ValueObject<CourseSearchInnerResultGrouping>, ICourseSearchInnerGrouping
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

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return LARSRef;
            yield return Value;
        }
    }
}
