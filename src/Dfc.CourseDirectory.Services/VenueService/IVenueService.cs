using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Venues;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public interface IVenueService
    {
        Task<Result<Venue>> GetVenueByIdAsync(GetVenueByIdCriteria criteria);
        Task<Result<VenueSearchResult>> GetVenuesByPRNAndNameAsync(GetVenuesByPRNAndNameCriteria criteria);
        Task<Result<VenueSearchResult>> SearchAsync(VenueSearchCriteria criteria);
        Task<Result<Venue>> AddAsync(Venue venue);
        Task<Result<Venue>> UpdateAsync(Venue venue);
    }
}
