using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Xunit;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {

        public async Task<VenueUploadRow> CreateVenueUploadRow(
        Guid VenueUploadId,
        int RowNumber = 2,
        bool IsValid = false,
        bool IsSupplementary = false,
        bool OutsideOfEngland = false)
        {
            return await WithSqlQueryDispatcher(async dispatcher =>
             {
                 var rowUpload = await dispatcher.ExecuteQuery(new CreateVenueUploadRow()
                 {
                     ProviderVenueRef = Guid.NewGuid().ToString(),
                     VenueName = Faker.Company.Name(),
                     AddressLine1 = Faker.Address.StreetAddress(),
                     AddressLine2 = Faker.Address.SecondaryAddress(),
                     Town = Faker.Address.City(),
                     County = Faker.Address.UkCounty(),
                     Postcode = "AB1 2DE",  // Faker's method sometimes produces invalid postcodes :-/
                     Email = Faker.Internet.Email(),
                     Telephone = "01234 567890",  // There's no Faker method for a UK phone number
                     Website = Faker.Internet.Url(),
                     IsValid = IsValid,
                     VenueUploadId = VenueUploadId,
                     OutsideOfEngland = OutsideOfEngland,
                     IsSupplementary = IsSupplementary,
                     RowNumber = RowNumber,
                     LastUpdated = _clock.UtcNow,
                     LastValidated = _clock.UtcNow
                 });

                 var venueUploadRow = await dispatcher.ExecuteQuery(new GetVenueUploadRow() { VenueUploadRowId = rowUpload });

                 return venueUploadRow;

             });
        }
    }
}
