using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipSearchResult
{
    public class ApprenticeshipsSearchResultModel
    {
       
        public string SearchTerm { get; }
        public IEnumerable<ApprenticeShipsSearchResultItemModel> Items { get; set; }
       
    }
}