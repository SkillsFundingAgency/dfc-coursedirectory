using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Providers;

namespace Dfc.CourseDirectory.Services.Interfaces.ProviderService
{
    public interface IProviderService
    {
        Task<Result<IEnumerable<Provider>>> GetProviderByPRNAsync(string prn);
        Task<Result<Provider>> AddProviderAsync(IProviderAdd provider);
        Task<Result> UpdateProviderDetails(Provider provider);
    }
}
