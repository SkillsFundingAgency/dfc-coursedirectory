using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class SummaryViewModel
    {

        public Dictionary<string, List<string>> Regions { get; set; }

        public ApprenticeshipMode Mode { get; set; }

        public bool SummaryOnly { get; set; }

        public IApprenticeship Apprenticeship { get; set; }


    }
}
