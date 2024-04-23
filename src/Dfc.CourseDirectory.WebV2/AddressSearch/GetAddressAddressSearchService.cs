using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Globalization;
using Newtonsoft.Json.Converters;
using System.Collections;
using System.Data;
using Dfc.CourseDirectory.Core.Models;
using FluentValidation.Resources;
using System.Drawing.Drawing2D;
using NullValueHandling = Newtonsoft.Json.NullValueHandling;
using static Dfc.CourseDirectory.WebV2.AddressSearch.GetAddressAddressSearchService;


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



            if (result.FirstOrDefault() == null)
            {
                result = results.Results.Where(x => x.Lpi != null && x.Lpi.Uprn == Convert.ToInt64(id));
                // start number is returning 0 when null _line1 = result.FirstOrDefault()?.Lpi.PAO_START_NUMBER.ToString()  +" " + result.FirstOrDefault()?.Lpi.PAO_TEXT?.ToString();
                _line1 =  result.FirstOrDefault()?.Lpi.PAO_TEXT?.ToString();
                _line2 = result.FirstOrDefault()?.Lpi.StreetDescription?.ToString() + " " + result.FirstOrDefault()?.Lpi.SAO_TEXT?.ToString();
                _postTown = result.FirstOrDefault()?.Lpi.TownName;
            }
            else 
            {
                _line1 = result.FirstOrDefault()?.Dpa.ORGANISATION_NAME?.ToString() + " " + result.FirstOrDefault()?.Dpa.BUILDING_NAME?.ToString() + " " + result.FirstOrDefault()?.Dpa.SUB_BUILDING_NAME?.ToString();
                _line2 = (result.FirstOrDefault()?.Dpa.BuildingNumber.ToString() ?? string.Empty) + " " + (result.FirstOrDefault()?.Dpa.ThoroughfareName ?? string.Empty);
                _postTown = result.FirstOrDefault()?.Dpa.PostTown;
            }

            var addressLines = result;

            return new AddressDetail { 
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

            //return result?.Results?.Select(a =>
            //{
            //    return new PostcodeSearchResult
            //    {
            //        Id = $"{postcode}{CompositeIdDelimiter}{a.Dpa.Uprn}",
            //        //StreetAddress = string.Join(" ", a.Dpa.Address.Where(l => !string.IsNullOrWhiteSpace(l))).Trim(),
            //        StreetAddress = string.Join(" ", a.Dpa.Address.Trim()),
            //        Place = a.Dpa.PostTown
            //    };
            //}).ToArray() ?? Array.Empty<PostcodeSearchResult>();

            return result?.Results?.Select(resultItem =>
            {
                var dpa = resultItem.Dpa;
                var lpi = resultItem.Lpi;
                string streetAddress = "";
                string id = "";
                string place = "";


                if (lpi != null)
                {
                    streetAddress = string.Join(" ", lpi.Address.Trim());
                    id = $"{postcode}{CompositeIdDelimiter}{lpi.Uprn}";
                    place = $"{lpi.TownName}";
                }
                if (dpa != null)
                {
                    streetAddress = string.Join(" ", dpa.Address.Trim());
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
            .Where(resultItem => resultItem != null).DistinctBy(resultItem => resultItem.Id)
            .OrderBy(resultItem => resultItem.Id)
            .ToArray() ?? Array.Empty<PostcodeSearchResult>();

        }





        private async Task<OsGetAddress> FindAddresses(string postcode)
        {
            var url = new Url(string.Format(_options.ApiUrl, postcode));
            //.SetQueryParam("api-key", _options.ApiKey)
            //.SetQueryParam("expand", "true")
            //.SetQueryParam("dataset", "LPI");
          

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
            [JsonProperty("LPI", NullValueHandling = NullValueHandling.Ignore)]
            public Lpi Lpi { get; set; }

        }
        public partial class Lpi
        {
            [JsonProperty("UPRN")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long Uprn { get; set; }

            [JsonProperty("ADDRESS")]
            public string Address { get; set; }

            [JsonProperty("USRN")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long Usrn { get; set; }

            [JsonProperty("LPI_KEY")]
            public string LpiKey { get; set; }

            [JsonProperty("PAO_TEXT", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
            public string PAO_TEXT { get; set; }       

            [JsonProperty("SAO_TEXT", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
            public string SAO_TEXT { get; set; }

            [JsonProperty("PAO_START_NUMBER", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
       
            public long PAO_START_NUMBER { get; set; }

            [JsonProperty("STREET_DESCRIPTION")]
            public string  StreetDescription { get; set; }

            [JsonProperty("TOWN_NAME")]
            public string TownName { get; set; }

            [JsonProperty("ADMINISTRATIVE_AREA")]
            public string AdministrativeArea { get; set; }

            //[JsonProperty("POSTCODE_LOCATOR")]
            //public Postcode PostcodeLocator { get; set; }

            [JsonProperty("RPC")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long Rpc { get; set; }

            [JsonProperty("X_COORDINATE")]
            public double XCoordinate { get; set; }

            [JsonProperty("Y_COORDINATE")]
            public double YCoordinate { get; set; }

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

            [JsonProperty("STREET_STATE_CODE")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long StreetStateCode { get; set; }

            [JsonProperty("STREET_STATE_CODE_DESCRIPTION")]
            public string StreetStateCodeDescription { get; set; }

            [JsonProperty("STREET_CLASSIFICATION_CODE")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long StreetClassificationCode { get; set; }

            [JsonProperty("STREET_CLASSIFICATION_CODE_DESCRIPTION")]
            public string StreetClassificationCodeDescription { get; set; }

            [JsonProperty("LPI_LOGICAL_STATUS_CODE")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long LpiLogicalStatusCode { get; set; }

            [JsonProperty("LPI_LOGICAL_STATUS_CODE_DESCRIPTION")]
            public string LpiLogicalStatusCodeDescription { get; set; }

            //[JsonProperty("LANGUAGE")]
            //public Language Language { get; set; }

            [JsonProperty("MATCH")]
            public long Match { get; set; }

            [JsonProperty("MATCH_DESCRIPTION")]
            public string MatchDescription { get; set; }

        
            [JsonProperty("PAO_START_SUFFIX", NullValueHandling = NullValueHandling.Ignore)]
            public string PaoStartSuffix { get; set; }
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
