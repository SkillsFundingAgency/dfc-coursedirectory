using System.Collections.Generic;
using Sanitizer = Ganss.XSS.HtmlSanitizer;

namespace Dfc.CourseDirectory.WebV2.Validation
{
    public static class HtmlSanitizer
    {
        public static readonly IReadOnlyCollection<string> DefaultAllowedTags = new[]
        {
            "b",
            "strong",
            "p",
            "div",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "ul",
            "ol",
            "li"
        };

        public static string SanitizeHtml(string html) => SanitizeHtml(html, DefaultAllowedTags);

        public static string SanitizeHtml(string html, IEnumerable<string> allowedTags)
        {
            var sanitizer = new Sanitizer(allowedTags: allowedTags);
            return sanitizer.Sanitize(html);
        }
    }
}
