using System.Collections.Generic;
using Dfc.CourseDirectory.Web.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipLocationList
{
    public class ApprenticeshipLocationListModel
    {
        public List<ApprenticeshipLocation> Locations { get; set; }
        public bool? SummaryPage { get; set; }
        public ApprenticeshipMode Mode { get; set; }
    }
}
