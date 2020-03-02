using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2
{
    public interface IProviderOwnershipCache
    {
        Task<Guid?> GetProviderForApprenticeship(Guid apprenticeshipId);
        void OnApprenticeshipDeleted(Guid apprenticeshipId);
    }
}
