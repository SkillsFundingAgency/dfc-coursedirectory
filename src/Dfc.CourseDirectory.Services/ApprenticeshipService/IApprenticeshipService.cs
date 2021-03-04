using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Services.ApprenticeshipService
{
    public interface IApprenticeshipService
    {
        Task<Result> AddApprenticeship(Apprenticeship apprenticeship);
        Task<Result> AddApprenticeships(IEnumerable<Apprenticeship> apprenticeships, bool addInParallel);
        Task<Result<IEnumerable<Apprenticeship>>> GetApprenticeshipByUKPRN(string criteria);
        Task<Result<Apprenticeship>> UpdateApprenticeshipAsync(Apprenticeship apprenticeship);
        Task<Result> DeleteBulkUploadApprenticeships(int UKPRN);
        Task<Result> ChangeApprenticeshipStatusesForUKPRNSelection(int ukprn, int currentStatus, int statusToBeChangedTo);
    }
}
