using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Features.Apprenticeships.FindStandardOrFramework;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using ClassroomLocation = Dfc.CourseDirectory.WebV2.Features.Apprenticeships.ClassroomLocation;
using FindStandardOrFramework = Dfc.CourseDirectory.WebV2.Features.Apprenticeships.FindStandardOrFramework;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class FlowModel : IMptxState, ClassroomLocation.IFlowModelCallback, FindStandardOrFramework.IFlowModelCallback
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
        public IReadOnlyCollection<string> ApprenticeshipLocationSubRegionIds { get; set; }
        public Dictionary<Guid, ClassroomLocationEntry> ApprenticeshipClassroomLocations { get; set; }

        public bool GotApprenticeshipDetails { get; set; }
        public bool GotProviderDetails { get; set; }
        public Guid? ApprenticeshipId { get; set; }

        IReadOnlyCollection<Guid> ClassroomLocation.IFlowModelCallback.BlockedVenueIds =>
            ApprenticeshipClassroomLocations?.Keys;

        public void RemoveLocation(Guid venueId) => ApprenticeshipClassroomLocations.Remove(venueId);

        public void SetClassroomLocationForVenue(
            Guid venueId,
            Guid? originalVenueId,
            int radius,
            ApprenticeshipDeliveryModes deliveryModes)
        {
            ApprenticeshipClassroomLocations ??= new Dictionary<Guid, ClassroomLocationEntry>();

            // This may be an amendment - ensure we remove original record since venue ID may have changed
            if (originalVenueId.HasValue)
            {
                ApprenticeshipClassroomLocations.Remove(originalVenueId.Value);
            }

            ApprenticeshipClassroomLocations[venueId] = new ClassroomLocationEntry()
            {
                VenueId = venueId,
                Radius = radius,
                DeliveryModes = deliveryModes
            };
        }

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
            ApprenticeshipLocationSubRegionIds = regionIds;
        }

        public void SetApprenticeshipLocationType(ApprenticeshipLocationType apprenticeshipLocationType)
        {
            ApprenticeshipLocationType = apprenticeshipLocationType;

            if (apprenticeshipLocationType == Core.Models.ApprenticeshipLocationType.ClassroomBased)
            {
                ApprenticeshipIsNational = null;
            }
        }

        public void SetApprenticeshipStandardOrFramework(StandardOrFramework standardOrFramework) =>
            ApprenticeshipStandardOrFramework = standardOrFramework;

        void ClassroomLocation.IFlowModelCallback.ReceiveLocation(
            string instanceId,
            Guid venueId,
            Guid? originalVenueId,
            int radius,
            ApprenticeshipDeliveryModes deliveryModes) =>
            SetClassroomLocationForVenue(venueId, originalVenueId, radius, deliveryModes);

        void IFlowModelCallback.ReceiveStandardOrFramework(StandardOrFramework standardOrFramework) =>
            SetApprenticeshipStandardOrFramework(standardOrFramework);

        public class ClassroomLocationEntry
        {
            public Guid VenueId { get; set; }
            public int Radius { get; set; }
            public ApprenticeshipDeliveryModes DeliveryModes { get; set; }
        }
    }
}
