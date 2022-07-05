using System;
using System.Threading.Tasks;

namespace Dfc.CosmosBulkUtils.Services
{
    public interface IContainerService
    {
        Task Update(Guid id, object payload);
        Task Delete(Guid id);
        Task<object> Get(Guid id);
    }
}
