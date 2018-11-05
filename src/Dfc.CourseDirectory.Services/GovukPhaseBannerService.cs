using Dfc.CourseDirectory.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Services
{
    public class GovukPhaseBannerService : IGovukPhaseBannerService
    {
        private readonly GovukPhaseBannerSettings _settings;

        public GovukPhaseBannerService(IOptions<GovukPhaseBannerSettings> settings)
        {
            _settings = settings.Value;
        }

        public IGovukPhaseBannerSettings GetSettings()
        {
            return _settings;
        }

        public IGovukPhaseBannerSettings GetSettings(bool? isVisible, string tag, string linkUrl, string linkText)
        {
            var actualIsVisible = isVisible ?? _settings.IsVisible;
            var actualTag = string.IsNullOrWhiteSpace(tag) ? _settings.Tag : tag;
            var actualLinkUrl = string.IsNullOrWhiteSpace(linkUrl) ? _settings.LinkUrl : linkUrl;
            var actualLinkText = string.IsNullOrWhiteSpace(linkText) ? _settings.LinkText : linkText;

            return new GovukPhaseBannerSettings()
            {
                IsVisible = actualIsVisible,
                Tag = actualTag,
                LinkUrl = actualLinkUrl,
                LinkText = actualLinkText
            };
        }
    }
}