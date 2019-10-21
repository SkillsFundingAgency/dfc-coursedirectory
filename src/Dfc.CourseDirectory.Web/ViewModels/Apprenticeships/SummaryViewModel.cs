using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class SummaryViewModel
    {
       
        public LocationChoiceSelectionViewModel LocationChoiceSelectionViewModel { get; set; }

        public DetailViewModel DetailViewModel { get; set; }

        public DeliveryViewModel DeliveryViewModel { get; set; }

        public RegionsViewModel RegionsViewModel { get; set; }

        public DeliveryOptionsViewModel DeliveryOptions { get; set; }

        public DeliveryOptionsCombined DeliveryOptionsCombined { get; set; }

        public Dictionary<string, List<string>> Regions { get; set; }

        public ApprenticeshipMode  Mode { get; set; }

        public bool? Cancelled { get; set; }




}
}
