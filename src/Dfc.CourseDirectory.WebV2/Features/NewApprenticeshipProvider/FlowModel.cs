using System.Collections.Generic;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class FlowModel : IMptxState
    {
        public string ProviderMarketingInformation { get; set; }
        public StandardOrFramework ApprenticeshipStandardOrFramework { get; set; }
        public string ApprenticeshipMarketingInformation { get; set; }
        public string ApprenticeshipWebsite { get; set; }
        public string ApprenticeshipContactTelephone { get; set; }
        public string ApprenticeshipContactEmail { get; set; }
        public string ApprenticeshipContactWebsite { get; set; }
        public ApprenticeshipLocationType? ApprenticeshipLocationType { get; set; }
        public bool? ApprenticeshipIsNational { get; set; }
        public IReadOnlyCollection<string> ApprenticeshipLocationRegionIds { get; set; } = new List<string>();

        public bool GotApprenticeshipDetails { get; set; }
        public bool GotProviderDetails { get; set; }

        public bool IsValid => GotProviderDetails &&
            ApprenticeshipStandardOrFramework != null &&
            ApprenticeshipLocationType != null &&
            GotApprenticeshipDetails; // FIXME

        public void SetProviderDetails(string marketingInformation)
        {
            ProviderMarketingInformation = marketingInformation;
            GotProviderDetails = true;
        }

        public void SetApprenticeshipDetails(
            string marketingInformation,
            string website,
            string contactTelephone,
            string contactEmail,
            string contactWebsite)
        {
            ApprenticeshipMarketingInformation = marketingInformation;
            ApprenticeshipWebsite = website;
            ApprenticeshipContactTelephone = contactTelephone;
            ApprenticeshipContactEmail = contactEmail;
            ApprenticeshipContactWebsite = contactWebsite;
            GotApprenticeshipDetails = true;
        }
		
		public void SetApprenticeshipIsNational(bool national)
        {
            ApprenticeshipIsNational = national;
        }

        public void SetApprenticeshipLocationRegionIds(IReadOnlyCollection<string> regionIds)
        {
            ApprenticeshipIsNational = false;
            ApprenticeshipLocationRegionIds = regionIds;
        }

        public void SetApprenticeshipLocationType(ApprenticeshipLocationType apprenticeshipLocationType)
        {
            ApprenticeshipLocationType = apprenticeshipLocationType;

            if (apprenticeshipLocationType == Models.ApprenticeshipLocationType.ClassroomBased)
            {
                ApprenticeshipIsNational = false;
            }
        }

        public void SetApprenticeshipStandardOrFramework(StandardOrFramework standardOrFramework) =>
            ApprenticeshipStandardOrFramework = standardOrFramework;
    }
}
