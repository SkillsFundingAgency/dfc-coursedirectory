using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dfc.CosmosBulkUtils.Config;
using Dfc.CosmosBulkUtils.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Dfc.CosmosBulkUtils.Services
{
    public class ContainerService : IContainerService
    {
        private readonly Container _container;
        private readonly ILogger<ContainerService> _logger;
        private readonly CosmosDbSettings _settings;

        public ContainerService(ILogger<ContainerService> logger, IOptions<CosmosDbSettings> settings)
        {
            _settings = settings.Value;
            _logger = logger;
            _container =
                new CosmosClient(_settings.EndpointUrl, _settings.AccessKey,
                        new CosmosClientOptions
                        {
                            ConnectionMode = ConnectionMode.Gateway, 
                            AllowBulkExecution = false, 
                            MaxRetryAttemptsOnRateLimitedRequests = 200, 
                            MaxRetryWaitTimeOnRateLimitedRequests = new TimeSpan(0, 10, 0)
                        })
                    .GetDatabase(_settings.DatabaseId).GetContainer(_settings.ContainerId);
        }

        public async Task Update(Guid id,object item)
        {
            _logger.LogInformation("Updating {id}", id);
            var response = await _container.UpsertItemAsync(item, PartitionKey.None);

            if (!response.StatusCode.IsSuccessStatusCode())
            {
                throw new ApplicationException($"Failed to update record {response.StatusCode.ToString()}");

            }
        }

        public async Task<bool> Delete(Guid id)
        {
            _logger.LogInformation("Deleting {id}", id);


            if (!await Exists(id))
            {
                _logger.LogWarning("Skipping {id} not found", id);
                return false;
            }

            var response = await _container.DeleteItemAsync<object>(id.ToString(), PartitionKey.None);

            if (response.StatusCode.IsSuccessStatusCode())
            {
                _logger.LogInformation("Deleted {id}", id);
                return true;
            }

            return false;
        }

        public async Task<bool> Add(string filename, IDictionary<string, object> document, string partitionKey)
        {
            var response = await _container.CreateItemAsync<IDictionary<string,object>>(document);

            if (response.StatusCode.IsSuccessStatusCode())
            {
                _logger.LogInformation("Added {filename}", filename);
                return true;
            }

            return false;
        }

        public async Task<object> Get(Guid id)
        {
            _logger.LogInformation("Getting {id}", id);
            var response = await _container.ReadItemAsync<object>(id.ToString(), PartitionKey.None);
            return response.StatusCode.IsSuccessStatusCode()
                ? response.Resource
                : throw new ApplicationException($"Failed to get record {response.StatusCode.ToString()}");
        }

        public async Task<bool> Exists(Guid id)
        {
            var response = await _container.ReadItemStreamAsync(id.ToString(), PartitionKey.None);


            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return false;
                default:
                    return response.IsSuccessStatusCode;
            }

        }

        public async Task<bool> Patch(PatchConfig config)
        {          
            var stopWatch = Stopwatch.StartNew();
            var results = await GetCosmosRecordIds(config);


            var patchOperations = (from o in config.Operations
                                  select PatchOperation.Replace(o.Field, o.Value)).ToList();

            var tasks = new List<Task<(ItemResponse<object>?, Exception?)>>();

            _logger.LogInformation("Processing....");
            var success = 0;
            var failed = new Dictionary<string,string>();

            for (int i = 0; i < results.Count; i++)
            {
                var item = results[i];

                var result = await _container.PatchItemAsync<object>(
                    id: item.Id,
                    partitionKey: String.IsNullOrEmpty(config.PartitionKeyValue) ? PartitionKey.None : new PartitionKey(config.PartitionKeyValue),
                    patchOperations: patchOperations
                );

                if (result.StatusCode.IsSuccessStatusCode())
                {
                    success++;
                }
                else
                {
                    failed.Add(item.Id, result.StatusCode.ToString());
                }

            }

            foreach (var key in failed.Keys)
            {
                _logger.LogError("{0} - {1}", key, failed[key]);
            }


            _logger.LogInformation("Complete Elapsed = {0}", stopWatch.Elapsed);

            return true;
        }

        private async Task<List<CosmosRecord>> GetCosmosRecordIds(PatchConfig config)
        {
            var results = new List<CosmosRecord>();

            if (config.FilterIds.Any())
            {
                results.AddRange(config.FilterIds.Select(id => new CosmosRecord { Id = id.ToLower() }));
                return results;
            }

            var queryResults = _container.GetItemQueryIterator<CosmosRecord>(config.FilterPredicate);
            while (queryResults.HasMoreResults)
            {
                var currentResults = await queryResults.ReadNextAsync();
                results.AddRange(currentResults);
                _logger.LogInformation("Found {0}", results.Count);
            }

            _logger.LogInformation("Total of {0} records found", results.Count);

            return results;
        }

        public CosmosDbSettings GetSettings()
        {
            return new CosmosDbSettings
            {
                DatabaseId = _settings.DatabaseId,
                ContainerId = _settings.ContainerId,
                EndpointUrl = _settings.EndpointUrl,
                AccessKey = "XXXXXXXXXX"

            };
        }
    }
}
