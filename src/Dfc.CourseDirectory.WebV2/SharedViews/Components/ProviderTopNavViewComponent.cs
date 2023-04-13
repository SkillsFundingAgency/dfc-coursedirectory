using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class ProviderTopNavViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ProviderInfo providerInfo)
        {
            var vm = ProviderNavViewModel.Create(providerInfo);

            return View("~/SharedViews/Components/ProviderTopNav.cshtml", vm);
        }
    }
}
