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
    public class UpsertSectorSubjectAreaTier2sHandler : ICosmosDbQueryHandler<UpsertSectorSubjectAreaTier2s, None>
    {
        public async Task<None> Execute(DocumentClient client, Configuration configuration, UpsertSectorSubjectAreaTier2s request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.SectorSubjectAreaTier2sCollectionName);

            foreach (var record in request.Records)
            {
                var existingRecord = await FindExistingRecord();

                var sectorSubjectAreaTier2 = existingRecord ?? new SectorSubjectAreaTier2()
                {
                    Id = Guid.NewGuid(),
                    SectorSubjectAreaTier2Id = record.SectorSubjectAreaTier2Id,
                    CreatedDateTimeUtc = request.Now
                };

                sectorSubjectAreaTier2.SectorSubjectAreaTier2Desc = record.SectorSubjectAreaTier2Desc;
                sectorSubjectAreaTier2.SectorSubjectAreaTier2Desc2 = record.SectorSubjectAreaTier2Desc2;
                sectorSubjectAreaTier2.EffectiveFrom = record.EffectiveFrom;
                sectorSubjectAreaTier2.EffectiveTo = record.EffectiveTo;
                sectorSubjectAreaTier2.ModifiedDateTimeUtc = request.Now;

                if (existingRecord != null)
                {
                    var documentUri = UriFactory.CreateDocumentUri(
                        configuration.DatabaseId,
                        configuration.SectorSubjectAreaTier2sCollectionName,
                        existingRecord.Id.ToString());

                    await client.ReplaceDocumentAsync(documentUri, sectorSubjectAreaTier2);
                }
                else
                {
                    await client.CreateDocumentAsync(collectionUri, sectorSubjectAreaTier2);
                }

                async Task<SectorSubjectAreaTier2> FindExistingRecord()
                {
                    return (await client.CreateDocumentQuery<SectorSubjectAreaTier2>(collectionUri)
                            .Where(s => s.SectorSubjectAreaTier2Id == record.SectorSubjectAreaTier2Id)
                            .AsDocumentQuery()
                            .FetchAll())
                        .SingleOrDefault();
                }
            }

            return new None();
        }
    }
}
