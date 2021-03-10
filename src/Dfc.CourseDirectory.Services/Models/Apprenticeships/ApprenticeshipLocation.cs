using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Services.Models.Apprenticeships
{
    public class ApprenticeshipLocation
    {
        public Guid Id { get; set; }
        public Guid? VenueId { get; set; }
        public Guid? LocationGuidId { get; set; }
        public int? LocationId { get; set; }
        public bool? National { get; set; }
        public ApprenticeshipLocationAddress Address { get; set; }
        public List<int> DeliveryModes { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int ProviderUKPRN { get; set; }
        public IEnumerable<string> Regions { get; set; }
        public ApprenticeshipLocationType ApprenticeshipLocationType { get; set; }
        public LocationType LocationType { get; set; }
        public int? Radius { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        public CreateApprenticeshipLocation ToCreateApprenticeshipLocation()
        {
            return new CreateApprenticeshipLocation
            {
                Id = Id,
                VenueId = VenueId,
                National = National,
                Address = Address != null
                        ? new Core.DataStore.CosmosDb.Models.ApprenticeshipLocationAddress
                        {
                            Address1 = Address.Address1,
                            Address2 = Address.Address2,
                            County = Address.County,
                            Email = Address.Email,
                            Latitude = Address.Latitude ?? 0,
                            Longitude = Address.Longitude ?? 0,
                            Phone = Address.Phone,
                            Postcode = Address.Postcode,
                            Town = Address.Town,
                            Website = Address.Website
                        }
                        : null,
                DeliveryModes = DeliveryModes.Cast<ApprenticeshipDeliveryMode>().ToList(),
                Name = Name,
                Phone = Phone,
                Regions = Regions,
                ApprenticeshipLocationType = ApprenticeshipLocationType,
                LocationType = LocationType,
                Radius = Radius
            };
        }
    }
}
