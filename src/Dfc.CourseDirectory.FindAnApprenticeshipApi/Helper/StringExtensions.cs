using System.Text.RegularExpressions;

namespace Dfc.Providerportal.FindAnApprenticeship.Helper
{
    public static class StringExtensions
    {
        /// <summary>
        /// COUR-2346 - Ensure spaces after fullstops are preserved once html tags are stripped out
        /// </summary>
        /// <param name="input">the string to parse</param>
        /// <returns>A processed string with spaces after fullstops, except for the last fullstop</returns>
        public static string EnforceSpacesAfterFullstops(this string input)
        {
            return Regex.Replace(input, "\\.(?!\\s)(?!\\.)(?!$)", ". ");
        }

        /// <summary>
        /// COUR-2346 - Replace HTML tags with newline equivalent for FAT compatibility
        /// Course directory uses a rich HTML entry box, whereas FAT is expecting carriage returns and newlines.
        /// TODO: Pass full sanitized HTML content to FAT so that they can show richer content
        /// </summary>
        /// <param name="input">the html to parse</param>
        /// <returns>parsed html, with newlines instead of breaks/paragraphs, except for the last para</returns>
        public static string ConvertHtmlBreaksToLineBreaks(this string input)
        {
            return Regex.Replace(input, "(<br>)|(<br \\/>)|(</p>)(?!$)", "\r\n\r\n");
        }
    }

}
