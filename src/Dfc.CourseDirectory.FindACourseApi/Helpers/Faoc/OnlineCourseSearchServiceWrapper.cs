using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindACourseApi.Interfaces.Faoc;
using Dfc.CourseDirectory.FindACourseApi.Models;
using Dfc.CourseDirectory.FindACourseApi.Models.Search.Faoc;
using Dfc.ProviderPortal.Packages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.FindACourseApi.Helpers.Faoc
{
    public class OnlineCourseSearchServiceWrapper
    {
        private readonly ILogger _log;
        private readonly IOnlineCourseSearchServiceSettings _settings;
        private readonly SearchServiceClient _queryService;
        private readonly ISearchIndexClient _queryIndex;
        private readonly HttpClient _httpClient;
        private readonly Uri _uri;

        public OnlineCourseSearchServiceWrapper(IOnlineCourseSearchServiceSettings settings, ILoggerFactory loggerFactory)
        {
            Throw.IfNull(loggerFactory, nameof(loggerFactory));
            Throw.IfNull(settings, nameof(settings));

            _log = loggerFactory.CreateLogger<OnlineCourseSearchServiceWrapper>();
            _settings = settings;

            _queryService = new SearchServiceClient(settings.SearchService, new SearchCredentials(settings.QueryKey));
            _queryIndex = _queryService?.Indexes?.GetClient(settings.Index);

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("api-key", settings.QueryKey);
            _httpClient.DefaultRequestHeaders.Add("api-version", settings.ApiVersion);
            _httpClient.DefaultRequestHeaders.Add("indexes", settings.Index);

            _uri = new Uri($"{settings.ApiUrl}?api-version={settings.ApiVersion}");
        }

        public async Task<FaocSearchResult> SearchOnlineCourses(OnlineCourseSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));

            _log.LogInformation("FAOC search criteria.", criteria);
            _log.LogInformation("FAOC search uri.", _uri.ToString());

            var sortBy = criteria.SortBy ?? CourseSearchSortBy.Relevance;

            var filterClauses = new List<string>();

            if (criteria.StartDateFrom.HasValue)
            {
                filterClauses.Add($"StartDate ge {criteria.StartDateFrom.Value:o}");
            }

            if (criteria.StartDateTo.HasValue)
            {
                filterClauses.Add($"StartDate le {criteria.StartDateTo.Value:o}");
            }

            if (criteria.QualificationLevels?.Any() ?? false)
            {
                filterClauses.Add($"search.in(NotionalNVQLevelv2, '{string.Join("|", criteria.QualificationLevels.Select(EscapeFilterValue))}', '|')");
            }

            if (!string.IsNullOrWhiteSpace(criteria.ProviderName))
            {
                var providerNameEscaped = EscapeFilterValue(criteria.ProviderName);
                filterClauses.Add($"search.ismatchscoring('{providerNameEscaped}', 'ProviderName', 'simple', 'any')");
            }

            var filter = string.Join(" and ", filterClauses);

            var orderBy = 
                sortBy == CourseSearchSortBy.StartDateDescending 
                ? "StartDate desc" 
                : sortBy == CourseSearchSortBy.StartDateAscending 
                    ? "StartDate asc" 
                    : "search.score() desc";

            var (limit, start) = ResolvePagingParams(criteria.Limit, criteria.Start);

            var searchText = TranslateCourseSearchSubjectText(criteria.SubjectKeyword);

            var results = await _queryIndex.Documents.SearchAsync<AzureSearchOnlineCourse>(
                searchText,
                new SearchParameters()
                {
                    Facets = new[]
                    {
                        "NotionalNVQLevelv2,count:100",
                        "ProviderName,count:100",
                    },
                    Filter = filter,
                    IncludeTotalResultCount = true,
                    SearchFields = new[]
                    {
                        "QualificationCourseTitle",
                        "CourseName",
                    },
                    SearchMode = SearchMode.All,
                    Top = limit,
                    Skip = start,
                    OrderBy = new[] { orderBy }
                });

            return new FaocSearchResult()
            {
                Limit = limit,
                Start = start,
                Total = (int)results.Count.Value,
                Facets = results.Facets,
                Items = results.Results.Select(r => new FaocSearchResultItem()
                {
                    Course = r.Document,
                    Score = r.Score
                })
            };

            string EscapeFilterValue(string v) => v.Replace("'", "''");
        }

        public static string TranslateCourseSearchSubjectText(string subjectText)
        {
            if (string.IsNullOrWhiteSpace(subjectText))
            {
                return "*";
            }

            var terms = new List<string>();

            // Find portions wrapped in quotes
            var remaining = EscapeSearchText(subjectText.Trim());
            var groupsRegex = new Regex("('|\")(.*?)(\\1)");
            Match m;
            while ((m = groupsRegex.Match(remaining)).Success)
            {
                var value = m.Groups[2].Value;

                if (m.Groups[1].Value == "'")
                {
                    terms.Add($"({CombineWords(value, " + ")})");
                }
                else   // double quotes
                {
                    terms.Add($"(\"{value}\")");
                }

                remaining = remaining.Remove(m.Index, m.Length);
            }

            // Any remaining terms should be made prefix terms
            terms.AddRange(remaining.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(t => $"{t}*"));

            return string.Join(" | ", terms);

            string CombineWords(string text, string sep) =>
                string.Join(sep, text.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            string EscapeSearchText(string text) => text
                .Replace("+", "\\+")
                .Replace("|", "\\|")
                .Replace("-", "\\-")
                .Replace("*", "\\*")
                .Replace("(", "\\(")
                .Replace(")", "\\)");
        }

        private (int limit, int start) ResolvePagingParams(int? limit, int? start)
        {
            if (limit.HasValue)
            {
                if (limit.Value <= 0)
                {
                    throw new ProblemDetailsException(
                        new ProblemDetails()
                        {
                            Detail = "limit parameter is invalid.",
                            Status = 400,
                            Title = "InvalidPagingParameters"
                        });
                }
                else if (limit.Value > _settings.MaxTop)
                {
                    throw new ProblemDetailsException(
                        new ProblemDetails()
                        {
                            Detail = $"limit parameter cannot be greater than {_settings.MaxTop}.",
                            Status = 400,
                            Title = "InvalidPagingParameters"
                        });
                }
            }

            if (start.HasValue && start.Value < 0)
            {
                throw new ProblemDetailsException(
                    new ProblemDetails()
                    {
                        Detail = "start parameter is invalid.",
                        Status = 400,
                        Title = "InvalidPagingParameters"
                    });
            }

            var top = limit ?? _settings.DefaultTop;
            var skip = start ?? 0;

            return (top, skip);
        }
    }
}
