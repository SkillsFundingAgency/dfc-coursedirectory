using System.Threading.Tasks;
using UkrlpService;

namespace Dfc.CourseDirectory.Core.ReferenceData.Ukrlp
{
    public interface IUkrlpService
    {
       Task<ProviderRecordStructure> GetProviderData(int ukprn);
    }
}
