using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces;
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

        public async Task<IActionResult> Index(string searchTerm)
        {
            var criteria = new LarsSearchCriteria(searchTerm);
            var result = await _larsSearchService.SearchAsync(criteria);
            var items = new List<Components.LarsSearchResult.LarsSearchResultItemModel>();
            Components.LarsSearchResult.LarsSearchResultModel model;

            if (result.IsSuccess && result.HasValue)
            {
                foreach (var item in result.Value.Value)
                {
                    items.Add(new Components.LarsSearchResult.LarsSearchResultItemModel
                    {
                        SearchScore = item.SearchScore,
                        LearnAimRef = item.LearnAimRef,
                        LearnAimRefTitle = item.LearnAimRefTitle,
                        NotionalNVQLevelv2 = item.NotionalNVQLevelv2,
                        AwardOrgCode = item.AwardOrgCode,
                        LearnDirectClassSystemCode1 = item.LearnDirectClassSystemCode1,
                        LearnDirectClassSystemCode2 = item.LearnDirectClassSystemCode2,
                        SectorSubjectAreaTier1 = item.SectorSubjectAreaTier1,
                        SectorSubjectAreaTier2 = item.SectorSubjectAreaTier2,
                        GuidedLearningHours = item.GuidedLearningHours,
                        TotalQualificationTime = item.TotalQualificationTime,
                        UnitType = item.UnitType,
                        AwardOrgName = item.AwardOrgName
                    });
                }

                model = new Components.LarsSearchResult.LarsSearchResultModel(searchTerm, items);
            }
            else
            {
                model = new Components.LarsSearchResult.LarsSearchResultModel(result.Error);
            }

            return ViewComponent(nameof(LarsSearchResult), model);
        }
    }
}