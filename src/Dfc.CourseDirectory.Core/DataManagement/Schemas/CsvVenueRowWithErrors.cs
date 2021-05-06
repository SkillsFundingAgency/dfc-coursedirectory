using System;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Validation;

namespace Dfc.CourseDirectory.Core.DataManagement.Schemas
{
    public class CsvVenueRowWithErrors : CsvVenueRow
    {
        [Index(int.MaxValue), Name("ERRORS")]
        public string Errors { get; set; }

        public new static CsvVenueRowWithErrors FromModel(VenueUploadRow row) => new CsvVenueRowWithErrors()
        {
            ProviderVenueRef = row.ProviderVenueRef,
            VenueName = row.VenueName,
            AddressLine1 = row.AddressLine1,
            AddressLine2 = row.AddressLine2,
            Town = row.Town,
            County = row.County,
            Postcode = row.Postcode,
            Email = row.Email,
            Telephone = row.Telephone,
            Website = row.Website,
            Errors = string.Join("\n", row.Errors.Select(errorCode => ErrorRegistry.All[errorCode].GetMessage()))
        };
    }
}
