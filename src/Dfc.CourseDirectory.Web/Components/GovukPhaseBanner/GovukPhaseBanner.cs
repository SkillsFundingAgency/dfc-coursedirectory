using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Components.GovukPhaseBanner
{
    public class GovukPhaseBanner : ViewComponent
    {
        public GovukPhaseBanner() { }

        public async Task<IViewComponentResult> InvokeAsync(
            bool isVisible,
            string tag,
            string linkUrl,
            string linkText)
        {
            return View("~/Components/GovukPhaseBanner/Default.cshtml", new GovukPhaseBannerModel(isVisible));
        }
    }
}
