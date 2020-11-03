using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Providers;

namespace Dfc.CourseDirectory.Services.Interfaces.ProviderService
{
    public interface IProviderService
    {
        Task<IResult<IEnumerable<Provider>>> GetProviderByPRNAsync(string prn);
        Task<IResult<Provider>> AddProviderAsync(IProviderAdd provider);
        Task<IResult> UpdateProviderDetails(Provider provider);
    }
}
