using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;

namespace Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult
{
    public class VenueSearchResultItemModel : IViewComponentModel
    {
        public bool HasErrors => Errors.Count() > 0;
        public IEnumerable<string> Errors { get; }
        public string VenueName { get; }
        public string AddressLine1 { get; }
        public string AddressLine2 { get; }
        public string Town { get; }
        public string County { get; }
        public string PostCode { get; }
        public string Id { get; }

        public VenueSearchResultItemModel(
            string venueName,
            string addressLine1,
            string addressLine2,
            string town,
            string county,
            string postCode,
            string id)
        {
            Errors = new string[] { };
            VenueName = venueName;
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
            Town = town;
            County = county;
            PostCode = postCode;
            Id = id;
        }

        public VenueSearchResultItemModel()
        {
            Errors = new string[] { };
        }

        public VenueSearchResultItemModel(string error)
        {
            Errors = new string[] { error };
        }
    }
}
