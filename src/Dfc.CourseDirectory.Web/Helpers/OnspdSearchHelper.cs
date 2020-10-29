using System.Linq;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Models.Models.Onspd;
using Mapster;

namespace Dfc.CourseDirectory.Web.Helpers
{
    public class OnspdSearchHelper : IOnspdSearchHelper
    {
        private readonly ISearchClient<Core.Search.Models.Onspd> _searchClient;

        public OnspdSearchHelper(ISearchClient<Core.Search.Models.Onspd> searchClient)
        {
            _searchClient = searchClient;
        }

        public Onspd GetOnsPostcodeData(string postcode)
        {
            var onspd = new Onspd();
            if (!string.IsNullOrWhiteSpace(postcode))
            {
                var searchResult = _searchClient.Search(new OnspdSearchQuery() { Postcode = postcode })
                    .GetAwaiter().GetResult();

                if (searchResult.Results.Count > 0)
                {
                    return searchResult.Results.Single().Adapt<Onspd>();
                }
            }

            return onspd;
        }
    }
}
