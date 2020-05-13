using System;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class UpsertStandardsHandler : ICosmosDbQueryHandler<UpsertStandards, None>
    {
        public None Execute(InMemoryDocumentStore inMemoryDocumentStore, UpsertStandards request)
        {
            foreach (var record in request.Records)
            {
                inMemoryDocumentStore.Standards.CreateOrUpdate(
                    d => d.StandardCode == record.StandardCode && d.Version == record.Version,
                    () => new Core.DataStore.CosmosDb.Models.Standard()
                    {
                        Id = Guid.NewGuid(),
                        StandardCode = record.StandardCode,
                        Version = record.Version
                    },
                    standard =>
                    {
                        standard.NotionalEndLevel = record.NotionalEndLevel;
                        standard.OtherBodyApprovalRequired = record.OtherBodyApprovalRequired;
                        standard.StandardName = record.StandardName;
                        standard.RecordStatusId = 2;  // Live
                    });
            }

            return new None();
        }
    }
}
