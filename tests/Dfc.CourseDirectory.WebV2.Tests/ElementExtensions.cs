using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp.Dom;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public static class ElementExtensions
    {
        public static T As<T>(this IElement element)
            where T : class, IElement
        {
            return element as T;
        }

        public static IReadOnlyList<IElement> GetAllElementsByTestId(this IElement element, string testId) =>
            element.QuerySelectorAll($"*[data-testid='{testId}']").ToList();

        public static IElement GetElementByTestId(this IElement element, string testId) =>
            GetAllElementsByTestId(element, testId).SingleOrDefault();

        public static string GetTrimmedTextContent(this IElement element) =>
            Regex.Replace(element.TextContent, @"\s+", " ", RegexOptions.Multiline).Trim();
    }
}
