using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CosmosBulkUtils.Config;

namespace Dfc.CosmosBulkUtils.Services
{
    public interface IContainerService
    {
        Task Update(Guid id, object payload);
        Task<bool> Delete(Guid id);
        Task<object> Get(Guid id);

        Task<bool> Exists(Guid id);

        Task<bool> Patch(PatchConfig config);

        Task<bool> Add(string id, IDictionary<string, object> document, string partitionKey);

        CosmosDbSettings GetSettings();
    }
}
