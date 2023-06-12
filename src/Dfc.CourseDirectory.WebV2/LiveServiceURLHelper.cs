using System;

namespace Dfc.CourseDirectory.WebV2
{
    public static class LiveServiceURLHelper
    {
        public static string GetLiveServiceURLFromHost(string hostURL)
        {
            string returnURL = string.Empty;

            if (hostURL.Contains("dev-", StringComparison.OrdinalIgnoreCase))
            {
                returnURL = "https://dev-beta.nationalcareersservice.org.uk/";
            }
            else if (hostURL.Contains("sit-", StringComparison.OrdinalIgnoreCase))
            {
                returnURL = "https://sit-beta.nationalcareersservice.org.uk/";
            }
            else if (hostURL.Contains("sit2-", StringComparison.OrdinalIgnoreCase))
            {
                returnURL = "https://sit-beta.nationalcareersservice.org.uk/";
            }
            else if (hostURL.Contains("pp-", StringComparison.OrdinalIgnoreCase))
            {
                returnURL = "https://staging.nationalcareers.service.gov.uk/";
            }
            else
            {
                returnURL = "https://nationalcareers.service.gov.uk/";
            }

            return returnURL;
        }
    }
}
