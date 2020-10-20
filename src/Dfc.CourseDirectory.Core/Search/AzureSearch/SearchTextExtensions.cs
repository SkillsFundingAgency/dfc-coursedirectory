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

        public static string TransformWhen(this string value, Func<string, bool> predicate, Func<string, string> transformation)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (transformation == null)
            {
                throw new ArgumentNullException(nameof(transformation));
            }

            return predicate(value)
                ? transformation(value)
                : value;
        }

        public static string TransformSegments(this string value, Func<string, string> transformation, string separator = " ")
        {
            if (transformation == null)
            {
                throw new ArgumentNullException(nameof(transformation));
            }

            if (string.IsNullOrEmpty(separator))
            {
                throw new ArgumentException($"{nameof(separator)} cannot be null or empty.", nameof(separator));
            }

            var segments = value?.Split(separator, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>();

            if (!segments.Any())
            {
                return string.Empty;
            }

            return string.Join(separator, segments.Where(s => !string.IsNullOrWhiteSpace(s)).Select(transformation));
        }
    }
}