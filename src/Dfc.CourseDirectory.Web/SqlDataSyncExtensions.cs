using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Services.Models.Venues;

namespace Dfc.CourseDirectory.Web
{
    public static class SqlDataSyncExtensions
    {
        public static Task SyncVenue(this SqlDataSync sqlDataSync, Venue venue) =>
            sqlDataSync.SyncVenue(new Core.DataStore.CosmosDb.Models.Venue()
            {
                AdditionalData = null,
                AddressLine1 = venue.Address1,
                AddressLine2 = venue.Address2,
                County = venue.County,
                CreatedDate = venue.DateAdded,
                DateUpdated = venue.DateUpdated,
                Email = venue.Email,
                Id = Guid.Parse(venue.ID),
                Latitude = venue.Latitude,
                LocationId = venue.LocationId,
                Longitude = venue.Longitude,
                PHONE = venue.Telephone,
                Postcode = venue.PostCode,
                ProvVenueID = venue.ProvVenueID,
                Status = (int)venue.Status,
                Town = venue.Town,
                Ukprn = venue.UKPRN,
                UpdatedBy = venue.UpdatedBy,
                VenueName = venue.VenueName,
                Website = venue.Website
            });
    }
}
