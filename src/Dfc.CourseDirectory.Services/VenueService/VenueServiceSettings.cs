using System;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class VenueServiceSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }

        public Uri ToGetVenueByIdUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetVenueById");
        }

        public Uri ToGetVenuesByPRNAndNameUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetVenuesByPRNAndName");
        }

        public Uri ToSearchVenueUri()
        {
            var uri = new Uri(ApiUrl);
            var trimmed = uri.AbsoluteUri.TrimEnd('/');
            return new Uri($"{trimmed}/GetVenuesByPRN");
        }
    }
}
