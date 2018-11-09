using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class LarsSearchController : Controller
    {
        private readonly ILogger<LarsSearchController> _logger;
        public ILarsSearchService _larsSearchService { get; }

        public LarsSearchController(
            ILogger<LarsSearchController> logger,
            ILarsSearchService larsSearchService)
        {
            _logger = logger;
            _larsSearchService = larsSearchService;
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
                var criteria = requestModel.ToLarsSearchCriteria(true, new LarsSearchFacet[]
                {
                    LarsSearchFacet.AwardOrgCode,
                    LarsSearchFacet.NotionalNVQLevelv2
                });

                var result = await _larsSearchService.SearchAsync(criteria);
                var items = new List<LarsSearchResultItemModel>();

                if (result.IsSuccess && result.HasValue)
                {
                    int? totalCount = result.Value.ODataCount;
                    var filters = new List<LarsSearchFilterModel>();

                    if (result.Value.SearchFacets != null)
                    {
                        var notionalNVQLevelv2Facets = new List<LarsFilterItemModel>();
                        var notionalNVQLevelv2FacetName = "NotionalNVQLevelv2Filter";
                        var notionalNVQLevelv2IdCount = 0;

                        foreach (var item in result.Value.SearchFacets.NotionalNVQLevelv2)
                        {
                            notionalNVQLevelv2Facets.Add(new LarsFilterItemModel
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

                        var notionalNVQLevelv2 = new LarsSearchFilterModel
                        {
                            Title = "Notional NVQ Level v2",
                            Items = notionalNVQLevelv2Facets
                        };

                        filters.Add(notionalNVQLevelv2);

                        var awardOrgCodeFacets = new List<LarsFilterItemModel>();
                        var awardOrgCodeFacetName = "AwardOrgCodeFilter";
                        var awardOrgCodeIdCount = 0;

                        foreach (var item in result.Value.SearchFacets.AwardOrgCode)
                        {
                            awardOrgCodeFacets.Add(new LarsFilterItemModel
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

                        var awardOrgCode = new LarsSearchFilterModel
                        {
                            Title = "Award Org Code",
                            Items = awardOrgCodeFacets
                        };

                        filters.Add(awardOrgCode);
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

                    model = new LarsSearchResultModel(requestModel.SearchTerm, items, filters, totalCount);
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