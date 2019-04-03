using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class UnregulatedCoursesController : Controller
    {
        private readonly ILogger<UnregulatedCoursesController> _logger;
        private readonly ILarsSearchSettings _larsSearchSettings;
        private readonly ILarsSearchService _larsSearchService;
        private readonly ILarsSearchHelper _larsSearchHelper;
        private readonly IPaginationHelper _paginationHelper;

        public UnregulatedCoursesController(
            ILogger<UnregulatedCoursesController> logger,
            IOptions<LarsSearchSettings> larsSearchSettings,
            ILarsSearchService larsSearchService,
            ILarsSearchHelper larsSearchHelper,
            IPaginationHelper paginationHelper)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(larsSearchSettings, nameof(larsSearchSettings));
            Throw.IfNull(larsSearchService, nameof(larsSearchService));
            Throw.IfNull(larsSearchHelper, nameof(larsSearchHelper));
            Throw.IfNull(paginationHelper, nameof(paginationHelper));

            _logger = logger;
            _larsSearchSettings = larsSearchSettings.Value;
            _larsSearchService = larsSearchService;
            _larsSearchHelper = larsSearchHelper;
            _paginationHelper = paginationHelper;
        }

        [Authorize]
        public IActionResult Index(string NotificationTitle, string NotificationMessage)
        {
            var model = new UnRegulatedSearchViewModel()
                    {NotificationTitle = NotificationTitle, NotificationMessage = NotificationMessage};
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Index(UnRegulatedSearchViewModel theModel)
        {
            if (theModel.Search.ToLower() == "z9999999")
            {
                return RedirectToAction("Index", "UnregulatedCourses",
                    new
                    {
                        NotificationTitle = "Z code does not exist",
                        NotificationMessage = "Check the code you have entered and try again"
                    });
            }



            LarsSearchRequestModel requestModel = new LarsSearchRequestModel();
            //requestModel.SearchTerm = theModel.Search;

            requestModel.SectorSubjectAreaTier1Filter= new string[1];
            requestModel.SectorSubjectAreaTier1Filter[0] = "1.00";

            LarsSearchResultModel model;

            if (requestModel == null)
            {
                model = new LarsSearchResultModel();
            }
            else
            {
                var criteria = _larsSearchHelper.GetLarsSearchCriteria(
                    requestModel,
                    _paginationHelper.GetCurrentPageNo(Request.GetDisplayUrl(), _larsSearchSettings.PageParamName),
                    _larsSearchSettings.ItemsPerPage,
                    (LarsSearchFacet[])Enum.GetValues(typeof(LarsSearchFacet)));

                var criteria = new LarsSearchCriteria(
                    "",
                    1,
                    1,
                    null,
                    (LarsSearchFacet[])Enum.GetValues(typeof(LarsSearchFacet)));

                var result = await _larsSearchService.SearchAsync(criteria);

                if (result.IsSuccess && result.HasValue)
                {
                    var filters = _larsSearchHelper.GetLarsSearchFilterModels(result.Value.SearchFacets, requestModel);
                    var items = _larsSearchHelper.GetLarsSearchResultItemModels(result.Value.Value);

                    model = new LarsSearchResultModel(
                        requestModel.SearchTerm,
                        items,
                        Request.GetDisplayUrl(),
                        _larsSearchSettings.PageParamName,
                        _larsSearchSettings.ItemsPerPage,
                        result.Value.ODataCount ?? 0,
                        filters);
                }
                else
                {
                    model = new LarsSearchResultModel(result.Error);
                }
            }
            _logger.LogMethodExit();




            return View("ZCodeResults");
        }



        [Authorize]
        public IActionResult UnknownZCode()
        {
            SectorSubjectAreaTier s = new SectorSubjectAreaTier();
            var ssaLevel1 = s.SectorSubjectAreaTierAll.Select(y => new SSAOptions(){Id = y.Id,Description = y.Description}).ToList();

            List<SelectListItem> levelOnes = new List<SelectListItem>();
            List<SelectListItem> levelTwos = new List<SelectListItem>();
            List<SelectListItem> levels = new List<SelectListItem>();
            List<SelectListItem> categories = new List<SelectListItem>();


            UnRegulatedNotFoundViewModel model = new UnRegulatedNotFoundViewModel();

            model.ssaLevel1 = ssaLevel1;

            if (ssaLevel1 != null && ssaLevel1.Count > 0)
            {
                var defaultItem = new SelectListItem { Text = "Choose a sector area", Value = "" };

                foreach (var level1 in ssaLevel1)
                {
                    var item = new SelectListItem { Text = level1.Description, Value = level1.Id };
                    levelOnes.Add(item);
                };

                levelOnes.Insert(0, defaultItem);
                levelTwos.Insert(0, defaultItem);
            }

            model.Level1 = levelOnes;
            model.Level2 = levelTwos;

            Levels l = new Levels();
            var allLevels = l.AllLevels;

            if (allLevels != null && allLevels.Count > 0)
            {
                
                foreach (var level in allLevels)
                {
                    var item = new SelectListItem { Text = level.Level,Value = level.Id };
                    levels.Add(item);
                };
            }

            model.Levels = levels;

            Categories c = new Categories();
            var allCategogies = c.AllCategogies;

            if (allCategogies != null && allCategogies.Count > 0)
            {

                foreach (var category in allCategogies)
                {
                    var item = new SelectListItem { Text = category.Category, Value = category.Id };
                    categories.Add(item);
                };
            }

            model.Categories = categories;

            return View(model);
        }

        


 
    }
}