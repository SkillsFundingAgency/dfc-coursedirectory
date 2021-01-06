using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models;

namespace Dfc.CourseDirectory.FindAnApprenticeshipApi.Interfaces.Services
{
    public interface IReferenceDataService
    {
        Task<IEnumerable<FeChoice>> GetAllFeChoicesAsync();
    }
}