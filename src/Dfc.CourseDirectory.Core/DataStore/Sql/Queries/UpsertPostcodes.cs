using System.Collections.Generic;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertPostcodes : ISqlQuery<None>
    {
        public IEnumerable<UpsertPostcodesRecord> Records { get; set; }
    }

    public class UpsertPostcodesRecord
    {
        public string Postcode { get; set; }
        public (double Latitude, double Longitude) Position { get; set; }
        public bool InEngland { get; set; }
    }
}
