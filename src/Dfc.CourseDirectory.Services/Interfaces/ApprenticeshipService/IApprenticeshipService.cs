using System;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService
{
    public interface IApprenticeshipService
    {
        Task<IResult<IEnumerable<IStandardsAndFrameworks>>> StandardsAndFrameworksSearch(string criteria, int UKPRN);
        Task<IResult> AddApprenticeship(IApprenticeship apprenticeship);
        Task<IResult<IEnumerable<IApprenticeship>>> GetApprenticeshipByUKPRN(string criteria);

        Task<IResult<IApprenticeship>> GetApprenticeshipByIdAsync(string Id);
        Task<IResult<IEnumerable<IStandardsAndFrameworks>>> GetStandardByCode(StandardSearchCriteria criteria);
        Task<IResult<IEnumerable<IStandardsAndFrameworks>>> GetFrameworkByCode(FrameworkSearchCriteria criteria);
        Task<IResult<IApprenticeship>> UpdateApprenticeshipAsync(IApprenticeship apprenticeship);
        Task<IResult> DeleteBulkUploadApprenticeships(int UKPRN);
        Task<IResult> ChangeApprenticeshipStatusesForUKPRNSelection(int ukprn, int currentStatus,
            int statusToBeChangedTo);
        Task<IResult<ApprenticeshipDashboardCounts>> GetApprenticeshipDashboardCounts(int UKPRN);
    }
}
