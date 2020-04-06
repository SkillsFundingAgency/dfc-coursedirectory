﻿using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries
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
    }

    public class CreateApprenticeshipLocation
    {
        public Guid Id { get; set; }
        public Guid? VenueId { get; set; }
        public bool? National { get; set; }
        public ApprenticeshipLocationAddress Address { get; set; }
        public ApprenticeshipDeliveryMode DeliveryModes { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public IEnumerable<string> Regions { get; set; }
        public ApprenticeshipLocationType ApprenticeshipLocationType { get; set; }
        public LocationType LocationType { get; set; }
        public int? Radius { get; set; }

        public static CreateApprenticeshipLocation CreateNational() => new CreateApprenticeshipLocation()
        {
            Id = Guid.NewGuid(),
            National = true,
            DeliveryModes = ApprenticeshipDeliveryMode.EmployerAddress,
            Regions = Array.Empty<string>(),
            ApprenticeshipLocationType = ApprenticeshipLocationType.EmployerBased,
            LocationType = LocationType.Venue  // fishy
        };
    }
}
