using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class UpsertSectorSubjectAreaTier2sHandler : ICosmosDbQueryHandler<UpsertSectorSubjectAreaTier2s, None>
    {
        public None Execute(InMemoryDocumentStore inMemoryDocumentStore, UpsertSectorSubjectAreaTier2s request)
        {
            foreach (var record in request.Records)
            {
                inMemoryDocumentStore.SectorSubjectAreaTier2s.CreateOrUpdate(
                    d => d.SectorSubjectAreaTier2Id == record.SectorSubjectAreaTier2Id,
                    () => new Core.DataStore.CosmosDb.Models.SectorSubjectAreaTier2()
                    {
                        Id = Guid.NewGuid(),
                        SectorSubjectAreaTier2Id = record.SectorSubjectAreaTier2Id,
                        CreatedDateTimeUtc = request.Now
                    },
                    s =>
                    {
                        s.EffectiveFrom = record.EffectiveFrom;
                        s.EffectiveTo = record.EffectiveTo;
                        s.ModifiedDateTimeUtc = request.Now;
                        s.SectorSubjectAreaTier2Desc = record.SectorSubjectAreaTier2Desc;
                        s.SectorSubjectAreaTier2Desc2 = record.SectorSubjectAreaTier2Desc2;
                    });
            }

            return new None();
        }
    }
}
