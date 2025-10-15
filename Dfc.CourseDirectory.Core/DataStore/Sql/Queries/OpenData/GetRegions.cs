using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries.OpenData
{
    /// <summary>
    /// TBC - might clash with the new SQL regions implementation from James Gunn
    /// </summary>
    public class GetRegions : ISqlQuery<IAsyncEnumerable<RegionItem>>
    {
    }

    public class RegionItem
    {
        public string RegionId { get; set; }
        public string RegionName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Postcode { get; set; }
    }
}
