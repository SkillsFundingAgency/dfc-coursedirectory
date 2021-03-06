﻿using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class ProcessAllProvidersHandler : ICosmosDbQueryHandler<ProcessAllProviders, None>
    {
        public async Task<None> Execute(DocumentClient client, Configuration configuration, ProcessAllProviders request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.ProviderCollectionName);

            var query = client.CreateDocumentQuery<Provider>(
                collectionUri,
                new FeedOptions() { EnableCrossPartitionQuery = true, MaxItemCount = -1 }).AsDocumentQuery();

            await query.ProcessAll(request.ProcessChunk);

            return new None();
        }
    }
}
