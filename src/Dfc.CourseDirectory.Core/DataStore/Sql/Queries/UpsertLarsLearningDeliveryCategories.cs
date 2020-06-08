using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertLarsLearningDeliveryCategories : ISqlQuery<None>
    {
        public IEnumerable<UpsertLarsLearningDeliveryCategoriesRecord> Records { get; set; }
    }

    public class UpsertLarsLearningDeliveryCategoriesRecord
    {
        public string LearnAimRef { get; set; }
        public string CategoryRef { get; set; }
        public string EffectiveFrom { get; set; }
        public string EffectiveTo { get; set; }
        public string Created_On { get; set; }
        public string Created_By { get; set; }
        public string Modified_On { get; set; }
        public string Modified_By { get; set; }
    }
}
