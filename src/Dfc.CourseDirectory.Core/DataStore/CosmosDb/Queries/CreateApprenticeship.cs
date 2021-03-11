using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries
{
    public class CreateApprenticeship : ICosmosDbQuery<Success>
    {
        public Guid Id { get; set; }
        public Guid ProviderId { get; set; }
        public int ProviderUkprn { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public StandardOrFramework StandardOrFramework { get; set; }
        public string MarketingInformation { get; set; }
        public string Url { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
        public IEnumerable<CreateApprenticeshipLocation> ApprenticeshipLocations { get; set; }
        public DateTime CreatedDate { get; set; }
        public UserInfo CreatedByUser { get; set; }
        public int Status { get; set; } = 1;
        public IEnumerable<BulkUploadError> BulkUploadErrors { get; set; }
    }

    public class CreateApprenticeshipLocation
    {
        public Guid? Id { get; set; }
        public Guid? VenueId { get; set; }
        public bool? National { get; set; }
        public ApprenticeshipLocationAddress Address { get; set; }
        public IEnumerable<ApprenticeshipDeliveryMode> DeliveryModes { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public IEnumerable<string> Regions { get; set; }
        public ApprenticeshipLocationType ApprenticeshipLocationType { get; set; }
        public LocationType LocationType { get; set; }
        public int? Radius { get; set; }

        public static CreateApprenticeshipLocation CreateFromVenue(
            Venue venue,
            int radius,
            IEnumerable<ApprenticeshipDeliveryMode> deliveryModes) =>
            new CreateApprenticeshipLocation()
            {
                Id = Guid.NewGuid(),
                VenueId = venue.Id,
                Address = new ApprenticeshipLocationAddress()
                {
                    Address1 = venue.AddressLine1,
                    Address2 = venue.AddressLine2,
                    County = venue.County,
                    Email = venue.Email,
                    Latitude = venue.Latitude,
                    Longitude = venue.Longitude,
                    Phone = venue.PHONE,
                    Postcode = venue.Postcode,
                    Town = venue.Town,
                    Website = venue.Website
                },
                DeliveryModes = deliveryModes,
                Name = venue.VenueName,
                ApprenticeshipLocationType = ApprenticeshipLocationType.ClassroomBased,
                LocationType = LocationType.Venue,
                Radius = radius
            };

        public static CreateApprenticeshipLocation CreateNational() => new CreateApprenticeshipLocation()
        {
            Id = Guid.NewGuid(),
            National = true,
            DeliveryModes = new[] { ApprenticeshipDeliveryMode.EmployerAddress },
            Regions = Array.Empty<string>(),
            ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased
        };

        public static CreateApprenticeshipLocation CreateRegions(IEnumerable<string> regionIds) => new CreateApprenticeshipLocation()
        {
            Id = Guid.NewGuid(),
            National = false,
            DeliveryModes = new[] { ApprenticeshipDeliveryMode.EmployerAddress },
            Regions = regionIds,
            ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
            LocationType = LocationType.SubRegion
        };
    }
}
