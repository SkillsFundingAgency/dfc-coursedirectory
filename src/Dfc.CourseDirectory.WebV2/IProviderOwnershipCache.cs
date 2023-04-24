using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2
{
    public interface IProviderOwnershipCache
    {
        Task<Guid?> GetProviderForCourse(Guid courseId);
        Task<Guid?> GetProviderForTLevel(Guid tLevelId);
        Task<Guid?> GetProviderForVenue(Guid venueId);
        void OnCourseDeleted(Guid courseId);
        void OnTLevelDeleted(Guid tLevelId);
        void OnVenueDeleted(Guid venueId);
    }
}
