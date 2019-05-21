using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderApprenticeships.ProviderApprenticeshipSearchResult
{
    public class ProviderApprenticeshipsSearchResultModel
    {
       
        public string SearchTerm { get; }
        public IEnumerable<ProviderApprenticeShipsSearchResultItemModel> Items { get; set; }
       
    }
}