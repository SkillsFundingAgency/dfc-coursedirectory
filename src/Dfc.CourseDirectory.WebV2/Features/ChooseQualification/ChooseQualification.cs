using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Configuration;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation.Results;
using FormFlow;
using MediatR;
using Microsoft.Extensions.Options;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Features.ChooseQualification
{
    public class Query : IRequest<ViewModel>
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
        public IEnumerable<string> NotionalNVQLevelv2 { get; set; }
        public IEnumerable<string> AwardingOrganisation { get; set; }
        public string SearchTerm { get; set; }
        public int PageSize { get; set; }
    }

    public class Result
    {
        public string CourseName { get; set; }
        public string LARSCode { get; set; }
        public string Level { get; set; }
        public string AwardingOrganisation { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
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

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.SearchTerm))
                return new ViewModel();

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
                    NotionalNVQLevelv2Filters = request.NotionalNVQLevelv2,
                    //AwardOrgCodeFilters = request.AwardOrgCodeFilter,
                    AwardOrgAimRefFilters = request.AwardingOrganisation,
                    CertificationEndDateFilter = DateTimeOffset.UtcNow,
                    Facets = new[] { nameof(Lars.AwardOrgCode), nameof(Lars.NotionalNVQLevelv2) },
                    PageSize = _larsSearchSettings.ItemsPerPage,
                    PageNumber = request.PageNumber
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
                PageNumber = request.PageNumber ?? 1,
                Total = result.TotalCount,
                SearchResults = res.ToList(),
                PageSize = _larsSearchSettings.ItemsPerPage,
                SearchTerm = request.SearchTerm,
                TotalPages = result.TotalCount.HasValue ? (int)Math.Ceiling((decimal)result.TotalCount / _larsSearchSettings.ItemsPerPage) : 0,
                NotionalNVQLevelv2 = request.NotionalNVQLevelv2,
                AwardingOrganisation = request.AwardingOrganisation
            };
        }
    }
}
