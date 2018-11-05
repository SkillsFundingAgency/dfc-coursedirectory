using System;

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
            if (string.IsNullOrWhiteSpace(tag)) throw new ArgumentException("Cannot be null, empty or whitespace only.", nameof(tag));
            if (string.IsNullOrWhiteSpace(linkUrl)) throw new ArgumentException("Cannot be null, empty or whitespace only.", nameof(linkUrl));
            if (string.IsNullOrWhiteSpace(linkText)) throw new ArgumentException("Cannot be null, empty or whitespace only.", nameof(linkText));

            IsVisible = isVisible;
            Tag = tag;
            LinkUrl = linkUrl;
            LinkText = linkText;
        }
    }
}