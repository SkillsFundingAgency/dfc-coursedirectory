﻿using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Dfc.CourseDirectory.Core.Search.AzureSearch;
using Dfc.CourseDirectory.Core.Search.Models;

namespace Dfc.CourseDirectory.Core.Search
{
    public class LarsSearchQuery : IAzureSearchQuery<Lars>
    {
        public string SearchText { get; set; }

        public IEnumerable<string> SearchFields { get; set; }

        public IEnumerable<string> NotionalNVQLevelv2Filters { get; set; }

        public IEnumerable<string> AwardOrgCodeFilters { get; set; }

        public IEnumerable<string> AwardOrgAimRefFilters { get; set; }

        public IEnumerable<string> SectorSubjectAreaTier1Filters { get; set; }

        public IEnumerable<string> SectorSubjectAreaTier2Filters { get; set; }

        public DateTimeOffset? CertificationEndDateFilter { get; set; }

        public IEnumerable<string> Facets { get; set; }

        public int PageSize { get; set; }

        public int? PageNumber { get; set; }

        public bool ExcludeExpired { get; set; } = false;

        public (string SearchText, SearchOptions Options) GenerateSearchQuery()
        {
            var searchTerms = SearchText.TransformSegments(s => s
                .TransformWhen(s => s.EndsWith("'s"), s => s.Substring(0, s.Length - 2))
                .TransformWhen(s => s.EndsWith("s'"), s => s.Substring(0, s.Length - 2))
                .TransformWhen(s => s.EndsWith("s"), s => s.Substring(0, s.Length - 1))
                .RemoveNonAlphanumericChars()
                .AppendWildcardWhenLastCharIsAlphanumeric());

            var searchText = searchTerms.Any()
                ? $"\"{SearchText.Trim().EscapeSimpleQuerySearchOperators()}\" | ({searchTerms})"
                : !string.IsNullOrWhiteSpace(SearchText)
                    ? $"{SearchText.Trim().EscapeSimpleQuerySearchOperators()}*"
                    : "*";

            var builder = new AzureSearchQueryBuilder(searchText)
                .WithSearchMode(SearchMode.All)
                .WithSearchFields(SearchFields?.ToArray())
                .WithSearchInFilter(nameof(Lars.NotionalNVQLevelv2), NotionalNVQLevelv2Filters)
                .WithSearchInFilter(nameof(Lars.AwardOrgCode), AwardOrgCodeFilters)
                .WithSearchInFilter(nameof(Lars.AwardOrgAimRef), AwardOrgAimRefFilters)
                .WithSearchInFilter(nameof(Lars.SectorSubjectAreaTier1), SectorSubjectAreaTier1Filters)
                .WithSearchInFilter(nameof(Lars.SectorSubjectAreaTier2), SectorSubjectAreaTier2Filters)
                .WithFacets(Facets?.ToArray())
                .WithSize(PageSize)
                .WithIncludeTotalCount();

            if (CertificationEndDateFilter.HasValue)
            {
                builder.WithFilters($"({nameof(Lars.CertificationEndDate)} ge {CertificationEndDateFilter.Value:O} or {nameof(Lars.CertificationEndDate)} eq null)");
            }

            if (PageNumber.HasValue)
            {
                builder.WithSkip(PageSize * (PageNumber - 1));
            }

            if (ExcludeExpired)
            {
                builder.WithFilters($"{nameof(Lars.IsExpired)} eq 0 ");
            }
            return builder.Build();
        }
    }
}
