using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Services
{
    public class AddressSelectionResult : ValueObject<AddressSelectionResult>, IAddressSelectionResult
    {

        public bool HasErrors => Errors.Count() > 0;
        public IEnumerable<string> Errors { get; }

        public string Id { get; }
        public string Line1 { get; }
        [JsonProperty(Required = Required.AllowNull)]
        public string Line2 { get; }

        public string City { get; }

        [JsonProperty("province")]
        public string County { get; }

        [JsonProperty("postalcode")]
        public string PostCode { get; }

        public AddressSelectionResult()
        {
            Errors = new string[] { };
        }

        public AddressSelectionResult(string error)
        {
            Errors = new string[] { error };
        }

        public AddressSelectionResult(
            string id,
            string line1,
            string line2,
            string city,
            string county,
            string postcode
           )
        {
            Throw.IfNullOrWhiteSpace(id, nameof(id));
            Throw.IfNullOrWhiteSpace(line1, nameof(line1));
            Throw.IfNullOrWhiteSpace(postcode, nameof(postcode));
            Errors = new string[] { };
            Id = id;
            Line1 = line1;
            Line2 = line2;
            City = city;
            County = county;
            PostCode = postcode;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HasErrors;
            yield return Errors;
            yield return Id;
            yield return Line1;
            yield return Line2;
            yield return City;
            yield return County;
            yield return PostCode;
        }

    }
}