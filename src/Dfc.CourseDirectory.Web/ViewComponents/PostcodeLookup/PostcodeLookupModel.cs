using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.PostcodeLookup
{
    public class PostcodeLookupModel : /*ValueObject<PostcodeLookupModel>,*/ IViewComponentModel
    {
        private const string _locationError = "Enter a full and valid postcode";

        public bool HasErrors => Errors.Count() > 0;
        public IEnumerable<string> Errors { get; }

        [Display(Name = "Postcode")]
        [RegularExpression(@"([a-zA-Z][0-9]|[a-zA-Z][0-9][0-9]|[a-zA-Z][a-zA-Z][0-9]|[a-zA-Z][a-zA-Z][0-9][0-9]|[a-zA-Z][0-9][a-zA-Z]|[a-zA-Z][a-zA-Z][0-9][a-zA-Z]) ([0-9][abdefghjklmnpqrstuwxyzABDEFGHJLMNPQRSTUWXYZ][abdefghjklmnpqrstuwxyzABDEFGHJLMNPQRSTUWXYZ])", ErrorMessage = _locationError)]
        public string Postcode { get; set; }
        public IEnumerable<PostcodeLookupItemModel> Items { get; set; }

        public PostcodeLookupModel()
        {
            Errors = new string[] { };
            Items = new PostcodeLookupItemModel[] { };
        }

        public PostcodeLookupModel(
            string postcode,
            string error)
        {
            Throw.IfNullOrWhiteSpace(postcode, nameof(postcode)); // should this be some check via regex for a valid postcode ???
            Throw.IfNullOrWhiteSpace(error, nameof(error));

            Errors = new string[] { error };
            Postcode = postcode;
            Items = new PostcodeLookupItemModel[] { };
        }

        public PostcodeLookupModel(
            string postcode,
            IEnumerable<PostcodeLookupItemModel> items)
        {
            Throw.IfNullOrWhiteSpace(postcode, nameof(postcode)); // should this be some check via regex for a valid postcode ???
            Throw.IfNull(items, nameof(items));

            Errors = new string[] { };
            Postcode = postcode;
            Items = items;
        }

        //protected override IEnumerable<object> GetEqualityComponents()
        //{
        //    yield return HasErrors;
        //    yield return Errors;
        //    yield return Postcode;
        //    yield return Items;
        //}
    }
}
