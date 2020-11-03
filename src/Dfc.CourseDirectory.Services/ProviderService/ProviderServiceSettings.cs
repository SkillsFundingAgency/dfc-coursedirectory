using System;

namespace Dfc.CourseDirectory.Services.ProviderService
{
    public class ProviderServiceSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }

        public Uri ToGetProviderByPRNUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetProviderByPRN");
        }

        public Uri ToUpdateProviderByIdUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/UpdateProviderById");
        }

        public Uri ToUpdateProviderDetailsUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/UpdateProviderDetails");
        }
    }
}
