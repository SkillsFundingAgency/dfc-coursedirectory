using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Providers;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.Interfaces.ProviderService
{
    public interface IProviderService
    {
        Task<IResult<IProviderSearchResult>> GetProviderByPRNAsync(IProviderSearchCriteria criteria);
        Task<IResult<IProvider>> AddProviderAsync(IProviderAdd provider);
        Task<IResult> UpdateProviderDetails(IProvider provider);
    }
}
