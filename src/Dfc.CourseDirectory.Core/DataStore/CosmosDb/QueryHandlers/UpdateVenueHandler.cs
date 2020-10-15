using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateVenueHandler : ICosmosDbQueryHandler<UpdateVenue, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(
            DocumentClient client,
            Configuration configuration,
            UpdateVenue request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.VenuesCollectionName,
                request.VenueId.ToString());

            Venue venue;

            try
            {
                var query = await client.ReadDocumentAsync<Venue>(documentUri);

                venue = query.Document;
            }
            catch (DocumentClientException dex) when (dex.StatusCode == HttpStatusCode.NotFound)
            {
                return new NotFound();
            }

            venue.VenueName = request.Name;
            venue.Email = request.Email;
            venue.PHONE = request.PhoneNumber;
            venue.Website = request.Website;
            venue.AddressLine1 = request.AddressLine1;
            venue.AddressLine2 = request.AddressLine2;
            venue.Town = request.Town;
            venue.County = request.County;
            venue.Postcode = request.Postcode;
            venue.Latitude = request.Latitude;
            venue.Longitude = request.Longitude;
            venue.UpdatedBy = request.UpdatedBy.Email;
            venue.DateUpdated = request.UpdatedDate;

            await client.ReplaceDocumentAsync(documentUri, venue);

            return new Success();
        }
    }
}
