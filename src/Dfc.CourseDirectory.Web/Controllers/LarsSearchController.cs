using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Configuration;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class LarsSearchController : Controller
    {
        private readonly ISearchClient<Lars> _searchClient;
        private readonly LarsSearchSettings _larsSearchSettings;

        public LarsSearchController(
            ISearchClient<Lars> searchClient,
            IOptions<LarsSearchSettings> larsSearchSettings)
        {
            _searchClient = searchClient ?? throw new ArgumentNullException(nameof(searchClient));
            _larsSearchSettings = larsSearchSettings?.Value ?? throw new ArgumentNullException(nameof(larsSearchSettings));
        }

        [Authorize]
        public async Task<IActionResult> Index([FromQuery] LarsSearchRequestModel request)
        {
            if (request == null)
            {
                return BadRequest();
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
                    NotionalNVQLevelv2Filters = request.NotionalNVQLevelv2Filter,
                    AwardOrgCodeFilters = request.AwardOrgCodeFilter,
                    AwardOrgAimRefFilters = request.AwardOrgAimRefFilter,
                    CertificationEndDateFilter = DateTimeOffset.UtcNow,
                    Facets = new[] { nameof(Lars.AwardOrgCode), nameof(Lars.NotionalNVQLevelv2) },
                    PageSize = _larsSearchSettings.ItemsPerPage,
                    PageNumber = request.PageNo
                }));

            var unfilteredResult = results[0];
            var result = results[1];

            var viewModel = new LarsSearchResultModel
            {
                SearchTerm = request.SearchTerm,
                Items = result.Items.Select(r => r.Record).Select(LarsSearchResultItemModel.FromLars),
                Filters = new[]
                {
                    new LarsSearchFilterModel
                    {
                        Title = "Qualification level",
                        Items = unfilteredResult.Facets[nameof(Lars.NotionalNVQLevelv2)]
                            .Select((f, i) =>
                                new LarsSearchFilterItemModel
                                {
                                    Id = $"{nameof(LarsSearchRequestModel.NotionalNVQLevelv2Filter)}-{i}",
                                    Name = nameof(LarsSearchRequestModel.NotionalNVQLevelv2Filter),
                                    Text = LarsSearchFilterItemModel.FormatAwardOrgCodeSearchFilterItemText(f.Key.ToString()),
                                    Value = f.Key.ToString(),
                                    Count = (int)(f.Value ?? 0),
                                    IsSelected = request.NotionalNVQLevelv2Filter.Contains(f.Key.ToString())
                                })
                            .OrderBy(f => f.Text).ToArray()
                    },
                    new LarsSearchFilterModel
                    {
                        Title = "Awarding organisation",
                        Items = unfilteredResult.Facets[nameof(Lars.AwardOrgCode)]
                            .Select((f, i) =>
                                new LarsSearchFilterItemModel
                                {
                                    Id = $"{nameof(LarsSearchRequestModel.AwardOrgCodeFilter)}-{i}",
                                    Name = nameof(LarsSearchRequestModel.AwardOrgCodeFilter),
                                    Text = f.Key.ToString(),
                                    Value = f.Key.ToString(),
                                    Count = (int)(f.Value ?? 0),
                                    IsSelected = request.AwardOrgCodeFilter.Contains(f.Key.ToString())
                                })
                            .OrderBy(f => f.Text).ToArray()
                    }
                },
                TotalCount = (int)(result.TotalCount ?? 0),
                PageNumber = request.PageNo,
                ItemsPerPage = _larsSearchSettings.ItemsPerPage,
                PageParamName = _larsSearchSettings.PageParamName,
                Url = Request.GetDisplayUrl()
            };

            return ViewComponent(nameof(LarsSearchResult), viewModel);
        }
    }
}
