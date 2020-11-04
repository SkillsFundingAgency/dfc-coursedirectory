using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.AddressSearch
{
    public interface IAddressSearchService
    {
        Task<AddressDetail> GetById(string id);
        Task<IReadOnlyCollection<PostcodeSearchResult>> SearchByPostcode(string postcode);
    }
}