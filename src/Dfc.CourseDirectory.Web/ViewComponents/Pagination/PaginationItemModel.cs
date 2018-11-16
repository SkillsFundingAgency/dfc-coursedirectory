using Dfc.CourseDirectory.Common;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewComponents.Pagination
{
    public class PaginationItemModel : ValueObject<PaginationItemModel>
    {
        public string Url { get; }
        public string Text { get; }
        public string AriaLabel { get; }
        public bool IsCurrent { get; }

        public PaginationItemModel(
            string url,
            string text,
            string ariaLabel,
            bool isCurrent = false)
        {
            Throw.IfNullOrWhiteSpace(url, nameof(url));
            Throw.IfNullOrWhiteSpace(text, nameof(text));
            Throw.IfNullOrWhiteSpace(ariaLabel, nameof(ariaLabel));

            Url = url;
            Text = text;
            AriaLabel = ariaLabel;
            IsCurrent = isCurrent;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Url;
            yield return Text;
            yield return AriaLabel;
            yield return IsCurrent;
        }
    }
}