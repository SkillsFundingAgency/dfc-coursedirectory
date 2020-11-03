using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Venues;

namespace Dfc.CourseDirectory.Services.Interfaces.VenueService
{
    public interface IVenueService
    {
        Task<IResult<Venue>> GetVenueByIdAsync(IGetVenueByIdCriteria criteria);
        Task<IResult<Venue>> GetVenueByVenueIdAsync(IGetVenueByVenueIdCriteria criteria);
        Task<IResult<Venue>> GetVenueByLocationIdAsync(IGetVenueByLocationIdCriteria criteria);
        Task<IResult<IVenueSearchResult>> GetVenuesByPRNAndNameAsync(IGetVenuesByPRNAndNameCriteria criteria);
        Task<IResult<IVenueSearchResult>> SearchAsync(IVenueSearchCriteria criteria);
        Task<IResult<Venue>> AddAsync(Venue venue);
        Task<IResult<Venue>> UpdateAsync(Venue venue);
    }
}
