
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using System.Collections.Generic;


namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseSearchCriteria : ValueObject<CourseSearchCriteria>, ICourseSearchCriteria
    {
        public int? UKPRN { get; set; }

        public CourseSearchCriteria(int? UKPRNvalue)
        {
            Throw.IfNull(UKPRNvalue, nameof(UKPRNvalue));
            UKPRN = UKPRNvalue;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return UKPRN;
        }
    }
}
