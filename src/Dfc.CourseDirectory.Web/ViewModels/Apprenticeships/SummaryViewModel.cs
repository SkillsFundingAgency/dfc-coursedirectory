using System.Collections.Generic;
using Dfc.CourseDirectory.Web.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class SummaryViewModel
    {
        public Dictionary<string, List<string>> Regions { get; set; }
        
        public bool SummaryOnly { get; set; }

        public Apprenticeship Apprenticeship { get; set; }

        public ApprenticeshipMode Mode { get; set; }

        public bool HasAllLocationTypes { get; set; }
    }
}
