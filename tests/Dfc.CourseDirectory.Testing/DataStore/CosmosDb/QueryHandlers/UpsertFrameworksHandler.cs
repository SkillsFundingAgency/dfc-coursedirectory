using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class UpsertFrameworksHandler : ICosmosDbQueryHandler<UpsertFrameworks, None>
    {
        public None Execute(InMemoryDocumentStore inMemoryDocumentStore, UpsertFrameworks request)
        {
            foreach (var record in request.Records)
            {
                inMemoryDocumentStore.Frameworks.CreateOrUpdate(
                    d => d.FrameworkCode == record.FrameworkCode &&
                        d.ProgType == record.ProgType &&
                        d.PathwayCode == record.PathwayCode,
                    () => new Core.DataStore.CosmosDb.Models.Framework()
                    {
                        Id = Guid.NewGuid(),
                        FrameworkCode = record.FrameworkCode,
                        ProgType = record.ProgType,
                        PathwayCode = record.PathwayCode,
                        CreatedDateTimeUtc = request.Now,
                    },
                    framework =>
                    {
                        framework.EffectiveFrom = record.EffectiveFrom;
                        framework.EffectiveTo = record.EffectiveTo;
                        framework.ModifiedDateTimeUtc = request.Now;
                        framework.NasTitle = record.NasTitle;
                        framework.RecordStatusId = 2;  // Live
                        framework.SectorSubjectAreaTier1 = record.SectorSubjectAreaTier1;
                        framework.SectorSubjectAreaTier2 = record.SectorSubjectAreaTier2;
                    });
            }

            return new None();
        }
    }
}
