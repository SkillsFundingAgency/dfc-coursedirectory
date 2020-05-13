using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class UpsertProgTypesHandler : ICosmosDbQueryHandler<UpsertProgTypes, None>
    {
        public async Task<None> Execute(DocumentClient client, Configuration configuration, UpsertProgTypes request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.ProgTypesCollectionName);

            foreach (var record in request.Records)
            {
                var existingRecord = await FindExistingRecord();

                var progType = existingRecord ?? new ProgType()
                {
                    Id = Guid.NewGuid(),
                    ProgTypeId = record.ProgTypeId,
                    CreatedDateTimeUtc = request.Now
                };

                progType.ProgTypeDesc = record.ProgTypeDesc;
                progType.ProgTypeDesc2 = record.ProgTypeDesc2;
                progType.EffectiveFrom = record.EffectiveFrom;
                progType.EffectiveTo = record.EffectiveTo;
                progType.ModifiedDateTimeUtc = request.Now;

                if (existingRecord != null)
                {
                    var documentUri = UriFactory.CreateDocumentUri(
                        configuration.DatabaseId,
                        configuration.ProgTypesCollectionName,
                        existingRecord.Id.ToString());

                    await client.ReplaceDocumentAsync(documentUri, progType);
                }
                else
                {
                    await client.CreateDocumentAsync(collectionUri, progType);
                }

                async Task<ProgType> FindExistingRecord()
                {
                    return (await client.CreateDocumentQuery<ProgType>(collectionUri)
                            .Where(s => s.ProgTypeId == record.ProgTypeId)
                            .AsDocumentQuery()
                            .FetchAll())
                        .SingleOrDefault();
                }
            }

            return new None();
        }
    }
}
