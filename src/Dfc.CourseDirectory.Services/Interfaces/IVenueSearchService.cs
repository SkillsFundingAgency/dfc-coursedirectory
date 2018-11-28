using Dfc.CourseDirectory.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IVenueSearchService
    {
        Task<IResult<IVenueSearchResult>> SearchAsync(IVenueSearchCriteria criteria);
    }
}
