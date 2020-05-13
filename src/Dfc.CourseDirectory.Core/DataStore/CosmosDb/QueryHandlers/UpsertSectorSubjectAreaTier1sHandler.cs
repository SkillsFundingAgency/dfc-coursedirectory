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
    public class UpsertSectorSubjectAreaTier1sHandler : ICosmosDbQueryHandler<UpsertSectorSubjectAreaTier1s, None>
    {
        public async Task<None> Execute(DocumentClient client, Configuration configuration, UpsertSectorSubjectAreaTier1s request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.SectorSubjectAreaTier1sCollectionName);

            foreach (var record in request.Records)
            {
                var existingRecord = await FindExistingRecord();

                var sectorSubjectAreaTier1 = existingRecord ?? new SectorSubjectAreaTier1()
                {
                    Id = Guid.NewGuid(),
                    SectorSubjectAreaTier1Id = record.SectorSubjectAreaTier1Id,
                    CreatedDateTimeUtc = request.Now
                };

                sectorSubjectAreaTier1.SectorSubjectAreaTier1Desc = record.SectorSubjectAreaTier1Desc;
                sectorSubjectAreaTier1.SectorSubjectAreaTier1Desc2 = record.SectorSubjectAreaTier1Desc2;
                sectorSubjectAreaTier1.EffectiveFrom = record.EffectiveFrom;
                sectorSubjectAreaTier1.EffectiveTo = record.EffectiveTo;
                sectorSubjectAreaTier1.ModifiedDateTimeUtc = request.Now;

                if (existingRecord != null)
                {
                    var documentUri = UriFactory.CreateDocumentUri(
                        configuration.DatabaseId,
                        configuration.SectorSubjectAreaTier1sCollectionName,
                        existingRecord.Id.ToString());

                    await client.ReplaceDocumentAsync(documentUri, sectorSubjectAreaTier1);
                }
                else
                {
                    await client.CreateDocumentAsync(collectionUri, sectorSubjectAreaTier1);
                }

                async Task<SectorSubjectAreaTier1> FindExistingRecord()
                {
                    return (await client.CreateDocumentQuery<SectorSubjectAreaTier1>(collectionUri)
                            .Where(s => s.SectorSubjectAreaTier1Id == record.SectorSubjectAreaTier1Id)
                            .AsDocumentQuery()
                            .FetchAll())
                        .SingleOrDefault();
                }
            }

            return new None();
        }
    }
}
