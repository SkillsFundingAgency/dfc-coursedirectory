using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.Providers;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Helper
{
    public interface IProviderServiceClient
    {
        Task<IEnumerable<Provider>> GetAllProviders();
    }
}