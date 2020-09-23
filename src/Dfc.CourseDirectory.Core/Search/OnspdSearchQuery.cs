using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Dfc.CourseDirectory.Core.Search.AzureSearch;
using Dfc.CourseDirectory.Core.Search.Models;

namespace Dfc.CourseDirectory.Core.Search
{
    public class OnspdSearchQuery : IAzureSearchQuery<Onspd>
    {
        public string Postcode { get; set; }

        public (string SearchText, SearchOptions Options) GenerateSearchQuery()
        {
            return new AzureSearchQueryBuilder(Postcode)
                .WithSearchMode(SearchMode.All)
                .WithSearchFields(nameof(Onspd.pcd2))
                .WithSize(1)
                .Build();
        }
    }
}
