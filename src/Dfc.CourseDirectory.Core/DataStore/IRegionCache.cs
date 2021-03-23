using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore
{
    public interface IRegionCache
    {
        Task<IReadOnlyCollection<Region>> GetAllRegions();
    }
}
