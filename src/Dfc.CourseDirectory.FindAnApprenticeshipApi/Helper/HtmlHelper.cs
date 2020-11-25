using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace Dfc.Providerportal.FindAnApprenticeship.Helper
{
    public static class HtmlHelper
    {
        /// <summary>
        /// COUR-2346 - strip HTML tags from rich text entry content
        /// Course Directory allows users to enter rich formatted content into MarketingInfo. FAT does not like this!
        /// </summary>
        /// <param name="html">The hml to process</param>
        /// <param name="preserveLineBreaks">Preserve line breaks while stripping out HTML tags</param>
        /// <returns>Cleaned text, without html tags, with optional carriage return linebreaks</returns>
        public static string StripHtmlTags(string html, bool preserveLineBreaks = false)
        {
            if (preserveLineBreaks)
                html = html.ConvertHtmlBreaksToLineBreaks();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return doc.DocumentNode.InnerText.EnforceSpacesAfterFullstops();
        }
    }
}
