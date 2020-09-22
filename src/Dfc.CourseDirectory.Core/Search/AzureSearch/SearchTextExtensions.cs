using System;
using System.Linq;

namespace Dfc.CourseDirectory.Core.Search.AzureSearch
{
    public static class SearchTextExtensions
    {
        public static string RemoveNonAlphanumericChars(this string searchText)
        {
            return new string(searchText?.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
        }

        public static string ApplyWildcardsToAllSegments(this string searchText, string segmentDelimiter = " ")
        {
            var segments = searchText?.Split(segmentDelimiter, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>();

            if (!segments.Any())
            {
                return "*";
            }

            return string.Join(segmentDelimiter, segments.Select(s => $"{s.Trim()}*"));
        }
    }
}