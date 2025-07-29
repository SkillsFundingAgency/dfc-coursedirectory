using System;

namespace Dfc.CourseDirectory.WebV2
{
    public static class UrlUtil
    {
        public static string EnsureHttpPrefixed(string website)
        {
            if (string.IsNullOrEmpty(website))
            {
                throw new ArgumentException("Value cannot be empty.", nameof(website));
            }

            if (website.StartsWith("https://"))
                return website;

            if (website.StartsWith("http://"))
            {
                string websiteWithoutProtocol = website.Substring(4, website.Length - 4);
                return $"https{websiteWithoutProtocol}";
            }

            return "https://" + website;
        }
    }
}
