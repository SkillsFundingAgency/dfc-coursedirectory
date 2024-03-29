﻿using System.Collections.Generic;
using HtmlAgilityPack;
using Sanitizer = Ganss.Xss.HtmlSanitizer;

namespace Dfc.CourseDirectory.Core.Validation
{
    public static class Html
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
            if (string.IsNullOrWhiteSpace(html))
            {
                return html;
            }

            var sanitizer = new Sanitizer();
            foreach (var allowedTag in allowedTags)
            {
                sanitizer.AllowedTags.Add(allowedTag);
            }
            return sanitizer.Sanitize(html);
        }

        public static string StripTags(string html)
        {
            if (html == null)
            {
                return null;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
        }
    }
}
