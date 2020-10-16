using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2
{
    public interface IProviderOwnershipCache
    {
        Task<Guid?> GetProviderForApprenticeship(Guid apprenticeshipId);
        Task<Guid?> GetProviderForCourse(Guid courseId);
        Task<Guid?> GetProviderForVenue(Guid venueId);
        void OnApprenticeshipDeleted(Guid apprenticeshipId);
        void OnCourseDeleted(Guid courseId);
        void OnVenueDeleted(Guid venueId);
    }
}
