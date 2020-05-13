using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class UpsertStandardSectorCodesHandler : ICosmosDbQueryHandler<UpsertStandardSectorCodes, None>
    {
        public None Execute(InMemoryDocumentStore inMemoryDocumentStore, UpsertStandardSectorCodes request)
        {
            foreach (var record in request.Records)
            {
                inMemoryDocumentStore.StandardSectorCodes.CreateOrUpdate(
                    d => d.StandardSectorCodeId == record.StandardSectorCodeId,
                    () => new Core.DataStore.CosmosDb.Models.StandardSectorCode()
                    {
                        Id = Guid.NewGuid(),
                        StandardSectorCodeId = record.StandardSectorCodeId,
                        CreatedDateTimeUtc = request.Now
                    },
                    ssc =>
                    {
                        ssc.EffectiveFrom = record.EffectiveFrom;
                        ssc.EffectiveTo = record.EffectiveTo;
                        ssc.ModifiedDateTimeUtc = request.Now;
                        ssc.StandardSectorCodeDesc = record.StandardSectorCodeDesc;
                        ssc.StandardSectorCodeDesc2 = record.StandardSectorCodeDesc2;
                    });
            }

            return new None();
        }
    }
}
