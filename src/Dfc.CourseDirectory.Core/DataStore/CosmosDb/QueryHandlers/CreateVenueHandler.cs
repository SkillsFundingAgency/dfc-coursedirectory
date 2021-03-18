using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.Azure.Documents.Client;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class CreateVenueHandler : ICosmosDbQueryHandler<CreateVenue, Success>
    {
        private readonly SqlDataSync _sqlDataSync;

        public CreateVenueHandler(SqlDataSync sqlDataSync)
        {
            _sqlDataSync = sqlDataSync;
        }

        public async Task<Success> Execute(
            DocumentClient client,
            Configuration configuration,
            CreateVenue request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.VenuesCollectionName);

            var venue = new Venue()
            {
                Id = request.VenueId,
                VenueName = request.Name,
                Email = request.Email,
                PHONE = request.Telephone,
                Website = request.Website,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                Town = request.Town,
                County = request.County,
                Postcode = request.Postcode,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                CreatedBy = request.CreatedBy.UserId,
                CreatedDate = request.CreatedDate,
                UpdatedBy = request.CreatedBy.UserId,
                DateUpdated = request.CreatedDate,
                Status = VenueStatus.Live,
                Ukprn = request.ProviderUkprn
            };

            await client.CreateDocumentAsync(collectionUri, venue);

            await _sqlDataSync.SyncVenue(venue);

            return new Success();
        }
    }
}
