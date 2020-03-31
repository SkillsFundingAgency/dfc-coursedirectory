using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2
{
    public interface IStandardsAndFrameworksCache
    {
        void Clear();
        Task<IReadOnlyCollection<Framework>> GetAllFrameworks();
        Task<IReadOnlyCollection<Standard>> GetAllStandards();
        Task<Framework> GetFramework(int frameworkCode, int progType, int pathwayCode);
        Task<Standard> GetStandard(int standardCode, int version);
    }
}
