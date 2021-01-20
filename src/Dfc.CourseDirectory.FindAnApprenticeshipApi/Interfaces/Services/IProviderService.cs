using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.Providers;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Services
{
    public interface IProviderService
    {
        Task<IEnumerable<Provider>> GetActiveProvidersAsync();
    }
}