using System.Collections.Generic;
using System.Linq;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Dfc.CourseDirectory.Core.Search.AzureSearch;
using Dfc.CourseDirectory.Core.Search.Models;

namespace Dfc.CourseDirectory.Core.Search
{
    public class ProviderSearchQuery : IAzureSearchQuery
    {
        private const int DefaultSize = 20;

        public string SearchText { get; set; }

        public IEnumerable<string> Towns { get; set; }

        public int? Size { get; set; }

        public (string SearchText, SearchOptions Options) GenerateSearchQuery()
        {
            return new AzureSearchQueryBuilder($"{SearchText?.Trim()}*")
                .WithSearchMode(SearchMode.All)
                .WithSearchInFilter(nameof(Provider.Town), Towns?.Distinct())
                .WithSize(Size ?? DefaultSize)
                .Build();
        }
    }
}