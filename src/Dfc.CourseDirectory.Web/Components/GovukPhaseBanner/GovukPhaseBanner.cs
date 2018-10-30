using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Components.GovukPhaseBanner
{
    public class GovukPhaseBanner : ViewComponent
    {
        private readonly ILogger<GovukPhaseBanner> _logger;

        public GovukPhaseBanner(ILogger<GovukPhaseBanner> logger)
        {
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(
            bool isVisible,
            string tag,
            string linkUrl,
            string linkText)
        {
            _logger.LogInformation("This is a sample log message!!!");
            _logger.LogError(new Exception("Ooow something went wrong....arrrrgh!?!"), "log this exception!");
            return View("~/Components/GovukPhaseBanner/Default.cshtml", new GovukPhaseBannerModel(isVisible));
        }
    }
}
