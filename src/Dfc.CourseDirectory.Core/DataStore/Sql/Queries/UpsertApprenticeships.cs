using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpsertApprenticeships : ISqlQuery<None>
    {
        public IEnumerable<UpsertApprenticeshipRecord> Records { get; set; }
    }

    public class UpsertApprenticeshipRecord
    {
        public Guid ApprenticeshipId { get; set; }
        public ApprenticeshipStatus ApprenticeshipStatus { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public int? TribalApprenticeshipId { get; set; }
        public int ProviderUkprn { get; set; }
        public Guid ProviderId { get; set; }
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public int? StandardCode { get; set; }
        public int? StandardVersion { get; set; }
        public int? FrameworkCode { get; set; }
        public int? FrameworkProgType { get; set; }
        public int? FrameworkPathwayCode { get; set; }
        public string MarketingInformation { get; set; }
        public string ApprenticeshipWebsite { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
        public IEnumerable<UpsertApprenticeshipRecordLocation> Locations { get; set; }
    }

    public class UpsertApprenticeshipRecordLocation
    {
        public Guid ApprenticeshipLocationId { get; set; }
        public ApprenticeshipStatus ApprenticeshipLocationStatus { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public string Telephone { get; set; }
        public Guid? VenueId { get; set; }
        public int? TribalApprenticeshipLocationId { get; set; }
        public bool? National { get; set; }
        public int? Radius { get; set; }
        public LocationType LocationType { get; set; }
        public ApprenticeshipLocationType ApprenticeshipLocationType { get; set; }
        public string Name { get; set; }
        public IEnumerable<ApprenticeshipDeliveryMode> DeliveryModes { get; set; }
        public IEnumerable<string> Regions { get; set; }
    }
}
