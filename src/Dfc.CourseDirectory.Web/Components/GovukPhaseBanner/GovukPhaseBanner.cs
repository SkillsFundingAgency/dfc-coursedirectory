using Dfc.CourseDirectory.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace Dfc.CourseDirectory.Web.Components.GovukPhaseBanner
{
    public class GovukPhaseBanner : ViewComponent
    {
        private readonly ILogger<GovukPhaseBanner> _logger;
        private readonly IGovukPhaseBannerService _service;

        public GovukPhaseBanner(
            ILogger<GovukPhaseBanner> logger,
            IGovukPhaseBannerService service)
        {
            _logger = logger;
            _service = service;
        }

        public IViewComponentResult Invoke(
            bool? isVisible,
            string tag,
            string linkUrl,
            string linkText)
        {
            var settings = _service.GetSettings(isVisible, tag, linkUrl, linkText);

            var model = new GovukPhaseBannerModel(
                settings.IsVisible,
                settings.Tag,
                settings.LinkUrl,
                settings.LinkText);

            _logger.LogInformation("This is a sample log message!!!");
            _logger.LogError(new Exception("Ooow something went wrong....arrrrgh!?!"), "log this exception!");
            return View("~/Components/GovukPhaseBanner/Default.cshtml", model);
        }
    }
}
