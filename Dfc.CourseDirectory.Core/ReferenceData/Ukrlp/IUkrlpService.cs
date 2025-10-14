using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UkrlpService;

namespace Dfc.CourseDirectory.Core.ReferenceData.Ukrlp
{
    public interface IUkrlpService
    {
       Task<IReadOnlyCollection<ProviderRecordStructure>> GetAllProviderData(DateTime updatedSince);
       Task<IReadOnlyDictionary<int, ProviderRecordStructure>> GetProviderData(IEnumerable<int> ukprns);
    }
}
