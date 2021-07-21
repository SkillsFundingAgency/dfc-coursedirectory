using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class LearningDelivery
    {
        public string LearnAimRef { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}
