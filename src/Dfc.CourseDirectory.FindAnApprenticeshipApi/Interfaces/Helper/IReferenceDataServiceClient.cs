using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.Providerportal.FindAnApprenticeship.Models;

namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.Helper
{
    public interface IReferenceDataServiceClient
    {
        Task<IEnumerable<FeChoice>> GetAllFeChoiceData();
    }
}