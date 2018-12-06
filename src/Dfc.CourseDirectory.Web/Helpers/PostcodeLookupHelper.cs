using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.PostcodeLookup;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class PostcodeLookupHelper : IPostcodeLookupHelper
    {
        public IPostCodeSearchCriteria GetPostCodeSearchCriteria(PostcodeLookupRequestModel postcodeLookupRequestModel)
        {
            Throw.IfNull(postcodeLookupRequestModel, nameof(postcodeLookupRequestModel));

            return new PostCodeSearchCriteria(postcodeLookupRequestModel.Postcode);
        }

        public IEnumerable<PostcodeLookupItemModel> GetPostCodeSearchResultItemModels(IEnumerable<PostCodeSearchResultItem> postCodeSearchResultItems)
        {
            Throw.IfNull(postCodeSearchResultItems, nameof(postCodeSearchResultItems));

            var items = new List<PostcodeLookupItemModel>();

            foreach (var item in postCodeSearchResultItems)
            {
                items.Add(new PostcodeLookupItemModel(
                    item.Id,
                    item.Text));
            }

            return items;
        }
    }
}
