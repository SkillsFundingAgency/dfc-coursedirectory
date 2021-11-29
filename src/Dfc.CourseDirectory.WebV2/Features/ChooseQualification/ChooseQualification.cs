using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Configuration;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation;
using FormFlow;
using Mapster;
using MediatR;
using Microsoft.Extensions.Options;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Features.ChooseQualification
{
    using QueryResponse = OneOf<ModelWithErrors<ViewModel>, ViewModel>;


    public class Query : IRequest<ViewModel>
    { 
    }

    public class SearchQuery : IRequest<QueryResponse>
    {
        public string SearchTerm { get; set; }
        public int? PageNumber { get; set; }
        public IEnumerable<string> NotionalNVQLevelv2 { get; set; }
        public IEnumerable<string> AwardingOrganisation { get; set; }
    }

    public class ViewModel
    {
        public long? Total { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public IReadOnlyCollection<Result> SearchResults { get; set; }
        public IEnumerable<LarsSearchFilterModel> NotionalNVQLevelv2Filters { get; set; }
        public IEnumerable<LarsSearchFilterModel> AwardingOrganisationFilters { get; set; }
        public IEnumerable<string> NotionalNVQLevelv2 { get; set; }
        public IEnumerable<string> AwardingOrganisation { get; set; }
        public string SearchTerm { get; set; }
        public int PageSize { get; set; }
        public bool SearchWasDone { get; set; }
    }

    public class Result
    {
        public string CourseName { get; set; }
        public string LARSCode { get; set; }
        public string Level { get; set; }
        public string AwardingOrganisation { get; set; }
    }

    public class Handler :
    IRequestHandler<Query, ViewModel>,
    IRequestHandler<SearchQuery, QueryResponse>
    {
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private readonly ISearchClient<Lars> _searchClient;
        private readonly LarsSearchSettings _larsSearchSettings;

        public Handler(JourneyInstanceProvider journeyInstanceProvider, ISearchClient<Lars> search, IOptions<LarsSearchSettings> larsSearchSettings)
        {
            _journeyInstanceProvider = journeyInstanceProvider;
            _searchClient = search;
            _larsSearchSettings = larsSearchSettings?.Value ?? throw new ArgumentNullException(nameof(larsSearchSettings));
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var vm = new ViewModel()
            {
                SearchWasDone = false
            };
            return Task.FromResult(vm);
        }


        public async Task<QueryResponse> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var validator = new QueryValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var vm = request.Adapt<ViewModel>();
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            var results = await Task.WhenAll(
                // There's not currently support for multi-select faceted search, so we need to get all the results for the search term before filtering on facets.
                _searchClient.Search(new LarsSearchQuery
                {
                    SearchText = request.SearchTerm,
                    CertificationEndDateFilter = DateTimeOffset.UtcNow,
                    Facets = new[] { nameof(Lars.AwardOrgCode), nameof(Lars.NotionalNVQLevelv2) },
                    PageSize = 0
                }),
                _searchClient.Search(new LarsSearchQuery
                {
                    SearchText = request.SearchTerm,
                    CertificationEndDateFilter = DateTimeOffset.UtcNow,
                    Facets = new[] { nameof(Lars.AwardOrgCode), nameof(Lars.NotionalNVQLevelv2) },
                    PageSize = _larsSearchSettings.ItemsPerPage,
                    PageNumber = request.PageNumber,
                    NotionalNVQLevelv2Filters = request.NotionalNVQLevelv2,
                    AwardOrgCodeFilters = request.AwardingOrganisation,
                    //AwardOrgAimRefFilters = request.AwardOrgAimRefFilter,
                }));

            var unfilteredResult = results[0];
            var result = results[1];

            var res = result.Items.Select(x => new Result()
            {
                CourseName = x.Record.LearnAimRefTitle,
                LARSCode = x.Record.LearnAimRef,
                Level = x.Record.NotionalNVQLevelv2,
                AwardingOrganisation = x.Record.AwardOrgName
            });

            return new ViewModel()
            {
                SearchWasDone = true,
                PageNumber = request.PageNumber ?? 1,
                Total = result.TotalCount,
                SearchResults = res.ToList(),
                PageSize = _larsSearchSettings.ItemsPerPage,
                SearchTerm = request.SearchTerm,
                TotalPages = result.TotalCount.HasValue ? (int)Math.Ceiling((decimal)result.TotalCount / _larsSearchSettings.ItemsPerPage) : 0,
                NotionalNVQLevelv2 = request.NotionalNVQLevelv2,
                AwardingOrganisation = request.AwardingOrganisation,

                NotionalNVQLevelv2Filters = new[]
                {
                    new LarsSearchFilterModel
                    {
                        Title = "Qualification level",
                        Items = unfilteredResult.Facets[nameof(Lars.NotionalNVQLevelv2)]
                            .Select((f, i) =>
                                new LarsSearchFilterItemModel
                                {
                                    Id = $"nvqlevel2-{i}",
                                    Name = $"nvqlevel2-{i}",
                                    Text = LarsSearchFilterItemModel.FormatAwardOrgCodeSearchFilterItemText(f.Key.ToString()),
                                    Value = f.Key.ToString(),
                                    Count = (int)(f.Value ?? 0),
                                    IsSelected = request.NotionalNVQLevelv2 != null ? request.NotionalNVQLevelv2.Contains(f.Key.ToString()) : false
                                })
                            .OrderBy(f => f.Text).ToArray()
                    }
                },
                AwardingOrganisationFilters =  new [] 
                {
                    new LarsSearchFilterModel
                    {
                        Title = "Awarding organisation",
                        Items = unfilteredResult.Facets[nameof(Lars.AwardOrgCode)]
                            .Select((f, i) =>
                                new LarsSearchFilterItemModel
                                {
                                    Id = $"awardcode-{i}",
                                    Name = $"awardcode-{i}",
                                    Text = f.Key.ToString(),
                                    Value = f.Key.ToString(),
                                    Count = (int)(f.Value ?? 0),
                                    IsSelected = request.AwardingOrganisation != null ? request.AwardingOrganisation.Contains(f.Key.ToString()) : false
                                })
                            .OrderBy(f => f.Text).ToArray()
                    }
                },
            };
        }

        private class QueryValidator : AbstractValidator<SearchQuery>
        {
            public QueryValidator()
            {
                RuleFor(q => q.SearchTerm)
                    .NotEmpty()
                    .MinimumLength(3)
                    .WithMessageForAllRules("Name or keyword for the course this training is for must be 3 characters or more");
            }
        }
    }

    public class LarsSearchFilterItemModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Text { get; set; }

        public string Value { get; set; }

        public int Count { get; set; }

        public bool IsSelected { get; set; }

        public static string FormatAwardOrgCodeSearchFilterItemText(string value) => value.ToUpper() switch
        {
            "E" => "Entry level",
            "X" => "Unknown or not applicable",
            "H" => "Higher",
            "M" => "Mixed",
            _ => $"Level {value}"
        };
    }

    public class LarsSearchFilterModel
    {
        public string Title { get; set; }

        public IEnumerable<LarsSearchFilterItemModel> Items { get; set; }
    }
}
