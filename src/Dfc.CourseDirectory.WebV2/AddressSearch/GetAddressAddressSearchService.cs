using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.WebV2.AddressSearch
{
    public class GetAddressAddressSearchService : IAddressSearchService
    {
        private const string IdDelimiter = "::";

        private readonly HttpClient _httpClient;
        private readonly GetAddressAddressSearchServiceOptions _options;

        public GetAddressAddressSearchService(HttpClient httpClient, IOptions<GetAddressAddressSearchServiceOptions> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<AddressDetail> GetById(string compositeId)
        {
            if (string.IsNullOrWhiteSpace(compositeId))
            {
                throw new ArgumentException($"{nameof(compositeId)} cannot be null or whitespace.");
            }

            var idSegments = compositeId.Split(IdDelimiter);
            var postcode = idSegments.First();
            var id = idSegments.Skip(1).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(postcode))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            var results = await FindAddresses(postcode);

            if (results == null)
            {
                return null;
            }

            var result = results.Addresses.FirstOrDefault(i => i.Id == id);

            if (result == null)
            {
                return null;
            }

            var addressLines = result.AddressLines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();

            return new AddressDetail
            {
                Line1 = addressLines.FirstOrDefault() ?? string.Empty,
                Line2 = addressLines.Skip(1).FirstOrDefault() ?? string.Empty,
                Line3 = addressLines.Skip(2).FirstOrDefault() ?? string.Empty,
                Line4 = addressLines.Skip(3).FirstOrDefault() ?? string.Empty,
                Line5 = addressLines.Skip(4).FirstOrDefault() ?? string.Empty,
                PostTown = result.TownOrCity,
                County = result.County,
                Postcode = results.PostCode,
                CountryName = result.Country
            };
        }

        public async Task<IReadOnlyCollection<PostcodeSearchResult>> SearchByPostcode(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode))
            {
                throw new ArgumentException($"{nameof(postcode)} cannot be null or whitespace.");
            }

            var result = await FindAddresses(postcode);

            return result?.Addresses?.Select(a =>
            {
                return new PostcodeSearchResult
                {
                    Id = $"{postcode}{IdDelimiter}{a.Id}",
                    StreetAddress = string.Join(" ", a.AddressLines.Where(l => !string.IsNullOrWhiteSpace(l))).Trim(),
                    Place = a.TownOrCity
                };
            }).ToArray() ?? Array.Empty<PostcodeSearchResult>();
        }

        private async Task<FindAddressResult> FindAddresses(string postcode)
        {
            var url = new Url($"https://api.getaddress.io/find/{postcode}")
                .SetQueryParam("api-key", _options.Key)
                .SetQueryParam("expand", "true")
                .SetQueryParam("sort", "true");

            using (var response = await _httpClient.GetAsync(url))
            {
                if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }

                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<FindAddressResult>(await response.Content.ReadAsStringAsync());
            }
        }

        private class FindAddressResult
        {
            public string PostCode { get; set; }

            public IEnumerable<FindAddressResultItem> Addresses { get; set; }
        }

        public class FindAddressResultItem
        {
            public string Id => string.Join(" ", AddressLines).Trim();

            public IEnumerable<string> AddressLines => new[] { Line1, Line2, Line3, Line4, Locality };

            [JsonProperty("line_1")]
            public string Line1 { get; set; }

            [JsonProperty("line_2")]
            public string Line2 { get; set; }

            [JsonProperty("line_3")]
            public string Line3 { get; set; }

            [JsonProperty("line_4")]
            public string Line4 { get; set; }

            [JsonProperty("locality")]
            public string Locality { get; set; }

            [JsonProperty("town_or_city")]
            public string TownOrCity { get; set; }

            [JsonProperty("county")]
            public string County { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }
        }
    }
}
