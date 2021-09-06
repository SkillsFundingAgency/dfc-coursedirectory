using System;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Validation;

namespace Dfc.CourseDirectory.Core.DataManagement.Schemas
{
    public class CsvApprenticeshipRowWithErrors : CsvApprenticeshipRow
    {
        [Index(int.MaxValue), Name("ERRORS")]
        public string Errors { get; set; }

        public new static CsvApprenticeshipRowWithErrors FromModel(ApprenticeshipUploadRow row) => new CsvApprenticeshipRowWithErrors()
        {
            StandardCode = row.StandardCode.ToString(),
            StandardVersion = row.StandardVersion.ToString(),
            ApprenticeshipInformation = row.ApprenticeshipInformation,
            ApprenticeshipWebpage = row.ApprenticeshipWebpage,
            ContactEmail = row.ContactEmail,
            ContactPhone = row.ContactPhone,
            ContactUrl = row.ContactUrl,
            DeliveryMethod = row.DeliveryMethod,
            VenueName = row.VenueName,
            YourVenueReference = row.YourVenueReference,
            Radius = row.Radius,
            DeliveryMode = row.DeliveryMode,
            NationalDelivery = row.NationalDelivery,
            SubRegion = row.SubRegions,
            Errors = string.Join(
                "\n",
                row.Errors.Select(errorCode => ErrorRegistry.All[errorCode].GetMessage()))
        };
    }
}
