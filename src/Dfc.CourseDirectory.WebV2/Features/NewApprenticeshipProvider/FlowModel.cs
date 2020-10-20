using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using ClassroomLocation = Dfc.CourseDirectory.WebV2.Features.Apprenticeships.ClassroomLocation;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class FlowModel : IMptxState, ClassroomLocation.IFlowModelCallback
    {
        public string ProviderMarketingInformation { get; private set; }
        public StandardOrFramework ApprenticeshipStandardOrFramework { get; private set; }
        public string ApprenticeshipMarketingInformation { get; private set; }
        public string ApprenticeshipWebsite { get; private set; }
        public string ApprenticeshipContactTelephone { get; private set; }
        public string ApprenticeshipContactEmail { get; private set; }
        public string ApprenticeshipContactWebsite { get; private set; }
        public ApprenticeshipLocationType? ApprenticeshipLocationType { get; private set; }
        public bool? ApprenticeshipIsNational { get; private set; }
        public IReadOnlyCollection<string> ApprenticeshipLocationSubRegionIds { get; private set; }
        public Dictionary<Guid, ClassroomLocationEntry> ApprenticeshipClassroomLocations { get; private set; }

        public bool GotApprenticeshipDetails { get; private set; }
        public bool GotProviderDetails { get; private set; }
        public Guid? ApprenticeshipId { get; private set; }

        IReadOnlyCollection<Guid> ClassroomLocation.IFlowModelCallback.BlockedVenueIds =>
            ApprenticeshipClassroomLocations?.Keys;

        public void RemoveLocation(Guid venueId) => ApprenticeshipClassroomLocations.Remove(venueId);

        public void SetApprenticeshipId(Guid apprenticeshipId) => ApprenticeshipId = apprenticeshipId;

        public void SetClassroomLocationForVenue(
            Guid venueId,
            Guid? originalVenueId,
            int radius,
            IEnumerable<ApprenticeshipDeliveryMode> deliveryModes)
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
            IEnumerable<ApprenticeshipDeliveryMode> deliveryModes) =>
            SetClassroomLocationForVenue(venueId, originalVenueId, radius, deliveryModes);

        public class ClassroomLocationEntry
        {
            public Guid VenueId { get; set; }
            public int Radius { get; set; }
            public IEnumerable<ApprenticeshipDeliveryMode> DeliveryModes { get; set; }
        }
    }
}
