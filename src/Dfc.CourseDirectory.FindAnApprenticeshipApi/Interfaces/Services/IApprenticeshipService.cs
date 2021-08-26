using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.DAS;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Services
{
    public interface IApprenticeshipService
    {
        Task<IEnumerable<Apprenticeship>> GetLiveApprenticeships();
        Task<IEnumerable<Apprenticeship>> GetApprenticeshipsByUkprn(int ukprn);
        Task<IEnumerable<DasProviderResult>> ApprenticeshipsToDasProviders(List<Apprenticeship> apprenticeships);
    }
}
