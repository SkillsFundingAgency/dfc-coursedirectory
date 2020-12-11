using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.ViewTLevels
{
    public class TLevelViewModel
    {
        public Guid TLevelId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public IReadOnlyCollection<string> VenueNames { get; set; }
    }
}
