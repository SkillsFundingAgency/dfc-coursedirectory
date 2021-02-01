using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core.Models
{
    public class Region
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IReadOnlyCollection<Region> SubRegions { get; set; }
    }
}
