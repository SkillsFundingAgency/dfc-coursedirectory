using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class DeleteVenueHandler : ICosmosDbQueryHandler<DeleteVenue, OneOf<NotFound, Success>>
    {
        private readonly SqlDataSync _sqlDataSync;

        public DeleteVenueHandler(SqlDataSync sqlDataSync)
        {
            _sqlDataSync = sqlDataSync;
        }

        public async Task<OneOf<NotFound, Success>> Execute(DocumentClient client, Configuration configuration, DeleteVenue request)
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
            catch (DocumentClientException ex)
                when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return new NotFound();
            }

            venue.Status = (int)VenueStatus.Archived;

            await client.ReplaceDocumentAsync(documentUri, venue);

            await _sqlDataSync.SyncVenue(venue);

            return new Success();
        }
    }
}
