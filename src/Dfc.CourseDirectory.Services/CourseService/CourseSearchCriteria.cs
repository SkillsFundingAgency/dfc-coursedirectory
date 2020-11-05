using System;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseSearchCriteria
    {
        public int? UKPRN { get; set; }

        public CourseSearchCriteria(int? UKPRNvalue)
        {
            if (UKPRNvalue == null)
            {
                throw new ArgumentNullException(nameof(UKPRNvalue));
            }

            UKPRN = UKPRNvalue;
        }
    }
}
