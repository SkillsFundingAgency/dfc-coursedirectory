using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class CreateApprenticeship : ISqlQuery<Success>
    {
        public Guid ApprenticeshipId { get; set; }
        public Guid ProviderId { get; set; }
        public ApprenticeshipStatus Status { get; set; } = ApprenticeshipStatus.Live;
        public Standard Standard { get; set; }
        public string MarketingInformation { get; set; }
        public string ApprenticeshipWebsite { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
        public IEnumerable<CreateApprenticeshipLocation> ApprenticeshipLocations { get; set; }
        public UserInfo CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class CreateApprenticeshipLocation
    {
        public Guid ApprenticeshipLocationId { get; set; }
        public ApprenticeshipLocationType ApprenticeshipLocationType { get; set; }
        public Guid? VenueId { get; set; }
        public bool? National { get; set; }
        public IEnumerable<string> SubRegionIds { get; set; }
        public int? Radius { get; set; }
        public IEnumerable<ApprenticeshipDeliveryMode> DeliveryModes { get; set; }
        public string Telephone { get; set; }

        public static CreateApprenticeshipLocation CreateClassroomBased(
            IEnumerable<ApprenticeshipDeliveryMode> deliveryModes,
            int radius,
            Guid venueId) =>
            new CreateApprenticeshipLocation()
            {
                ApprenticeshipLocationId = Guid.NewGuid(),
                ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBased,
                DeliveryModes = deliveryModes,
                Radius = radius,
                VenueId = venueId
            };

        public static CreateApprenticeshipLocation CreateNationalEmployerBased() =>
            new CreateApprenticeshipLocation()
            {
                ApprenticeshipLocationId = Guid.NewGuid(),
                ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                DeliveryModes = new[] { ApprenticeshipDeliveryMode.EmployerAddress },
                National = true
            };

        public static CreateApprenticeshipLocation CreateRegionalEmployerBased(IEnumerable<string> subRegionIds) =>
            new CreateApprenticeshipLocation()
            {
                ApprenticeshipLocationId = Guid.NewGuid(),
                ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
                DeliveryModes = new[] { ApprenticeshipDeliveryMode.EmployerAddress },
                National = false,
                SubRegionIds = subRegionIds
            };

        public static CreateApprenticeshipLocation FromModel(ApprenticeshipLocation model) =>
            new CreateApprenticeshipLocation()
            {
                ApprenticeshipLocationId = model.ApprenticeshipLocationId,
                ApprenticeshipLocationType = model.ApprenticeshipLocationType,
                DeliveryModes = model.DeliveryModes,
                National = model.National,
                Radius = model.Radius,
                SubRegionIds = model.SubRegionIds,
                VenueId = model.Venue?.VenueId
            };
    }
}
