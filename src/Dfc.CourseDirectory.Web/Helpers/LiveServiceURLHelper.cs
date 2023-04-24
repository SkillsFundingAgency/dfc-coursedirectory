using System.Security.AccessControl;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Dfc.CourseDirectory.Web.Helpers
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
