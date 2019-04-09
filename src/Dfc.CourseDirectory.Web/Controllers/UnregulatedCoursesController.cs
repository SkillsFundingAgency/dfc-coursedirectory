using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.UnregulatedProvision;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult;
using Dfc.CourseDirectory.Web.ViewComponents.ZCodeSearchResult;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            { NotificationTitle = NotificationTitle, NotificationMessage = NotificationMessage };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Index(UnRegulatedSearchViewModel theModel)
        {
            ZCodeSearchResultModel model = new ZCodeSearchResultModel();

            //if (theModel.Search.ToLower() == "z9999999")
            //{
            //    return RedirectToAction("Index", "UnregulatedCourses",
            //        new
            //        {
            //            NotificationTitle = "Z code does not exist",
            //            NotificationMessage = "Check the code you have entered and try again"
            //        });
            //}

            LarsSearchRequestModel requestModel = new LarsSearchRequestModel
            {
                SearchTerm = theModel.Search
            };

            // requestModel.SectorSubjectAreaTier1Filter= new string[1];
            //requestModel.SectorSubjectAreaTier1Filter[0] = "1.00";

            if (requestModel == null)
            {
                // model = new ZCodeSearchResultModel();
            }
            else
            {
                var criteria = _larsSearchHelper.GetLarsSearchCriteria(
                    requestModel,
                    _paginationHelper.GetCurrentPageNo(Request.GetDisplayUrl(), _larsSearchSettings.PageParamName),
                    _larsSearchSettings.ItemsPerPage,
                    (LarsSearchFacet[])Enum.GetValues(typeof(LarsSearchFacet)));



                //var criteria = new LarsSearchCriteria(
                //    "",
                //    1,
                //    1,
                //    null,
                //    (LarsSearchFacet[])Enum.GetValues(typeof(LarsSearchFacet)));

                var result = await _larsSearchService.SearchAsync(criteria);

                if (result.IsSuccess && result.HasValue)
                {
                    //var filters = _larsSearchHelper.GetLarsSearchFilterModels(result.Value.SearchFacets, requestModel);
                    //var items = _larsSearchHelper.GetLarsSearchResultItemModels(result.Value.Value);

                    //model = new LarsSearchResultModel(
                    //    requestModel.SearchTerm,
                    //    items,
                    //    Request.GetDisplayUrl(),
                    //    _larsSearchSettings.PageParamName,
                    //    _larsSearchSettings.ItemsPerPage,
                    //    result.Value.ODataCount ?? 0,
                    //    filters);
                    if (result.Value.Value.Count() > 0)
                    {
                        var zCodeResults = new List<ZCodeSearchResultItemModel>();

                        foreach (var item in result.Value.Value)
                        {
                            zCodeResults.Add(new ZCodeSearchResultItemModel()
                            {
                                AwardOrgCode = item.AwardOrgCode,
                                AwardOrgName = item.AwardOrgName,
                                LearnAimRef = item.LearnAimRef,
                                LearnAimRefTitle = item.LearnAimRefTitle,
                                LearnAimRefTypeDesc = item.LearnAimRefTypeDesc,
                                NotionalNVQLevelv2 = item.NotionalNVQLevelv2

                            });
                        }

                        model.Items = zCodeResults;

                        return View("ZCodeResults", model);
                    }

                }
            }

            _logger.LogMethodExit();

            return RedirectToAction("Index", "UnregulatedCourses",
                    new
                    {
                        NotificationTitle = "Z code does not exist",
                        NotificationMessage = "Check the code you have entered and try again"
                    });

        }


        [Authorize(Policy = "ElevatedUserRole")]
        public async Task<List<SelectListItem>> GetSSALevelTwo(string Level1Id)
        {
            List<SelectListItem> levelTwos = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(Level1Id))
            {
                SectorSubjectAreaTier s = new SectorSubjectAreaTier();
                var ssaLevel2 = s.SectorSubjectAreaTierAll.Where(t => t.Id == Level1Id).Select(y => y.SectorSubjectAreaTier2);

                var defaultItem = new SelectListItem { Text = "Choose a sector area", Value = "" };


                foreach (var level2 in ssaLevel2)
                {
                    foreach (var level2Item in level2)
                    {
                        var item = new SelectListItem { Text = level2Item.Value, Value = level2Item.Key };
                        levelTwos.Add(item);
                    }


                }

                levelTwos.Insert(0, defaultItem);

            }

            return levelTwos;
        }


        [Authorize]
        public IActionResult UnknownZCode()
        {
            SectorSubjectAreaTier s = new SectorSubjectAreaTier();
            var ssaLevel1 = s.SectorSubjectAreaTierAll.Select(y => new SSAOptions() { Id = y.Id, Description = y.Description }).ToList();

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
                var defaultItem = new SelectListItem { Text = "Select a level", Value = "" };
                foreach (var level in allLevels)
                {
                    var item = new SelectListItem { Text = level.Level, Value = level.Id };
                    levels.Add(item);
                };

                levels.Insert(0, defaultItem);
            }

            model.Levels = levels;

            Categories c = new Categories();
            var allCategogies = c.AllCategogies;

            if (allCategogies != null && allCategogies.Count > 0)
            {
                var defaultItem = new SelectListItem { Text = "Select a category", Value = "" };
                foreach (var category in allCategogies)
                {
                    var item = new SelectListItem { Text = category.Category, Value = category.Id };
                    categories.Add(item);
                };

                categories.Insert(0, defaultItem);
            }

            model.Categories = categories;

            return View(model);
        }







        [Authorize]
        public async Task<IActionResult> ZCodeNotKnown([FromQuery] ZCodeNotKnownRequestModel request)
        {
            ZCodeSearchResultModel model = new ZCodeSearchResultModel();

            LarsSearchRequestModel requestModel = new LarsSearchRequestModel();


            requestModel.SectorSubjectAreaTier1Filter = new string[1];
            requestModel.SectorSubjectAreaTier1Filter[0] = request.Level1Id;

            requestModel.SectorSubjectAreaTier2Filter = new string[1];
            requestModel.SectorSubjectAreaTier2Filter[0] = request.Level2Id;

            if (!string.IsNullOrEmpty(request.LevelId))
            {
                requestModel.NotionalNVQLevelv2Filter = new string[1];
                requestModel.NotionalNVQLevelv2Filter[0] = request.LevelId;
            }

            if (!string.IsNullOrEmpty(request.CategoryId))
            {
                requestModel.AwardOrgAimRefFilter = new string[1];
                requestModel.AwardOrgAimRefFilter[0] = request.CategoryId;
            }

            if (requestModel == null)
            {

            }
            else
            {
                var criteria = _larsSearchHelper.GetZCodeSearchCriteria(
                    requestModel,
                    _paginationHelper.GetCurrentPageNo(Request.GetDisplayUrl(), _larsSearchSettings.PageParamName),
                    _larsSearchSettings.ItemsPerPage,
                    (LarsSearchFacet[])Enum.GetValues(typeof(LarsSearchFacet)));


                var result = await _larsSearchService.SearchAsync(criteria);

                if (result.IsSuccess && result.HasValue)
                {

                    var filters = _larsSearchHelper.GetUnRegulatedSearchFilterModels(result.Value.SearchFacets, requestModel);



                    var zCodeResults = new List<ZCodeSearchResultItemModel>();

                    foreach (var item in result.Value.Value)
                    {
                        if (item.LearnAimRef.StartsWith("Z") || item.LearnAimRef.StartsWith("z"))
                        {
                            zCodeResults.Add(new ZCodeSearchResultItemModel()
                            {
                                AwardOrgCode = item.AwardOrgCode,
                                AwardOrgName = item.AwardOrgName,
                                LearnAimRef = item.LearnAimRef,
                                LearnAimRefTitle = item.LearnAimRefTitle,
                                LearnAimRefTypeDesc = item.LearnAimRefTypeDesc,
                                NotionalNVQLevelv2 = item.NotionalNVQLevelv2

                            });
                        }
                    }

                    model.Items = zCodeResults;
                    model.Url = Request.GetDisplayUrl();
                    model.PageParamName = _larsSearchSettings.PageParamName;
                    model.ItemsPerPage = _larsSearchSettings.ItemsPerPage;
                    model.TotalCount = result.Value.ODataCount ?? 0;
                    model.Filters = filters;

                }
            }

            _logger.LogMethodExit();

            return ViewComponent(nameof(ViewComponents.ZCodeSearchResult.ZCodeSearchResult), model);

        }


    }
}