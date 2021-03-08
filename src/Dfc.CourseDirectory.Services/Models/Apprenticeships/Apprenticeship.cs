using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Services.Models.Apprenticeships
{
    public enum ApprenticeshipMode
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Add")]
        Add = 1,
        [Description("EditApprenticeship")]
        EditApprenticeship = 2,
        [Description("EditYourApprenticeships")]
        EditYourApprenticeships = 3,
        [Description("DeleteYourAprrenticeships")]
        DeleteYourAprrenticeships = 4
    }

    public class Apprenticeship
    {
        public Guid id { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public Guid ProviderId { get; set; }
        public int ProviderUKPRN { get; set; }
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public Guid? FrameworkId { get; set; }
        public Guid? StandardId { get; set; }
        public int? FrameworkCode { get; set; }
        public int? ProgType { get; set; } 
        public int? PathwayCode { get; set; }
        public int? StandardCode { get; set; }
        public int? Version { get; set; }
        public string MarketingInformation { get; set; }
        public string Url { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
        public List<ApprenticeshipLocation> ApprenticeshipLocations { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public List<BulkUploadError> BulkUploadErrors { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public IEnumerable<string> ValidationErrors { get; set; }
        public IEnumerable<string> LocationValidationErrors { get; set; }

        public static Apprenticeship FromCosmosDbModel(Core.DataStore.CosmosDb.Models.Apprenticeship apprenticeship)
        {
            if (apprenticeship == null)
            {
                throw new ArgumentNullException(nameof(apprenticeship));
            }

            return new Apprenticeship
            {
                id = apprenticeship.Id,
                ApprenticeshipTitle = apprenticeship.ApprenticeshipTitle,
                ProviderId = apprenticeship.ProviderId,
                ProviderUKPRN = apprenticeship.ProviderUKPRN,
                ApprenticeshipType = apprenticeship.ApprenticeshipType,
                FrameworkId = apprenticeship.FrameworkId,
                StandardId = apprenticeship.StandardId,
                FrameworkCode = apprenticeship.FrameworkCode,
                ProgType = apprenticeship.ProgType,
                PathwayCode = apprenticeship.PathwayCode,
                StandardCode = apprenticeship.StandardCode,
                Version = apprenticeship.Version,
                MarketingInformation = apprenticeship.MarketingInformation,
                Url = apprenticeship.Url,
                ContactTelephone = apprenticeship.ContactTelephone,
                ContactEmail = apprenticeship.ContactEmail,
                ContactWebsite = apprenticeship.ContactWebsite,
                ApprenticeshipLocations = apprenticeship.ApprenticeshipLocations?.Where(l => l != null).Select(l => new ApprenticeshipLocation
                {
                    Id = l.Id,
                    VenueId = l.VenueId,
                    LocationGuidId = l.LocationGuidId,
                    LocationId = l.ApprenticeshipLocationId,
                    National = l.National,
                    Address = l.Address != null
                            ? new Address
                            {
                                Address1 = l.Address.Address1,
                                Address2 = l.Address.Address2,
                                County = l.Address.County,
                                Email = l.Address.Email,
                                Latitude = l.Address.Latitude,
                                Longitude = l.Address.Longitude,
                                Phone = l.Address.Phone,
                                Postcode = l.Address.Postcode,
                                Town = l.Address.Town,
                                Website = l.Address.Website
                            }
                            : null,
                    DeliveryModes = l.DeliveryModes.Cast<int>().ToList(),
                    Name = l.Name,
                    Phone = l.Phone,
                    ProviderUKPRN = l.ProviderUKPRN,
                    Regions = l.Regions,
                    ApprenticeshipLocationType = l.ApprenticeshipLocationType,
                    LocationType = l.LocationType,
                    Radius = l.Radius,
                    RecordStatus = (RecordStatus)l.RecordStatus,
                    CreatedDate = l.CreatedDate,
                    CreatedBy = l.CreatedBy,
                    UpdatedDate = l.UpdatedDate,
                    UpdatedBy = l.UpdatedBy
                }).ToList(),
                RecordStatus = (RecordStatus)apprenticeship.RecordStatus,
                CreatedDate = apprenticeship.CreatedDate,
                CreatedBy = apprenticeship.CreatedBy,
                UpdatedDate = apprenticeship.UpdatedDate,
                UpdatedBy = apprenticeship.UpdatedBy,
                BulkUploadErrors = apprenticeship.BulkUploadErrors?.Where(b => b != null).Select(b => new BulkUploadError
                {
                    LineNumber = b.LineNumber,
                    Header = b.Header,
                    Error = b.Error
                }).ToList(),
                NotionalNVQLevelv2 = apprenticeship.NotionalNVQLevelv2,
                ValidationErrors = apprenticeship.ValidationErrors,
                LocationValidationErrors = apprenticeship.LocationValidationErrors
            };
        }
    }
}
