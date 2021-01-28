using System;
using System.Text.RegularExpressions;

namespace Dfc.CourseDirectory.Core.Validation
{
    public static class PostcodeHelper
    {
        public static readonly Regex UkPostcodePattern = new Regex(
            @"^((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\s?[0-9][A-Za-z]{2})$",
            RegexOptions.Compiled);

        public static string NormalizePostcode(string postcode)
        {
            if (postcode == null)
            {
                throw new ArgumentNullException(nameof(postcode));
            }

            if (!UkPostcodePattern.IsMatch(postcode))
            {
                throw new ArgumentException("Postcode is not valid.", nameof(postcode));
            }

            // See https://www.bph-postcodes.co.uk/guidetopc.cgi

            var trimmed = postcode.Replace(" ", "").Trim();
            return trimmed[0..(trimmed.Length - 3)] + " " + trimmed[^3..];
        }
    }
}
