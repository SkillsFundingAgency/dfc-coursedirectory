using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.WebV2
{
    public interface IStandardsCache
    {
        void Clear();
        Task<IReadOnlyCollection<Standard>> GetAllStandards();
        Task<Standard> GetStandard(int standardCode, int version);
    }
}
