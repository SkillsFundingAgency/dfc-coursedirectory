using System.Collections.Generic;
using Dfc.CourseDirectory.Web.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderApprenticeships.ProviderApprenticeshipSearchResult
{
    public class ProviderApprenticeshipsSearchResultModel
    {
        public string SearchTerm { get; }
        public List<Apprenticeship> Items { get; set; }

        public ProviderApprenticeshipsSearchResultModel()
        {
            Items = new List<Apprenticeship>();
        }
    }
}
