using Dfc.CourseDirectory.Common;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult
{
    public class LarsSearchFilterModel : ValueObject<LarsSearchFilterModel>
    {
        public string Title { get; }
        public IEnumerable<LarsSearchFilterItemModel> Items { get; set; }

        public LarsSearchFilterModel(
            string title,
            IEnumerable<LarsSearchFilterItemModel> items)
        {
            Throw.IfNullOrWhiteSpace(title, nameof(title));
            Throw.IfNull(items, nameof(items));

            Title = title;
            Items = items;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Title;
            yield return Items;
        }
    }
}