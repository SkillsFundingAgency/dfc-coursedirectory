using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertLarsCategories : ISqlQuery<None>
    {
        public IEnumerable<UpsertLarsCategoriesRecord> Records { get; set; }
    }

    public class UpsertLarsCategoriesRecord
    {
        public string CategoryRef { get; set; }
        public string ParentCategoryRef { get; set; }
        public string CategoryName { get; set; }
        public string Target { get; set; }
        public string EffectiveFrom { get; set; }
        public string EffectiveTo { get; set; }
    }
}
