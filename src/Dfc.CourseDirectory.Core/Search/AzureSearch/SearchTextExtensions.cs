using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dfc.CourseDirectory.Core.Search.AzureSearch
{
    public static class SearchTextExtensions
    {
        public static string EscapeSimpleQuerySearchOperators(this string searchText)
        {
            if (searchText == null)
            {
                return string.Empty;
            }

            return Regex.Replace(searchText, @"([\\\+\|\""\(\)\'\-\*\?])", "\\$1")
                .Replace("#", string.Empty); // Appears to intefere with wildcarding in the search analyzer, so remove.
        }

        public static string AppendWildcardWhenLastCharIsAlphanumeric(this string searchText)
        {
            var lastChar = searchText?.LastOrDefault();

            return lastChar.HasValue && char.IsLetterOrDigit(lastChar.Value)
                ? $"{searchText}*"
                : searchText ?? string.Empty;
        }

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