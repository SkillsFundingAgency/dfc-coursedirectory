using System;
using System.Threading.Tasks;
using Dfc.CosmosBulkUtils.Config;
using Dfc.CosmosBulkUtils.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        public Task Delete(Guid id)
        {
            _logger.LogInformation("Deleting {id}", id);
            return Task.CompletedTask;
        }

        public async Task<object> Get(Guid id)
        {
            _logger.LogInformation("Getting {id}", id);
            var response = await _container.ReadItemAsync<object>(id.ToString(), PartitionKey.None);
            return response.StatusCode.IsSuccessStatusCode()
                ? response.Resource
                : throw new ApplicationException($"Failed to get record {response.StatusCode.ToString()}");
        }
    }
}
