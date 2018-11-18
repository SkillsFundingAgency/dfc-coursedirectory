using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class LarsSearchController : Controller
    {
        private readonly ILogger<LarsSearchController> _logger;
        private readonly ILarsSearchService _larsSearchService;
        private readonly ILarsSearchSettings _larsSearchSettings;
        private readonly IPaginationHelper _paginationHelper;

        public LarsSearchController(
            ILogger<LarsSearchController> logger,
            ILarsSearchService larsSearchService,
            IOptions<LarsSearchSettings> larsSearchSettings,
            IPaginationHelper paginationHelper)
        {
            _logger = logger;
            _larsSearchService = larsSearchService;
            _larsSearchSettings = larsSearchSettings.Value;
            _paginationHelper = paginationHelper;
        }

        public async Task<IActionResult> Index([FromQuery] LarsSearchRequestModel requestModel)
        {
            LarsSearchResultModel model;

            if (requestModel == null)
            {
                model = new LarsSearchResultModel();
            }
            else
            {
                var criteria = requestModel.ToLarsSearchCriteria(
                    _paginationHelper.GetCurrentPageNo(
                        Request.GetDisplayUrl(),
                        _larsSearchSettings.PageParamName),
                    _larsSearchSettings.ItemsPerPage,
                    new LarsSearchFacet[]
                    {
                        LarsSearchFacet.AwardOrgCode,
                        LarsSearchFacet.NotionalNVQLevelv2,
                        LarsSearchFacet.SectorSubjectAreaTier1,
                        LarsSearchFacet.SectorSubjectAreaTier2
                    });

                var result = await _larsSearchService.SearchAsync(criteria);
                var items = new List<LarsSearchResultItemModel>();

                if (result.IsSuccess && result.HasValue)
                {
                    var totalCount = result.Value.ODataCount ?? 0;
                    var filters = new List<LarsSearchFilterModel>();

                    if (result.Value.SearchFacets != null)
                    {
                        var notionalNVQLevelv2Facets = new List<LarsSearchFilterItemModel>();
                        var notionalNVQLevelv2FacetName = "NotionalNVQLevelv2Filter";
                        var notionalNVQLevelv2IdCount = 0;

                        foreach (var item in result.Value.SearchFacets.NotionalNVQLevelv2)
                        {
                            notionalNVQLevelv2Facets.Add(new LarsSearchFilterItemModel
                            {
                                Id = $"{notionalNVQLevelv2FacetName}-{notionalNVQLevelv2IdCount++}",
                                Name = notionalNVQLevelv2FacetName,
                                Text = $"Level {item.Value}",
                                Value = item.Value,
                                Count = item.Count,
                                IsSelected = requestModel.IsFilterSelected(
                                    nameof(requestModel.NotionalNVQLevelv2Filter),
                                    item.Value)
                            });
                        }

                        var notionalNVQLevelv2Filter = new LarsSearchFilterModel
                        {
                            Title = "Notional NVQ Level v2",
                            Items = notionalNVQLevelv2Facets
                        };

                        filters.Add(notionalNVQLevelv2Filter);

                        var awardOrgCodeFacets = new List<LarsSearchFilterItemModel>();
                        var awardOrgCodeFacetName = "AwardOrgCodeFilter";
                        var awardOrgCodeIdCount = 0;

                        foreach (var item in result.Value.SearchFacets.AwardOrgCode)
                        {
                            awardOrgCodeFacets.Add(new LarsSearchFilterItemModel
                            {
                                Id = $"{awardOrgCodeFacetName}-{awardOrgCodeIdCount++}",
                                Name = awardOrgCodeFacetName,
                                Text = item.Value,
                                Value = item.Value,
                                Count = item.Count,
                                IsSelected = requestModel.IsFilterSelected(
                                    nameof(requestModel.AwardOrgCodeFilter),
                                    item.Value)
                            });
                        }

                        var awardOrgCodeFilter = new LarsSearchFilterModel
                        {
                            Title = "Award Org Code",
                            Items = awardOrgCodeFacets
                        };

                        filters.Add(awardOrgCodeFilter);

                        var sectorSubjectAreaTier1Facets = new List<LarsSearchFilterItemModel>();
                        var sectorSubjectAreaTier1FacetName = "SectorSubjectAreaTier1Filter";
                        var sectorSubjectAreaTier1IdCount = 0;

                        foreach (var item in result.Value.SearchFacets.SectorSubjectAreaTier1)
                        {
                            sectorSubjectAreaTier1Facets.Add(new LarsSearchFilterItemModel
                            {
                                Id = $"{sectorSubjectAreaTier1FacetName}-{sectorSubjectAreaTier1IdCount++}",
                                Name = sectorSubjectAreaTier1FacetName,
                                Text = item.Value,
                                Value = item.Value,
                                Count = item.Count,
                                IsSelected = requestModel.IsFilterSelected(
                                    nameof(requestModel.SectorSubjectAreaTier1Filter),
                                    item.Value)
                            });
                        }

                        var sectorSubjectAreaTier1Filter = new LarsSearchFilterModel
                        {
                            Title = "Sector Subject Area Tier 1",
                            Items = sectorSubjectAreaTier1Facets
                        };

                        filters.Add(sectorSubjectAreaTier1Filter);

                        var sectorSubjectAreaTier2Facets = new List<LarsSearchFilterItemModel>();
                        var sectorSubjectAreaTier2FacetName = "SectorSubjectAreaTier2Filter";
                        var sectorSubjectAreaTier2IdCount = 0;

                        foreach (var item in result.Value.SearchFacets.SectorSubjectAreaTier2)
                        {
                            sectorSubjectAreaTier2Facets.Add(new LarsSearchFilterItemModel
                            {
                                Id = $"{sectorSubjectAreaTier2FacetName}-{sectorSubjectAreaTier2IdCount++}",
                                Name = sectorSubjectAreaTier2FacetName,
                                Text = item.Value,
                                Value = item.Value,
                                Count = item.Count,
                                IsSelected = requestModel.IsFilterSelected(
                                    nameof(requestModel.SectorSubjectAreaTier2Filter),
                                    item.Value)
                            });
                        }

                        var sectorSubjectAreaTier2Filter = new LarsSearchFilterModel
                        {
                            Title = "Sector Subject Area Tier 2",
                            Items = sectorSubjectAreaTier2Facets
                        };

                        filters.Add(sectorSubjectAreaTier2Filter);
                    }

                    foreach (var item in result.Value.Value)
                    {
                        items.Add(new LarsSearchResultItemModel(
                            item.SearchScore,
                            item.LearnAimRef,
                            item.LearnAimRefTitle,
                            item.NotionalNVQLevelv2,
                            item.AwardOrgCode,
                            item.LearnDirectClassSystemCode1,
                            item.LearnDirectClassSystemCode2,
                            item.SectorSubjectAreaTier1,
                            item.SectorSubjectAreaTier2,
                            item.GuidedLearningHours,
                            item.TotalQualificationTime,
                            item.UnitType,
                            item.AwardOrgName));
                    }

                    model = new LarsSearchResultModel(
                        requestModel.SearchTerm,
                        items,
                        Request.GetDisplayUrl(),
                        _larsSearchSettings.PageParamName,
                        _larsSearchSettings.ItemsPerPage,
                        totalCount,
                        filters);
                }
                else
                {
                    model = new LarsSearchResultModel(result.Error);
                }
            }

            return ViewComponent(nameof(ViewComponents.LarsSearchResult.LarsSearchResult), model);
        }
    }
}