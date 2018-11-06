using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.Components.Interfaces;

namespace Dfc.CourseDirectory.Web.Components.GovukPhaseBanner
{
    public class GovukPhaseBannerModel : ValueObject<GovukPhaseBannerModel>, IViewComponentModel
    {
        public bool HasErrors => Errors.Count() > 0;
        public IEnumerable<string> Errors { get; }
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

            Errors = new string[] { };
            IsVisible = isVisible;
            Tag = tag;
            LinkUrl = linkUrl;
            LinkText = linkText;
        }

        public GovukPhaseBannerModel()
        {
            Errors = new string[] { };
        }

        public GovukPhaseBannerModel(string error)
        {
            Errors = new string[] { error };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HasErrors;
            yield return Errors;
            yield return IsVisible;
            yield return Tag;
            yield return LinkUrl;
            yield return LinkText;
        }
    }
}