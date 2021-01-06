using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Helper
{
    public interface IReferenceDataServiceClient
    {
        Task<IEnumerable<FeChoice>> GetAllFeChoiceData();
    }
}