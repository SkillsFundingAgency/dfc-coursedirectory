using System;

namespace Dfc.CourseDirectory.FindACourseApi
{
    public static class UrlUtil
    {
        public static string EnsureHttpPrefixed(string website)
        {
            if (string.IsNullOrEmpty(website))
            {
                return null;
            }

            if (website.StartsWith("http://") || website.StartsWith("https://"))
            {
                return website;
            }
            else
            {
                return "http://" + website;
            }
        }
    }
}
