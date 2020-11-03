using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Apprenticeships;
using Dfc.CourseDirectory.Services.Models.Courses;

namespace Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService
{
    public interface IApprenticeshipService
    {
        Task<IResult<IEnumerable<StandardsAndFrameworks>>> StandardsAndFrameworksSearch(string criteria, int UKPRN);
        Task<IResult> AddApprenticeship(Apprenticeship apprenticeship);
        Task<IResult> AddApprenticeships(IEnumerable<Apprenticeship> apprenticeships, bool addInParallel);
        Task<IResult<IEnumerable<Apprenticeship>>> GetApprenticeshipByUKPRN(string criteria);
        Task<IResult<Apprenticeship>> GetApprenticeshipByIdAsync(string Id);
        Task<IResult<IEnumerable<StandardsAndFrameworks>>> GetStandardByCode(StandardSearchCriteria criteria);
        Task<IResult<IEnumerable<StandardsAndFrameworks>>> GetFrameworkByCode(FrameworkSearchCriteria criteria);
        Task<IResult<Apprenticeship>> UpdateApprenticeshipAsync(Apprenticeship apprenticeship);
        Task<IResult> DeleteBulkUploadApprenticeships(int UKPRN);
        Task<IResult> ChangeApprenticeshipStatusesForUKPRNSelection(int ukprn, int currentStatus, int statusToBeChangedTo);
        Task<IResult<ApprenticeshipDashboardCounts>> GetApprenticeshipDashboardCounts(int UKPRN);
        Task<IResult<IList<DfcMigrationReport>>> GetAllDfcReports();
        Task<IResult<int>> GetTotalLiveApprenticeships();
    }
}
