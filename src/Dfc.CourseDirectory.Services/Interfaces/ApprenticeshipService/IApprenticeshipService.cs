using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Apprenticeships;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService
{
    public interface IApprenticeshipService
    {
        Task<Result<IEnumerable<StandardsAndFrameworks>>> StandardsAndFrameworksSearch(string criteria, int UKPRN);
        Task<Result> AddApprenticeship(Apprenticeship apprenticeship);
        Task<Result> AddApprenticeships(IEnumerable<Apprenticeship> apprenticeships, bool addInParallel);
        Task<Result<IEnumerable<Apprenticeship>>> GetApprenticeshipByUKPRN(string criteria);
        Task<Result<Apprenticeship>> GetApprenticeshipByIdAsync(string Id);
        Task<Result<IEnumerable<StandardsAndFrameworks>>> GetStandardByCode(StandardSearchCriteria criteria);
        Task<Result<IEnumerable<StandardsAndFrameworks>>> GetFrameworkByCode(FrameworkSearchCriteria criteria);
        Task<Result<Apprenticeship>> UpdateApprenticeshipAsync(Apprenticeship apprenticeship);
        Task<Result> DeleteBulkUploadApprenticeships(int UKPRN);
        Task<Result> ChangeApprenticeshipStatusesForUKPRNSelection(int ukprn, int currentStatus, int statusToBeChangedTo);
        Task<Result<ApprenticeshipDashboardCounts>> GetApprenticeshipDashboardCounts(int UKPRN);
        Task<Result<IList<DfcMigrationReport>>> GetAllDfcReports();
        Task<Result<int>> GetTotalLiveApprenticeships();
    }
}
