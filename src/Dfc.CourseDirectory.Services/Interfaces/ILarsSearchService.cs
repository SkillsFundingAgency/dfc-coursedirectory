using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface ILarsSearchService
    {
        ILarsSearchResult Search(ILarsSearchCriteria criteria);
        Task<ILarsSearchResult> SearchAsync(ILarsSearchCriteria criteria);
    }
}
