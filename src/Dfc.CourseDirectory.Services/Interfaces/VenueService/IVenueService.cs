using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Venues;

namespace Dfc.CourseDirectory.Services.Interfaces.VenueService
{
    public interface IVenueService
    {
        Task<Result<Venue>> GetVenueByIdAsync(IGetVenueByIdCriteria criteria);
        Task<Result<Venue>> GetVenueByVenueIdAsync(IGetVenueByVenueIdCriteria criteria);
        Task<Result<Venue>> GetVenueByLocationIdAsync(IGetVenueByLocationIdCriteria criteria);
        Task<Result<IVenueSearchResult>> GetVenuesByPRNAndNameAsync(IGetVenuesByPRNAndNameCriteria criteria);
        Task<Result<IVenueSearchResult>> SearchAsync(IVenueSearchCriteria criteria);
        Task<Result<Venue>> AddAsync(Venue venue);
        Task<Result<Venue>> UpdateAsync(Venue venue);
    }
}
