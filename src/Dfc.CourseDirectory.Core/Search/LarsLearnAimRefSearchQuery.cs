using System;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Dfc.CourseDirectory.Core.Search.AzureSearch;
using Dfc.CourseDirectory.Core.Search.Models;

namespace Dfc.CourseDirectory.Core.Search
{
    public class LarsLearnAimRefSearchQuery : IAzureSearchQuery<Lars>
    {
        public string LearnAimRef { get; set; }

        public DateTimeOffset? CertificationEndDateFilter { get; set; }

        public (string SearchText, SearchOptions Options) GenerateSearchQuery()
        {
            var builder = new AzureSearchQueryBuilder(LearnAimRef)
                .WithSearchMode(SearchMode.All)
                .WithSearchFields(nameof(Lars.LearnAimRef))
                .WithSize(1);

            if (CertificationEndDateFilter.HasValue)
            {
                builder.WithFilters($"({nameof(Lars.CertificationEndDate)} ge {CertificationEndDateFilter.Value:O} or {nameof(Lars.CertificationEndDate)} eq null)");
            }

            return builder.Build();
        }
    }
}