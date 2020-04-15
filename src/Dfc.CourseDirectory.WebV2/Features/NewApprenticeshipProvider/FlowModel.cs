using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.WebV2.Features.Apprenticeships.ClassroomLocation;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class FlowModel : IMptxState, IFlowModelCallback
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
        public Dictionary<Guid, ClassroomLocation> ApprenticeshipClassroomLocations { get; set; }

        public bool GotApprenticeshipDetails { get; set; }
        public bool GotProviderDetails { get; set; }

        public bool IsValid => GotProviderDetails &&
            ApprenticeshipStandardOrFramework != null &&
            ApprenticeshipLocationType != null &&
            (
                (ApprenticeshipLocationType.Value.HasFlag(Models.ApprenticeshipLocationType.ClassroomBased) &&
                    (ApprenticeshipClassroomLocations?.Count ?? 0) > 0) ||
                (ApprenticeshipLocationType.Value.HasFlag(Models.ApprenticeshipLocationType.EmployerBased) &&
                    (ApprenticeshipIsNational.GetValueOrDefault() || (ApprenticeshipLocationSubRegionIds?.Count ?? 0) > 0))) &&
            GotApprenticeshipDetails;

        public void AddClassroomLocation(
            Guid venueId,
            bool national,
            int? radius,
            ApprenticeshipDeliveryModes deliveryModes)
        {
            ApprenticeshipClassroomLocations ??= new Dictionary<Guid, ClassroomLocation>();

            ApprenticeshipClassroomLocations[venueId] = new ClassroomLocation()
            {
                VenueId = venueId,
                National = national,
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

            if (apprenticeshipLocationType == Models.ApprenticeshipLocationType.ClassroomBased)
            {
                ApprenticeshipIsNational = null;
            }
        }

        public void SetApprenticeshipStandardOrFramework(StandardOrFramework standardOrFramework) =>
            ApprenticeshipStandardOrFramework = standardOrFramework;

        void IFlowModelCallback.ReceiveLocation(
            string instanceId,
            Guid venueId,
            bool national,
            int? radius,
            ApprenticeshipDeliveryModes deliveryModes) =>
            AddClassroomLocation(venueId, national, radius, deliveryModes);

        public class ClassroomLocation
        {
            public Guid VenueId { get; set; }
            public bool National { get; set; }
            public int? Radius { get; set; }
            public ApprenticeshipDeliveryModes DeliveryModes { get; set; }
        }
    }
}
