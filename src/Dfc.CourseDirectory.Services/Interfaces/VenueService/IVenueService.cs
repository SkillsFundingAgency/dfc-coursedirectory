using Dfc.CourseDirectory.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.Interfaces.VenueService
{
    public interface IVenueService
    {
        Task<IResult<IGetVenueByIdResult>> GetVenueByIdAsync(IGetVenueByIdCriteria criteria);

        Task<IResult<IVenueSearchResult>> SearchAsync(IVenueSearchCriteria criteria);

        Task<IResult<IVenueAddResultItem>> AddAsync(IVenueAdd venue);

        Task<IResult<IUpdatedVenueResult>> UpdateAsync(IUpdatedVenue venue);


    }
}
