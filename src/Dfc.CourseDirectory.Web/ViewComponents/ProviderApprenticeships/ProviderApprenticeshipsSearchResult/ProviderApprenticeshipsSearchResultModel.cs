using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderApprenticeships.ProviderApprenticeshipSearchResult
{
    public class ProviderApprenticeshipsSearchResultModel
    {
       
        public string SearchTerm { get; }
        public List<IApprenticeship> Items { get; set; }

        public ProviderApprenticeshipsSearchResultModel()
        {
            Items = new List<IApprenticeship>();
        }
    }
}