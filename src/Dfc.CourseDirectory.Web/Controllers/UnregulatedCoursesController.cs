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
using Dfc.CourseDirectory.Web.ViewComponents.ZCodeFoundResult;
using Dfc.CourseDirectory.Web.ViewComponents.ZCodeSearchResult;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dfc.CourseDirectory.WebV2.SharedViews.Components;
using Dfc.CourseDirectory.WebV2.Features.Apprenticeships.ClassroomLocation;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class UnregulatedCoursesController : Controller
    {
        private readonly ILogger<UnregulatedCoursesController> _logger;
        private readonly ILarsSearchSettings _larsSearchSettings;
        private readonly ILarsSearchService _larsSearchService;
        private readonly ILarsSearchHelper _larsSearchHelper;
        private readonly IPaginationHelper _paginationHelper;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession Session => _contextAccessor.HttpContext.Session;

        private const string SessionAddCourseSection1 = "AddCourseSection1";
        private const string SessionAddCourseSection2 = "AddCourseSection2";

        public UnregulatedCoursesController(
            ILogger<UnregulatedCoursesController> logger,
            IOptions<LarsSearchSettings> larsSearchSettings,
            ILarsSearchService larsSearchService,
            ILarsSearchHelper larsSearchHelper,
            IPaginationHelper paginationHelper,
            IHttpContextAccessor contextAccessor)
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
            _contextAccessor = contextAccessor;
        }

        [Authorize]
        public IActionResult Index(string NotificationTitle, string NotificationMessage)
        {
            var model = new UnRegulatedSearchViewModel()
            { NotificationTitle = NotificationTitle, NotificationMessage = NotificationMessage };
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> FindNonRegulated(UnRegulatedSearchViewModel theModel)
        {
            // ZCodeSearchResultModel model = new ZCodeSearchResultModel();
            ZCodeFoundResultModel model = new ZCodeFoundResultModel();
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

            var criteria = _larsSearchHelper.GetLarsSearchCriteria(
                requestModel,
                _paginationHelper.GetCurrentPageNo(Request.GetDisplayUrl(), _larsSearchSettings.PageParamName),
                _larsSearchSettings.ItemsPerPage,
                (LarsSearchFacet[])Enum.GetValues(typeof(LarsSearchFacet)));



            var result = await _larsSearchService.SearchAsync(criteria);

            if (result.IsSuccess && result.HasValue)
            {

                if (result.Value.Value.Any())
                {
                    model = result.Value.Value.Select(x => new ZCodeFoundResultModel()
                    {
                        AwardOrgCode = x.AwardOrgCode,
                        AwardOrgName = x.AwardOrgName,
                        LearnAimRef = x.LearnAimRef,
                        LearnAimRefTitle = x.LearnAimRefTitle,
                        LearnAimRefTypeDesc = x.LearnAimRefTypeDesc,
                        NotionalNVQLevelv2 = x.NotionalNVQLevelv2
                    }).FirstOrDefault();



                }



            }
            return ViewComponent(nameof(ViewComponents.ZCodeFoundResult.ZCodeFoundResult), model);




        }

        public async Task<List<SelectListItem>> GetSSALevelTwo(string Level1Id)
        {
            List<SelectListItem> levelTwos = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(Level1Id))
            {
                SectorSubjectAreaTier s = new SectorSubjectAreaTier();
                var ssaLevel2 = s.SectorSubjectAreaTierAll.Where(t => t.Id == Level1Id).Select(y => y.SectorSubjectAreaTier2);

                var defaultItem = new SelectListItem { Text = "Choose SSA level 2", Value = "" };


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
            RemoveSessionVariables();
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
                var defaultItem = new SelectListItem { Text = "Choose SSA level 1", Value = "" };

                foreach (var level1 in ssaLevel1)
                {
                    var item = new SelectListItem { Text = level1.Description, Value = level1.Id };
                    levelOnes.Add(item);
                };

                levelOnes.Insert(0, new SelectListItem { Text = "Choose SSA level 1", Value = "" });
                levelTwos.Insert(0, new SelectListItem { Text = "Choose SSA level 2", Value = "" });
            }

            model.Level1 = levelOnes;
            model.Level2 = levelTwos;


            return View(model);
        }







        [Authorize]
        public async Task<IActionResult> ZCodeNotKnown([FromQuery] ZCodeNotKnownRequestModel request)
        {
            RemoveSessionVariables();
            ZCodeSearchResultModel model = new ZCodeSearchResultModel();

            LarsSearchRequestModel requestModel = new LarsSearchRequestModel();

            requestModel.SectorSubjectAreaTier1Filter = new string[1];
            requestModel.SectorSubjectAreaTier1Filter[0] = request.Level1Id;

            requestModel.SectorSubjectAreaTier2Filter = new string[1];
            requestModel.SectorSubjectAreaTier2Filter[0] = request.Level2Id;

            requestModel.NotionalNVQLevelv2Filter = request.NotionalNVQLevelv2Filter;




            requestModel.AwardOrgAimRefFilter = request.AwardOrgAimRefFilter;



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
                    if (item.AwardOrgAimRef == "APP H CAT C" || item.AwardOrgAimRef == "APP H CAT D" ||
                        item.AwardOrgAimRef == "APP H CAT H" || item.AwardOrgAimRef == "APP H CAT N")
                    {
                    }
                    else
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




                model.Items = zCodeResults.OrderByDescending(x => x.LearnAimRef);
                model.Url = Request.GetDisplayUrl();
                model.PageParamName = (_larsSearchSettings.PageParamName);
                model.ItemsPerPage = _larsSearchSettings.ItemsPerPage;
                model.TotalCount = result.Value.ODataCount ?? 0;
                model.Filters = filters.ToList();
                model.Level1Id = request.Level1Id;
                model.Level2Id = request.Level2Id;
                int resultpage = 0;
                var success = int.TryParse(HttpContext.Request.Query[_larsSearchSettings.PageParamName], out resultpage);

                if (success)
                {
                    model.CurrentPage = resultpage;
                }
                else
                {
                    model.CurrentPage = 1;
                }


                //model.filter0Id = request.LevelId;
                //model.filter1Id = request.CategoryId;

            }

            _logger.LogMethodExit();

            return ViewComponent(nameof(ViewComponents.ZCodeSearchResult.ZCodeSearchResult), model);


        }

        internal void RemoveSessionVariables()
        {
            Session.Remove(SessionAddCourseSection1);
            Session.Remove(SessionAddCourseSection2);
        }
    }
}