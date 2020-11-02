using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipSearchResult
{
    public class ApprenticeshipsSearchResultModel
    {
       
        public string SearchTerm { get; set; }
        public IEnumerable<ApprenticeShipsSearchResultItemModel> Items { get; set; }
       
    }
}