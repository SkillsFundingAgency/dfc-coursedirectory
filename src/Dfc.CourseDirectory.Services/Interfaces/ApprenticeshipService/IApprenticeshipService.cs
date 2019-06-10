using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService
{
    public interface IApprenticeshipService
    {
        Task<IResult<IEnumerable<IStandardsAndFrameworks>>> StandardsAndFrameworksSearch(string criteria);
        Task<IResult<IApprenticeship>> AddApprenticeship(IApprenticeship apprenticeship);
        Task<IResult<IEnumerable<IApprenticeship>>> GetApprenticeshipByUKPRN(string criteria);
    }
}
