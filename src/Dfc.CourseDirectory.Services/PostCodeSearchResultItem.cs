using System.Collections.Generic;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Services
{
    public class PostCodeSearchResultItem : ValueObject<PostCodeSearchResultItem>, IPostCodeSearchResultItem
    {
        public string Id { get; }
        public string StreetAddress { get; }

        [JsonProperty("Error")]
        public string Error { get; }
        [JsonProperty("Description")]
        public string Description { get; }

        public PostCodeSearchResultItem(
            string id,
            string streetaddress
           )
        {
            Throw.IfNullOrWhiteSpace(id, nameof(id));
            Throw.IfNullOrWhiteSpace(streetaddress, nameof(streetaddress));

            Id = id;
            StreetAddress = streetaddress;
        }
        public PostCodeSearchResultItem(
           string id,
           string streetaddress, 
           string error,
           string description
          )
        {
            Throw.IfNullOrWhiteSpace(id, nameof(id));
            Throw.IfNullOrWhiteSpace(streetaddress, nameof(streetaddress));

            Id = id;
            StreetAddress = streetaddress;
            Error = error;
            Description = description;
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
            yield return StreetAddress;
        }
    }
    public class PostCodeSearchResultItems
    {
        public IEnumerable<PostCodeSearchResultItem> Items { get; set; }
    }
}