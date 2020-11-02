using Dfc.CourseDirectory.Common;

namespace Dfc.CourseDirectory.Web.ViewComponents.Pagination
{
    public class PaginationItemModel
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
    }
}
