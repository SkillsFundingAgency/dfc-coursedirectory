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
    public class UpsertFrameworksHandler : ICosmosDbQueryHandler<UpsertFrameworks, None>
    {
        public async Task<None> Execute(DocumentClient client, Configuration configuration, UpsertFrameworks request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.FrameworksCollectionName);

            foreach (var record in request.Records)
            {
                var existingRecord = await FindExistingRecord();

                var framework = existingRecord ?? new Framework()
                {
                    Id = Guid.NewGuid(),
                    FrameworkCode = record.FrameworkCode,
                    ProgType = record.ProgType,
                    PathwayCode = record.PathwayCode,
                    CreatedDateTimeUtc = request.Now
                };

                framework.NasTitle = record.NasTitle;
                framework.EffectiveFrom = record.EffectiveFrom;
                framework.EffectiveTo = record.EffectiveTo;
                framework.SectorSubjectAreaTier1 = record.SectorSubjectAreaTier1;
                framework.SectorSubjectAreaTier2 = record.SectorSubjectAreaTier2;
                framework.ModifiedDateTimeUtc = request.Now;
                framework.RecordStatusId = 2;  // Live

                if (existingRecord != null)
                {
                    var documentUri = UriFactory.CreateDocumentUri(
                        configuration.DatabaseId,
                        configuration.FrameworksCollectionName,
                        existingRecord.Id.ToString());

                    await client.ReplaceDocumentAsync(documentUri, framework);
                }
                else
                {
                    await client.CreateDocumentAsync(collectionUri, framework);
                }

                async Task<Framework> FindExistingRecord()
                {
                    return (await client.CreateDocumentQuery<Framework>(collectionUri)
                            .Where(s => s.FrameworkCode == record.FrameworkCode && s.ProgType == record.ProgType && s.PathwayCode == record.PathwayCode)
                            .AsDocumentQuery()
                            .FetchAll())
                        .SingleOrDefault();
                }
            }

            return new None();
        }
    }
}
