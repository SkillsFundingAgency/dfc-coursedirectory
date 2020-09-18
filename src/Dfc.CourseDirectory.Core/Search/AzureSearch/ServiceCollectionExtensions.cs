using System;
using Azure;
using Azure.Search.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Core.Search.AzureSearch
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureSearchClient<TQuery, TResult>(this IServiceCollection services, Uri endpoint, string key, string indexName)
            where TQuery : IAzureSearchQuery
        {
            services.AddSingleton<ISearchClient<TQuery, TResult>>(s =>
                new AzureSearchClient<TQuery, TResult>(
                    new SearchClient(endpoint, indexName, new AzureKeyCredential(key))));

            return services;
        }
    }
}