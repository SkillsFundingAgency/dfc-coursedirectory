using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Web.ViewComponents.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Web.ViewComponents.Pagination
{
    public class PaginationModel : ValueObject<PaginationModel>, IViewComponentModel
    {
        public bool HasErrors => Errors.Count() > 0;
        public IEnumerable<string> Errors { get; }
        public IEnumerable<PaginationItemModel> Items { get; set; }

        public PaginationModel()
        {
            Errors = new string[] { };
            Items = new PaginationItemModel[] 
            {
                new PaginationItemModel() { IsEnabled=true, CssClass="previous-page", Url="http:example.com/page/879", Text="Previous"},
                new PaginationItemModel() { IsEnabled=true, CssClass="page-no", Url="http:example.com/page/880", Text="880"},
                new PaginationItemModel() { IsEnabled=true, CssClass="page-no", Url="http:example.com/page/881", Text="881"},
                new PaginationItemModel() { IsEnabled=true, CssClass="page-no", Url="http:example.com/page/882", Text="882"},
                new PaginationItemModel() { IsEnabled=true, CssClass="page-no", Url="http:example.com/page/883", Text="883"},
                new PaginationItemModel() { IsEnabled=true, CssClass="page-no", Url="http:example.com/page/884", Text="884"},
                new PaginationItemModel() { IsEnabled=true, CssClass="page-no", Url="http:example.com/page/885", Text="885"},
                new PaginationItemModel() { IsEnabled=true, CssClass="page-no", Url="http:example.com/page/886", Text="886"},
                new PaginationItemModel() { IsEnabled=true, CssClass="page-no", Url="http:example.com/page/887", Text="887"},
                new PaginationItemModel() { IsEnabled=true, CssClass="page-no", Url="http:example.com/page/888", Text="888"},
                new PaginationItemModel() { IsEnabled=true, CssClass="page-no", Url="http:example.com/page/889", Text="889"},
                new PaginationItemModel() { IsEnabled=true, CssClass="next-page", Url="http:example.com/page/890", Text="Next"}
            };
        }

        public PaginationModel(string error)
        {
            Errors = new string[] { error };
            Items = new PaginationItemModel[] { };
        }

        public PaginationModel(
            IEnumerable<PaginationItemModel> items)
        {
            Throw.IfNull(items, nameof(items));

            Errors = new string[] { };
            Items = items;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HasErrors;
            yield return Errors;
            yield return Items;
        }
    }
}
