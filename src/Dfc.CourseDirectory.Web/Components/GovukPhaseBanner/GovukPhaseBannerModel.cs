using Dfc.CourseDirectory.Common;

namespace Dfc.CourseDirectory.Web.Components.GovukPhaseBanner
{
    public class GovukPhaseBannerModel
    {
        public bool IsVisible { get; }
        public string Tag { get; }
        public string LinkUrl { get; }
        public string LinkText { get; }

        public GovukPhaseBannerModel(
            bool isVisible,
            string tag,
            string linkUrl,
            string linkText)
        {
            Throw.IfNullOrWhiteSpace(tag, nameof(tag));
            Throw.IfNullOrWhiteSpace(linkUrl, nameof(linkUrl));
            Throw.IfNullOrWhiteSpace(linkText, nameof(linkText));

            IsVisible = isVisible;
            Tag = tag;
            LinkUrl = linkUrl;
            LinkText = linkText;
        }

        public GovukPhaseBannerModel()
        {
        }
    }
}