
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
            //IEnumerable<CourseSearchOuterGrouping> value)
            IEnumerable<IEnumerable<IEnumerable<Course>>> courses)
        {
            Throw.IfNull(courses, nameof(courses));

            Value = courses.Select(c => new CourseSearchOuterGrouping(c));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    public class CourseSearchOuterGrouping : ValueObject<CourseSearchOuterGrouping>, ICourseSearchOuterGrouping
    {
        public string QualType { get; set; }
        public IEnumerable<ICourseSearchInnerGrouping> Value { get; set; }

        public CourseSearchOuterGrouping(
            //IEnumerable<CourseInnerSearchResultGrouping> value)
            IEnumerable<IEnumerable<Course>> courses)
        {
            Throw.IfNullOrEmpty(courses, nameof(courses));
            //Throw.IfNullOrEmpty(value.FirstOrDefault()?.Value, nameof(value));

            ////List<List<CourseInnerSearchResultGrouping>> listOuter = new List<List<CourseInnerSearchResultGrouping>>();
            //List<CourseInnerSearchResultGrouping> list = new List<CourseInnerSearchResultGrouping>();

            //foreach (IEnumerable<IEnumerable<Course>> outerGroup in courses) {
            //    foreach (IEnumerable<Course> innerGroup in outerGroup) {
            //        list.Add(new CourseInnerSearchResultGrouping(innerGroup));
            //    }                
            //}
            
            Value = courses.Select(c => new CourseSearchInnerResultGrouping(c));
            QualType = courses?.FirstOrDefault()?.FirstOrDefault()?.QualificationType;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return QualType;
            yield return Value;
        }
    }

    public class CourseSearchInnerResultGrouping : ValueObject<CourseSearchInnerResultGrouping>, ICourseSearchInnerGrouping
    {
        public string LARSRef { get; set; }
        public IEnumerable<Course> Value { get; set; }

        public CourseSearchInnerResultGrouping(
            IEnumerable<Course> value)
        {
            Throw.IfNullOrEmpty(value, nameof(value));

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
