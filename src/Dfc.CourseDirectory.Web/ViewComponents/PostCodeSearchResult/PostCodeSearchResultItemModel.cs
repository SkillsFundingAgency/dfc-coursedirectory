using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.ViewComponents.PostCodeSearchResult
{
    public class PostCodeSearchResultItemModel : ValueObject<PostCodeSearchResultItemModel>, IViewComponentModel
    {
        public bool HasErrors => Errors.Count() > 0;
        public IEnumerable<string> Errors { get; }
        public string AddressLine1 { get; set; }
        public string Id { get; set; }

        public PostCodeSearchResultItemModel(
            string addressLine1,
            string id)
        {
            Errors = new string[] { };
            AddressLine1 = addressLine1;
            Id = id;
        }

        public PostCodeSearchResultItemModel()
        {
            Errors = new string[] { };
        }

        public PostCodeSearchResultItemModel(string error)
        {
            Errors = new string[] { error };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HasErrors;
            yield return Errors;
            yield return AddressLine1;
            yield return Id;
        }
    }
}