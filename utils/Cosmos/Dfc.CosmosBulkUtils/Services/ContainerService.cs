using System;
using System.Collections.Generic;
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
                        new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway })
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
            var results = new List<CosmosRecord>();
            var queryResults = _container.GetItemQueryIterator<CosmosRecord>(config.FilterPredicate);
            while (queryResults.HasMoreResults)
            {
                var currentResults = await queryResults.ReadNextAsync();
                results.AddRange(currentResults);
                _logger.LogInformation("Found {0}", results.Count);
            }

            var patchOperations = (from o in config.Operations
                                  select PatchOperation.Add(o.Field, o.Value)).ToList();

            foreach (var item in results)
            {
                var response = await _container.PatchItemAsync<object>(
                    id: item.Id,
                    partitionKey: PartitionKey.None,
                    patchOperations: patchOperations

                    );
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("Patched OK for {0}", item.Id);
                }
                else
                {
                    _logger.LogError("Patch failed for {0} with statuscode {1}", item.Id, response.StatusCode);
                }
            }

            return true;
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
