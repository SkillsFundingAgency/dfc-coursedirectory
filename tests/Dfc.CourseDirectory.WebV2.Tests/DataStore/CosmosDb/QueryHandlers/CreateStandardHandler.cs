﻿using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
{
    public class CreateStandardHandler : ICosmosDbQueryHandler<CreateStandard, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, CreateStandard request)
        {
            var Standard = new Standard()
            {
                Id = request.Id,
                StandardCode = request.StandardCode,
                Version = request.Version,
                StandardName = request.StandardName,
                NotionalEndLevel = request.NotionalEndLevel,
                OtherBodyApprovalRequired = request.OtherBodyApprovalRequired,
                RecordStatusId = 2
            };
            inMemoryDocumentStore.Standards.Save(Standard);

            return new Success();
        }
    }
}
