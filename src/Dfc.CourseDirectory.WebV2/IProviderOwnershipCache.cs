using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2
{
    public interface IProviderOwnershipCache
    {
        Task<Guid?> GetProviderForApprenticeship(Guid apprenticeshipId);
        Task<Guid?> GetProviderForCourse(Guid courseId);
        void OnApprenticeshipDeleted(Guid apprenticeshipId);
        void OnCourseDeleted(Guid courseId);
    }
}
