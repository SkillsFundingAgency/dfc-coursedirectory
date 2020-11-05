using System;

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
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException($"{nameof(url)} cannot be null or empty or whitespace.", nameof(url));
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException($"{nameof(text)} cannot be null or empty or whitespace.", nameof(text));
            }

            if (string.IsNullOrWhiteSpace(ariaLabel))
            {
                throw new ArgumentNullException($"{nameof(ariaLabel)} cannot be null or empty or whitespace.", nameof(ariaLabel));
            }

            Url = url;
            Text = text;
            AriaLabel = ariaLabel;
            IsCurrent = isCurrent;
        }
    }
}
