using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Xunit;
using Xunit.Sdk;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public static class HtmlDocumentExtensions
    {
        public static void AssertHasError(this IHtmlDocument doc, string fieldName, string expectedMessage)
        {
            var errorElementId = $"{fieldName}-error";
            var errorElement = doc.GetElementById(errorElementId);

            if (errorElement == null)
            {
                throw new XunitException($"No error found for field '{fieldName}'.");
            }

            var vht = errorElement.GetElementsByTagName("span")[0];
            var errorMessage = errorElement.InnerHtml.Substring(vht.OuterHtml.Length);
            Assert.Equal(expectedMessage, errorMessage);
        }

        public static IElement GetElementWithLabel(this IHtmlDocument doc, string label)
        {
            var allLabels = doc.QuerySelectorAll("label");

            foreach (var l in allLabels)
            {
                if (l.TextContent.Trim() == label)
                {
                    return doc.GetElementById(l.GetAttribute("for"));
                }
            }

            return null;
        }

        public static string GetSummaryListValueWithKey(this IHtmlDocument doc, string key)
        {
            var allRows = doc.QuerySelectorAll(".govuk-summary-list__row");

            foreach (var row in allRows)
            {
                var rowKey = row.QuerySelector(".govuk-summary-list__key");

                if (rowKey.TextContent.Trim() == key)
                {
                    var rowValue = row.QuerySelector(".govuk-summary-list__value");
                    return rowValue.TextContent.Trim();
                }
            }

            return null;
        }
    }
}
