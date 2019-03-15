using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.UnregulatedProvision
{
    public class SectorSubjectAreaTier1
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> SectorSubjectAreaTier2 { get; set; }
    }
}
