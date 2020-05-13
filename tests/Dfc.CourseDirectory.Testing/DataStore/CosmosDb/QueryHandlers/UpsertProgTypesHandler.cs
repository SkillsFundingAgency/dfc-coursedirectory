using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class UpsertProgTypesHandler : ICosmosDbQueryHandler<UpsertProgTypes, None>
    {
        public None Execute(InMemoryDocumentStore inMemoryDocumentStore, UpsertProgTypes request)
        {
            foreach (var record in request.Records)
            {
                inMemoryDocumentStore.ProgTypes.CreateOrUpdate(
                    d => d.ProgTypeId == record.ProgTypeId,
                    () => new Core.DataStore.CosmosDb.Models.ProgType()
                    {
                        Id = Guid.NewGuid(),
                        ProgTypeId = record.ProgTypeId,
                        CreatedDateTimeUtc = request.Now
                    },
                    progType =>
                    {
                        progType.EffectiveFrom = record.EffectiveFrom;
                        progType.EffectiveTo = record.EffectiveTo;
                        progType.ModifiedDateTimeUtc = request.Now;
                        progType.ProgTypeDesc = record.ProgTypeDesc;
                        progType.ProgTypeDesc2 = record.ProgTypeDesc2;
                    });
            }

            return new None();
        }
    }
}
