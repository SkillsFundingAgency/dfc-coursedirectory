using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Search.Documents;
using Dfc.CourseDirectory.Core.Search.AzureSearch;
using Dfc.CourseDirectory.Core.Search.Models;
using Azure.Search.Documents.Models;

namespace Dfc.CourseDirectory.Core.Search
{
    public class CourseSearchQuery : IAzureSearchQuery<Course>
    {
        private const string RegionBoostScoringProfileName = "region-boost";

        public string CourseName { get; set; }

        public IEnumerable<string> Facets { get; set; }

        public IEnumerable<string> Filters { get; set; }

        public string OrderBy { get; set; }

        public int Size { get; set; }

        public int? Skip { get; set; }

        public (string SearchText, SearchOptions Options) GenerateSearchQuery()
        {
            var filter = string.Join(" and ", (Filters ?? Array.Empty<string>())
                .Append($"{nameof(Course.Status)} eq 1"));

            var searchOptions = new SearchOptions()
            {
                Filter = filter,
                IncludeTotalCount = true,
                ScoringProfile = RegionBoostScoringProfileName,
                SearchFields =
                {
                    nameof(Course.QualificationCourseTitle),
                    nameof(Course.CourseName)
                },
                SearchMode = SearchMode.All,
                Size = Size,
                Skip = Skip,
                QueryType = SearchQueryType.Full
            };

            if (Facets != null)
            {
                foreach (var facet in Facets)
                {
                    searchOptions.Facets.Add(facet);
                }
            }

            if (OrderBy != null)
            {
                searchOptions.OrderBy.Add(OrderBy);
            }

            return (CourseName, searchOptions);
        }
    }
}
