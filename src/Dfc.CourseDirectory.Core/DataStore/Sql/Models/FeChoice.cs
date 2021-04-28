using System;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
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
