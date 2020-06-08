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
    public class UpsertStandardsHandler : ICosmosDbQueryHandler<UpsertStandards, None>
    {
        public async Task<None> Execute(DocumentClient client, Configuration configuration, UpsertStandards request)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.StandardsCollectionName);

            foreach (var record in request.Records)
            {
                var existingRecord = await FindExistingRecord();

                var standard = existingRecord ?? new Standard()
                {
                    Id = Guid.NewGuid(),
                    StandardCode = record.StandardCode,
                    Version = record.Version
                };

                standard.StandardCode = record.StandardCode;
                standard.Version = record.Version;
                standard.StandardName = record.StandardName;
                standard.StandardSectorCode = record.StandardSectorCode;
                standard.NotionalEndLevel = record.NotionalEndLevel;
                standard.EffectiveFrom = record.EffectiveFrom;
                standard.EffectiveTo = record.EffectiveTo;
                standard.URLLink = record.URLLink;
                standard.SectorSubjectAreaTier1 = record.SectorSubjectAreaTier1;
                standard.SectorSubjectAreaTier2 = record.SectorSubjectAreaTier2;
                standard.OtherBodyApprovalRequired = record.OtherBodyApprovalRequired;
                standard.RecordStatusId = 2;  // Live

                if (existingRecord != null)
                {
                    var documentUri = UriFactory.CreateDocumentUri(
                        configuration.DatabaseId,
                        configuration.StandardsCollectionName,
                        existingRecord.Id.ToString());

                    await client.ReplaceDocumentAsync(documentUri, standard);
                }
                else
                {
                    await client.CreateDocumentAsync(collectionUri, standard);
                }

                async Task<Standard> FindExistingRecord()
                {
                    return (await client.CreateDocumentQuery<Standard>(collectionUri)
                            .Where(s => s.StandardCode == record.StandardCode && s.Version == record.Version)
                            .AsDocumentQuery()
                            .FetchAll())
                        .SingleOrDefault();
                }
            }

            return new None();
        }
    }
}
