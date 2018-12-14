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


    }
}
