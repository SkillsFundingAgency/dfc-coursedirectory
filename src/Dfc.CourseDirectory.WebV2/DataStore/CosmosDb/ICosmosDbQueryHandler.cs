﻿using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb
{
    public interface ICosmosDbQueryHandler<TRequest, TResult>
        where TRequest : ICosmosDbQuery<TResult>
    {
        Task<TResult> Execute(DocumentClient client, Configuration configuration, TRequest request);
    }
}
