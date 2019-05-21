
using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Venues;


namespace Dfc.CourseDirectory.Services.Interfaces.VenueService
{
    public interface IVenueService
    {
        Task<IResult<IVenue>> GetVenueByIdAsync(IGetVenueByIdCriteria criteria);
        Task<IResult<IVenue>> GetVenueByVenueIdAsync(IGetVenueByVenueIdCriteria criteria);
        Task<IResult<IVenue>> GetVenueByLocationIdAsync(IGetVenueByLocationIdCriteria criteria);
        Task<IResult<IVenueSearchResult>> GetVenuesByPRNAndNameAsync(IGetVenuesByPRNAndNameCriteria criteria);
        Task<IResult<IVenueSearchResult>> SearchAsync(IVenueSearchCriteria criteria);
        Task<IResult<IVenue>> AddAsync(IVenue venue);
        Task<IResult<IVenue>> UpdateAsync(IVenue venue);
    }
}
