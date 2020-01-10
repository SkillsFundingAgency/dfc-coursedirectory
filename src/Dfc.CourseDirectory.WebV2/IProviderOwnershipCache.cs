using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2
{
    public interface IProviderOwnershipCache
    {
        Task<int?> GetProviderForApprenticeship(Guid apprenticeshipId);
        void OnApprenticeshipDeleted(Guid apprenticeshipId);
    }
}
