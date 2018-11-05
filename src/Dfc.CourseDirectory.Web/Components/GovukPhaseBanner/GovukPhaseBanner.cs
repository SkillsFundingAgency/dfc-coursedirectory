using Dfc.CourseDirectory.Common;
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
            _logger.LogMethodEnter();

            var model = new GovukPhaseBannerModel();

            try
            {
                var settings = _service.GetSettings(isVisible, tag, linkUrl, linkText);

                model = new GovukPhaseBannerModel(
                    settings.IsVisible,
                    settings.Tag,
                    settings.LinkUrl,
                    settings.LinkText);
            }
            catch (Exception e)
            {
                _logger.LogException("Gov uk phase banner model creation error.", e);
            }
            finally
            {
                _logger.LogMethodExit();
            }

            return View("~/Components/GovukPhaseBanner/Default.cshtml", model);
        }
    }
}