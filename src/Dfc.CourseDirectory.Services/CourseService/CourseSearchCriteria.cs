namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseSearchCriteria
    {
        public int? UKPRN { get; set; }

        public CourseSearchCriteria(int? UKPRNvalue)
        {
            Throw.IfNull(UKPRNvalue, nameof(UKPRNvalue));
            UKPRN = UKPRNvalue;
        }
    }
}
