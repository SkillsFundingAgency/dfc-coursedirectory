using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Web.Models.Apprenticeships
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
        public int ProviderUkprn { get; set; }
        public ApprenticeshipType ApprenticeshipType { get; set; }
        public int StandardCode { get; set; }
        public int StandardVersion { get; set; }
        public string MarketingInformation { get; set; }
        public string Url { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
        public List<ApprenticeshipLocation> ApprenticeshipLocations { get; set; }
        public DateTime CreatedDate { get; set; }
        //public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        //public string UpdatedBy { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public IEnumerable<string> ValidationErrors { get; set; }
        public IEnumerable<string> LocationValidationErrors { get; set; }

        public static Apprenticeship FromSqlModel(Core.DataStore.Sql.Models.Apprenticeship apprenticeship)
        {
            if (apprenticeship == null)
            {
                throw new ArgumentNullException(nameof(apprenticeship));
            }

            return new Apprenticeship
            {
                id = apprenticeship.ApprenticeshipId,
                ApprenticeshipTitle = apprenticeship.Standard.StandardName,
                ProviderId = apprenticeship.ProviderId,
                ProviderUkprn = apprenticeship.ProviderUkprn,
                ApprenticeshipType = ApprenticeshipType.Standard,
                StandardCode = apprenticeship.Standard.StandardCode,
                StandardVersion = apprenticeship.Standard.Version,
                MarketingInformation = apprenticeship.MarketingInformation,
                Url = apprenticeship.ApprenticeshipWebsite,
                ContactTelephone = apprenticeship.ContactTelephone,
                ContactEmail = apprenticeship.ContactEmail,
                ContactWebsite = apprenticeship.ContactWebsite,
                ApprenticeshipLocations = apprenticeship.ApprenticeshipLocations?.Where(l => l != null).Select(l => new ApprenticeshipLocation
                {
                    Id = l.ApprenticeshipLocationId,
                    VenueId = l.Venue?.VenueId,
                    Venue = l.Venue,
                    ApprenticeshipLocationType = l.ApprenticeshipLocationType,
                    National = l.National,
                    SubRegionIds = l.SubRegionIds,
                    Name = l.Venue?.VenueName,
                    Radius = l.Radius,
                    DeliveryModes = l.DeliveryModes.Select(v => (int)v).ToList()
                }).ToList(),
                CreatedDate = apprenticeship.CreatedOn,
                UpdatedDate = apprenticeship.UpdatedOn,
            };
        }
    }
}
