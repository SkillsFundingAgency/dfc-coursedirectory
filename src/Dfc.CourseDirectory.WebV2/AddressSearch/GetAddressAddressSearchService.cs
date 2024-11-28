using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NullValueHandling = Newtonsoft.Json.NullValueHandling;


namespace Dfc.CourseDirectory.WebV2.AddressSearch
{
    public class GetAddressAddressSearchService : IAddressSearchService
    {
        private const string CompositeIdDelimiter = "::";

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

            var idSegments = compositeId.Split(CompositeIdDelimiter);
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

            var result = results.Results.Where(x => x.Dpa != null && x.Dpa.Uprn == Convert.ToInt64(id));
            var _line1 = "";
            var _line2 = "";
            var _postTown = "";
            var _buildingNumber = "";

            if (result.FirstOrDefault()?.Dpa.BuildingNumber != 0)
            {
                long? buildingNumber = result.FirstOrDefault()?.Dpa.BuildingNumber;
                _buildingNumber = buildingNumber.ToString();
            }

            if (result.FirstOrDefault() != null)
            {

                if (result.FirstOrDefault()?.Dpa.ORGANISATION_NAME != null && (result.FirstOrDefault()?.Dpa.BuildingNumber != 0 || result.FirstOrDefault()?.Dpa.BUILDING_NAME != null))
                    _line1 = _buildingNumber + " " + result.FirstOrDefault()?.Dpa.BUILDING_NAME?.ToString() + " " + result.FirstOrDefault()?.Dpa.SUB_BUILDING_NAME?.ToString();
                else
                    _line1 = result.FirstOrDefault()?.Dpa.ORGANISATION_NAME?.ToString() + " " + _buildingNumber + " " + result.FirstOrDefault()?.Dpa.BUILDING_NAME?.ToString() + " " + result.FirstOrDefault()?.Dpa.SUB_BUILDING_NAME?.ToString();

                _line2 = (result.FirstOrDefault()?.Dpa.ThoroughfareName ?? string.Empty);
                _postTown = result.FirstOrDefault()?.Dpa.PostTown;
            }

            var addressLines = result;

            return new AddressDetail
            {
                Line1 = _line1,
                Line2 = _line2,
                PostTown = _postTown,
                Postcode = postcode,
            };
        }

        public async Task<IReadOnlyCollection<PostcodeSearchResult>> SearchByPostcode(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode))
            {
                throw new ArgumentException($"{nameof(postcode)} cannot be null or whitespace.");
            }

            var result = await FindAddresses(postcode);
            return result?.Results?.Select(resultItem =>
            {
                var dpa = resultItem.Dpa;
                string streetAddress = "";
                string id = "";
                string place = "";

                if (dpa != null)
                {
                    if (dpa.ORGANISATION_NAME != null && (dpa.BuildingNumber != 0 || dpa.BUILDING_NAME != null))
                        streetAddress = string.Join(" ", dpa.Address.Replace(dpa.ORGANISATION_NAME + ", ", "").Replace(", " + dpa.Postcode, "").Trim());
                    else
                        streetAddress = string.Join(" ", dpa.Address.Replace(", " + dpa.Postcode, "").Trim());

                    id = $"{postcode}{CompositeIdDelimiter}{dpa.Uprn}";
                    place = $"{dpa.PostTown}";
                }
                return new PostcodeSearchResult
                {
                    Id = id,
                    StreetAddress = streetAddress,
                    Place = place
                };

            })
            .Where(resultItem => resultItem != null)
            .OrderBy(resultItem => resultItem.Id).DistinctBy(resultItem => resultItem.StreetAddress)
            .ToArray() ?? Array.Empty<PostcodeSearchResult>();

        }

        private async Task<OsGetAddress> FindAddresses(string postcode)
        {
            var url = new Url(string.Format(_options.ApiUrl, postcode))
            .SetQueryParam("key", _options.ApiKey);


            using (var response = await _httpClient.GetAsync(url))
            {
                if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }

                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<OsGetAddress>(response.Content.ReadAsStringAsync().Result);


            }
        }
        public partial class OsGetAddress
        {
            [JsonProperty("header")]
            public Header Header { get; set; }

            [JsonProperty("results")]
            public Result[] Results { get; set; }
        }

        public partial class Header
        {
            [JsonProperty("uri")]
            public Uri Uri { get; set; }

            [JsonProperty("query")]
            public string Query { get; set; }

            [JsonProperty("offset")]
            public long Offset { get; set; }

            [JsonProperty("totalresults")]
            public long Totalresults { get; set; }

            [JsonProperty("format")]
            public string Format { get; set; }

            [JsonProperty("dataset")]
            public string Dataset { get; set; }

            [JsonProperty("lr")]
            public string Lr { get; set; }

            [JsonProperty("maxresults")]
            public long Maxresults { get; set; }

            [JsonProperty("epoch")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long Epoch { get; set; }

            [JsonProperty("lastupdate")]
            public DateTimeOffset Lastupdate { get; set; }

            [JsonProperty("output_srs")]
            public string OutputSrs { get; set; }
        }

        public partial class Result
        {

            [JsonProperty("DPA", NullValueHandling = NullValueHandling.Ignore)]
            public Dpa Dpa { get; set; }
        }

        public partial class Dpa
        {
            [JsonProperty("UPRN")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long Uprn { get; set; }

            [JsonProperty("UDPRN")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long Udprn { get; set; }

            [JsonProperty("ADDRESS")]
            public string Address { get; set; }

            [JsonProperty("BUILDING_NUMBER", NullValueHandling = NullValueHandling.Ignore)]
            [JsonConverter(typeof(ParseStringConverter))]
            public long BuildingNumber { get; set; }

            [JsonProperty("BUILDING_NAME", NullValueHandling = NullValueHandling.Ignore)]
            public string BUILDING_NAME { get; set; }

            [JsonProperty("THOROUGHFARE_NAME", NullValueHandling = NullValueHandling.Ignore)]
            public string ThoroughfareName { get; set; }

            [JsonProperty("POST_TOWN")]
            public string PostTown { get; set; }

            [JsonProperty("POSTCODE")]
            public string Postcode { get; set; }

            [JsonProperty("ORGANISATION_NAME", NullValueHandling = NullValueHandling.Ignore)]
            public string ORGANISATION_NAME { get; set; }

            [JsonProperty("SUB_BUILDING_NAME", NullValueHandling = NullValueHandling.Ignore)]
            public string SUB_BUILDING_NAME { get; set; }

            [JsonProperty("RPC")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long Rpc { get; set; }

            [JsonProperty("X_COORDINATE")]
            public long XCoordinate { get; set; }

            [JsonProperty("Y_COORDINATE")]
            public long YCoordinate { get; set; }

            [JsonProperty("STATUS")]
            public string Status { get; set; }

            [JsonProperty("LOGICAL_STATUS_CODE")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long LogicalStatusCode { get; set; }

            [JsonProperty("CLASSIFICATION_CODE")]
            public string ClassificationCode { get; set; }

            [JsonProperty("CLASSIFICATION_CODE_DESCRIPTION")]
            public string ClassificationCodeDescription { get; set; }

            [JsonProperty("LOCAL_CUSTODIAN_CODE")]
            public long LocalCustodianCode { get; set; }

            [JsonProperty("LOCAL_CUSTODIAN_CODE_DESCRIPTION")]
            public string LocalCustodianCodeDescription { get; set; }

            [JsonProperty("COUNTRY_CODE")]
            public string CountryCode { get; set; }

            [JsonProperty("COUNTRY_CODE_DESCRIPTION")]
            public string CountryCodeDescription { get; set; }

            [JsonProperty("POSTAL_ADDRESS_CODE")]
            public string PostalAddressCode { get; set; }

            [JsonProperty("POSTAL_ADDRESS_CODE_DESCRIPTION")]
            public string PostalAddressCodeDescription { get; set; }

            [JsonProperty("BLPU_STATE_CODE")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long BlpuStateCode { get; set; }

            [JsonProperty("BLPU_STATE_CODE_DESCRIPTION")]
            public string BlpuStateCodeDescription { get; set; }

            [JsonProperty("TOPOGRAPHY_LAYER_TOID")]
            public string TopographyLayerToid { get; set; }

            [JsonProperty("WARD_CODE")]
            public string WardCode { get; set; }

            [JsonProperty("LAST_UPDATE_DATE")]
            public string LastUpdateDate { get; set; }

            [JsonProperty("ENTRY_DATE")]
            public string EntryDate { get; set; }

            [JsonProperty("BLPU_STATE_DATE")]
            public string BlpuStateDate { get; set; }

            [JsonProperty("LANGUAGE")]
            public string Language { get; set; }

            [JsonProperty("MATCH")]
            public long Match { get; set; }

            [JsonProperty("MATCH_DESCRIPTION")]
            public string MatchDescription { get; set; }

            [JsonProperty("DELIVERY_POINT_SUFFIX")]
            public string DeliveryPointSuffix { get; set; }
        }

        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            };
        }

        internal class ParseStringConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                long l;
                if (Int64.TryParse(value, out l))
                {
                    return l;
                }
                throw new Exception("Cannot unmarshal type long");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (long)untypedValue;
                serializer.Serialize(writer, value.ToString());
                return;
            }

            public static readonly ParseStringConverter Singleton = new ParseStringConverter();
        }
        private class FindAddressResult
        {
            public string PostCode { get; set; }
            public IEnumerable<FindAddressResultItem> Addresses { get; set; }
        }

        private class FindAddressResultItem
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
