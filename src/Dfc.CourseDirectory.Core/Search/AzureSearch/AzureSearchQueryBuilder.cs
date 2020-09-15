using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace Dfc.CourseDirectory.Core.Search.AzureSearch
{
    public class AzureSearchQueryBuilder
    {
        private string _searchText;
        private SearchMode? _searchMode;
        private IList<string> _searchFields;
        private IList<string> _facets;
        private IList<string> _filters;
        private IList<string> _orderBy;
        private string _scoringProfile;
        private int? _size;
        private int? _skip;
        private bool? _includeTotalCount;

        public AzureSearchQueryBuilder(string searchText)
        {
            _searchText = searchText;
            _searchFields = new List<string>();
            _facets = new List<string>();
            _filters = new List<string>();
            _orderBy = new List<string>();
        }

        public AzureSearchQueryBuilder WithSearchMode(SearchMode? searchMode)
        {
            _searchMode = searchMode;
            return this;
        }

        public AzureSearchQueryBuilder WithSearchFields(params string[] searchFields)
        {
            foreach (var searchField in searchFields)
            {
                _searchFields.Add(searchField);
            }
            
            return this;
        }

        public AzureSearchQueryBuilder WithFacets(params string[] facets)
        {
            foreach (var facet in facets)
            {
                _facets.Add(facet);
            }

            return this;
        }

        public AzureSearchQueryBuilder WithSearchInFilter(string variable, IEnumerable<string> values, char delimiter = '|')
        {
            if (string.IsNullOrWhiteSpace(variable))
            {
                throw new ArgumentNullException(nameof(variable));
            }

            if (values?.Any() ?? false)
            {
                if (values.Any(v => v.Contains(delimiter)))
                {
                    throw new ArgumentException($"{nameof(values)} cannot contain {nameof(delimiter)} '{delimiter}'.", nameof(values));
                }

                _filters.Add($"search.in({variable}, '{string.Join(delimiter, values)}', '{delimiter}')");
            }

            return this;
        }

        public AzureSearchQueryBuilder WithOrderBy(params string[] orderBys)
        {
            foreach (var orderBy in orderBys)
            {
                _orderBy.Add(orderBy);
            }

            return this;
        }

        public AzureSearchQueryBuilder WithScoringProfile(string scoringProfile)
        {
            _scoringProfile = scoringProfile;
            return this;
        }

        public AzureSearchQueryBuilder WithSize(int? size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), size, $"{nameof(size)} must be greated than zero.");
            }

            _size = size;
            return this;
        }

        public AzureSearchQueryBuilder WithSkip(int? skip)
        {
            if (skip <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(skip), skip, $"{nameof(skip)} must be greated than zero.");
            }

            _skip = skip;
            return this;
        }

        public AzureSearchQueryBuilder WithIncludeTotalCount(bool? includeTotalCount = true)
        {
            _includeTotalCount = includeTotalCount;
            return this;
        }

        public (string SearchText, SearchOptions Options) Build()
        {
            var options = new SearchOptions
            {
                SearchMode = _searchMode,
                Filter = string.Join(" AND ", _filters),
                ScoringProfile = _scoringProfile,
                Size = _size,
                Skip = _skip,
                IncludeTotalCount = _includeTotalCount
            };

            foreach (var searchField in _searchFields)
            {
                options.SearchFields.Add(searchField);
            }

            foreach (var facet in _facets)
            {
                options.Facets.Add(facet);
            }

            foreach (var orderBy in _orderBy)
            {
                options.OrderBy.Add(orderBy);
            }

            return (!string.IsNullOrWhiteSpace(_searchText) ? _searchText : "*", options);
        }
    }
}