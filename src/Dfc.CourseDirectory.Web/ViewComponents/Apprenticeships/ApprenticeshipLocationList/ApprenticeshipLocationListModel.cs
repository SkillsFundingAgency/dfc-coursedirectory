using Dfc.CourseDirectory.Services.Models.Apprenticeships;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipLocationList
{
    public class ApprenticeshipLocationListModel
    {
        public List<ApprenticeshipLocation> Locations { get; set; }
        public bool? SummaryPage { get; set; }
        public ApprenticeshipMode Mode { get; set; }
    }
}
