using System;

namespace Dfc.CourseDirectory.Services.ApprenticeshipService
{
    public class ApprenticeshipServiceSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }

        public Uri UpdateAprrenticeshipUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/UpdateApprenticeship");
        }
    }
}
