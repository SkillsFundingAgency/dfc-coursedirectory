using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class UpsertSectorSubjectAreaTier1sHandler : ICosmosDbQueryHandler<UpsertSectorSubjectAreaTier1s, None>
    {
        public None Execute(InMemoryDocumentStore inMemoryDocumentStore, UpsertSectorSubjectAreaTier1s request)
        {
            foreach (var record in request.Records)
            {
                inMemoryDocumentStore.SectorSubjectAreaTier1s.CreateOrUpdate(
                    d => d.SectorSubjectAreaTier1Id == record.SectorSubjectAreaTier1Id,
                    () => new Core.DataStore.CosmosDb.Models.SectorSubjectAreaTier1()
                    {
                        Id = Guid.NewGuid(),
                        SectorSubjectAreaTier1Id = record.SectorSubjectAreaTier1Id,
                        CreatedDateTimeUtc = request.Now
                    },
                    s =>
                    {
                        s.EffectiveFrom = record.EffectiveFrom;
                        s.EffectiveTo = record.EffectiveTo;
                        s.ModifiedDateTimeUtc = request.Now;
                        s.SectorSubjectAreaTier1Desc = record.SectorSubjectAreaTier1Desc;
                        s.SectorSubjectAreaTier1Desc2 = record.SectorSubjectAreaTier1Desc2;
                    });
            }

            return new None();
        }
    }
}
