using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Providers;

namespace Dfc.CourseDirectory.Services.ProviderService
{
    public interface IProviderService
    {
        Task<Result<IEnumerable<Provider>>> GetProviderByPRNAsync(string prn);
        Task<Result<Provider>> AddProviderAsync(ProviderAdd provider);
        Task<Result> UpdateProviderDetails(Provider provider);
    }
}
