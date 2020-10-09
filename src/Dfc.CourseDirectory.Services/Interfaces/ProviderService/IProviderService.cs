using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Providers;
using Dfc.CourseDirectory.Models.Models.Providers;

namespace Dfc.CourseDirectory.Services.Interfaces.ProviderService
{
    public interface IProviderService
    {
        Task<IResult<IEnumerable<Provider>>> GetProviderByPRNAsync(string prn);
        Task<IResult<IProvider>> AddProviderAsync(IProviderAdd provider);
        Task<IResult> UpdateProviderDetails(IProvider provider);
    }
}