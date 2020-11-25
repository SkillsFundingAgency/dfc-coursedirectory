using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.Providerportal.FindAnApprenticeship.Interfaces.Apprenticeships;
using Dfc.Providerportal.FindAnApprenticeship.Models;
using Dfc.Providerportal.FindAnApprenticeship.Models.DAS;

namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.Services
{
    public interface IApprenticeshipService
    {
        Task<IEnumerable<IApprenticeship>> GetApprenticeshipCollection();
        Task<IEnumerable<IApprenticeship>> GetLiveApprenticeships();
        Task<IEnumerable<IApprenticeship>> GetApprenticeshipsByUkprn(int ukprn);
        Task<IEnumerable<DasProviderResult>> ApprenticeshipsToDasProviders(List<Apprenticeship> apprenticeships);
    }
}