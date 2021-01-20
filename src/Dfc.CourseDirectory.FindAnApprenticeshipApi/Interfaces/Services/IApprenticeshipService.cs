using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Apprenticeships;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.DAS;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Services
{
    public interface IApprenticeshipService
    {
        Task<IEnumerable<IApprenticeship>> GetApprenticeshipCollection();
        Task<IEnumerable<IApprenticeship>> GetLiveApprenticeships();
        Task<IEnumerable<IApprenticeship>> GetApprenticeshipsByUkprn(int ukprn);
        Task<IEnumerable<DasProviderResult>> ApprenticeshipsToDasProviders(List<Apprenticeship> apprenticeships);
    }
}