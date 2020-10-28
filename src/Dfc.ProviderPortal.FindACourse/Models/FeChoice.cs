using System;

namespace Dfc.ProviderPortal.FindACourse.Models
{
    public class FeChoice
    {
        public Guid Id { get; set; }
        public int UKPRN { get; set; }
        public decimal? LearnerSatisfaction { get; set; }
        public decimal? EmployerSatisfaction { get; set; }
        public DateTime? CreatedDateTimeUtc { get; set; }
    }
}
