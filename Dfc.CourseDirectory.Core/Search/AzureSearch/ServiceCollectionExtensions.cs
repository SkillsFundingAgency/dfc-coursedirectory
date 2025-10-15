using System;
using Azure;
using Azure.Search.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Core.Search.AzureSearch
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureSearchClient<TResult>(
            this IServiceCollection services,
            Uri endpoint,
            string key,
            string indexName,
            Action<SearchClientOptions> configureOptions = null)
        {
            var options = new SearchClientOptions();
            configureOptions?.Invoke(options);

            return services.AddSingleton<ISearchClient<TResult>>(s =>
                new AzureSearchClient<TResult>(
                    new SearchClient(endpoint, indexName, new AzureKeyCredential(key), options)));
        }
    }
}
