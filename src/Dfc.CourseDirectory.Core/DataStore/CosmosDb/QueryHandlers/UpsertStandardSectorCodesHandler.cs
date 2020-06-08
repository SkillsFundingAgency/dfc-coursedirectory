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
    public class UpsertStandardSectorCodesHandler : ICosmosDbQueryHandler<UpsertStandardSectorCodes, None>
    {
        public async Task<None> Execute(DocumentClient client, Configuration configuration, UpsertStandardSectorCodes request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.StandardSectorCodesCollectionName);

            foreach (var record in request.Records)
            {
                var existingRecord = await FindExistingRecord();

                var standardSectorCode = existingRecord ?? new StandardSectorCode()
                {
                    Id = Guid.NewGuid(),
                    StandardSectorCodeId = record.StandardSectorCodeId,
                    CreatedDateTimeUtc = request.Now
                };

                standardSectorCode.StandardSectorCodeDesc = record.StandardSectorCodeDesc;
                standardSectorCode.StandardSectorCodeDesc2 = record.StandardSectorCodeDesc2;
                standardSectorCode.EffectiveFrom = record.EffectiveFrom;
                standardSectorCode.EffectiveTo = record.EffectiveTo;
                standardSectorCode.ModifiedDateTimeUtc = request.Now;

                if (existingRecord != null)
                {
                    var documentUri = UriFactory.CreateDocumentUri(
                        configuration.DatabaseId,
                        configuration.StandardSectorCodesCollectionName,
                        existingRecord.Id.ToString());

                    await client.ReplaceDocumentAsync(documentUri, standardSectorCode);
                }
                else
                {
                    await client.CreateDocumentAsync(collectionUri, standardSectorCode);
                }

                async Task<StandardSectorCode> FindExistingRecord()
                {
                    return (await client.CreateDocumentQuery<StandardSectorCode>(collectionUri)
                            .Where(s => s.StandardSectorCodeId == record.StandardSectorCodeId)
                            .AsDocumentQuery()
                            .FetchAll())
                        .SingleOrDefault();
                }
            }

            return new None();
        }
    }
}
