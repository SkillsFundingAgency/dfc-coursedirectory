using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Services.ApprenticeshipService
{
    public interface IApprenticeshipService
    {
        Task<Result<Apprenticeship>> UpdateApprenticeshipAsync(Apprenticeship apprenticeship);
        Task<Result> ChangeApprenticeshipStatusesForUKPRNSelection(int ukprn, int currentStatus, int statusToBeChangedTo);
    }
}
