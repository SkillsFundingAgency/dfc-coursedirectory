using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseSearchCriteria : ICourseSearchCriteria
    {
        public int? UKPRN { get; set; }

        public CourseSearchCriteria(int? UKPRNvalue)
        {
            Throw.IfNull(UKPRNvalue, nameof(UKPRNvalue));
            UKPRN = UKPRNvalue;
        }
    }
}
