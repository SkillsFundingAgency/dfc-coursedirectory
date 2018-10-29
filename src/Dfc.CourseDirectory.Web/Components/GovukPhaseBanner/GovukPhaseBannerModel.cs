using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Components.GovukPhaseBanner
{
    public class GovukPhaseBannerModel
    {
        private const string TAG = "alpha";
        private const string LINK_URL = "#";
        private const string LINK_TEXT = "feedback";

        public bool IsVisible { get; set; }
        public string Tag { get; set; }
        public string LinkUrl { get; set; }
        public string LinkText { get; set; }

        public GovukPhaseBannerModel(bool isVisible = false) 
            : this(isVisible, TAG, LINK_URL, LINK_TEXT) { }

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
