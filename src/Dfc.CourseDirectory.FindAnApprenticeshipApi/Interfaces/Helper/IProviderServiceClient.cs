using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.Providerportal.FindAnApprenticeship.Models.Providers;

namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.Helper
{
    public interface IProviderServiceClient
    {
        Task<IEnumerable<Provider>> GetAllProviders();
    }
}