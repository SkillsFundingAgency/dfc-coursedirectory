using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dfc.CourseDirectory.Web.ViewComponents.PostCodeSearchResult;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class PostCodeSearchHelper : IPostCodeSearchHelper
    {
        public IPostCodeSearchCriteria GetPostCodeSearchCriteria(
            PostCodeSearchRequestModel postCodeSearchRequestModel)
        {
            Throw.IfNull(postCodeSearchRequestModel, nameof(postCodeSearchRequestModel));

            var criteria = new PostCodeSearchCriteria(
                FormatSearchTerm(postCodeSearchRequestModel.PostCode));

            return criteria;
        }

        public IEnumerable<PostCodeSearchResultItemModel> GetPostCodeSearchResultItemModels(
            IEnumerable<PostCodeSearchResultItem> postCodeSearchResultItems)
        {
            Throw.IfNull(postCodeSearchResultItems, nameof(postCodeSearchResultItems));

            var items = new List<PostCodeSearchResultItemModel>();

            foreach (var item in postCodeSearchResultItems)
            {
                items.Add(new PostCodeSearchResultItemModel(
                    item.Text.Split(',')[0], 
                    item.Id));
            }

            return items;
        }

        internal static string FormatSearchTerm(string searchTerm)
        {
            Throw.IfNullOrWhiteSpace(searchTerm, nameof(searchTerm));

            var split = searchTerm
                .Split(' ')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return split.Count() > 1 ? string.Join("*+", split) + "*" : $"{split[0]}*";
        }
    }
}
