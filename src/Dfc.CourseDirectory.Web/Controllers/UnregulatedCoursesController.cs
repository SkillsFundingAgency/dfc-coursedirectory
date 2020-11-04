using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Services.UnregulatedProvision;
using Dfc.CourseDirectory.Web.Configuration;
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
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class UnregulatedCoursesController : Controller
    {
        private const string SessionAddCourseSection1 = "AddCourseSection1";
        private const string SessionAddCourseSection2 = "AddCourseSection2";

        private readonly ISearchClient<Lars> _searchClient;
        private readonly LarsSearchSettings _larsSearchSettings;
        
        private ISession Session => HttpContext.Session;

        public UnregulatedCoursesController(
            ISearchClient<Lars> searchClient,
            IOptions<LarsSearchSettings> larsSearchSettings)
        {
            _searchClient = searchClient ?? throw new ArgumentNullException(nameof(searchClient));
            _larsSearchSettings = larsSearchSettings?.Value ?? throw new ArgumentNullException(nameof(larsSearchSettings));
        }

        [Authorize]
        public IActionResult Index(string NotificationTitle, string NotificationMessage)
        {
            return View(new UnRegulatedSearchViewModel()
            {
                NotificationTitle = NotificationTitle,
                NotificationMessage = NotificationMessage
            });
        }

        [Authorize]
        public async Task<IActionResult> FindNonRegulated(UnRegulatedSearchViewModel request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            var result = await _searchClient.Search(new LarsLearnAimRefSearchQuery
            {
                LearnAimRef = request.Search,
                CertificationEndDateFilter = DateTimeOffset.UtcNow
            });

            if (!result.Results.Any())
            {
                return ViewComponent(nameof(ZCodeFoundResult), new ZCodeFoundResultModel());
            }

            var foundResult = result.Results.First();

            return ViewComponent(nameof(ZCodeFoundResult), new ZCodeFoundResultModel
            {
                AwardOrgCode = foundResult.AwardOrgCode,
                AwardOrgName = foundResult.AwardOrgName,
                LearnAimRef = foundResult.LearnAimRef,
                LearnAimRefTitle = foundResult.LearnAimRefTitle,
                LearnAimRefTypeDesc = foundResult.LearnAimRefTypeDesc,
                NotionalNVQLevelv2 = foundResult.NotionalNVQLevelv2
            });
        }

        [Authorize]
        public IActionResult GetSSALevelTwo(string Level1Id)
        {
            if (string.IsNullOrEmpty(Level1Id))
            {
                return Json(Enumerable.Empty<SelectListItem>());
            }

            var level2s = new SectorSubjectAreaTier().SectorSubjectAreaTierAll
                .Where(t => t.Id == Level1Id)
                .SelectMany(s => s.SectorSubjectAreaTier2
                    .Select(s => new SelectListItem { Text = s.Value, Value = s.Key }));

            return Json(new[] { new SelectListItem { Text = "Choose SSA level 2", Value = "" } }.Concat(level2s));
        }

        [Authorize]
        public IActionResult UnknownZCode()
        {
            RemoveSessionVariables();

            var ssaLevel1 = new SectorSubjectAreaTier().SectorSubjectAreaTierAll
                .Select(y => new SSAOptions()
                {
                    Id = y.Id,
                    Description = y.Description
                })
                .ToList();

            var model = new UnRegulatedNotFoundViewModel
            {
                ssaLevel1 = ssaLevel1
            };

            if (ssaLevel1 == null || !ssaLevel1.Any())
            {
                return View(model);
            }

            model.Level1 = new[] { new SelectListItem { Text = "Choose SSA level 1", Value = "" } }
                .Concat(ssaLevel1.Select(s => new SelectListItem { Text = s.Description, Value = s.Id }))
                .ToList();
            
            model.Level2 = new List<SelectListItem>
            {
                new SelectListItem { Text = "Choose SSA level 2", Value = "" }
            };

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> ZCodeNotKnown([FromQuery] ZCodeNotKnownRequestModel request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            RemoveSessionVariables();

            var result = await _searchClient.Search(new LarsSearchQuery
            {
                SearchText = "Z", // Wildcard is applied automatically
                SearchFields = new[] { nameof(Lars.LearnAimRef) },
                NotionalNVQLevelv2Filters = request.NotionalNVQLevelv2Filter,
                AwardOrgAimRefFilters = request.AwardOrgAimRefFilter,
                SectorSubjectAreaTier1Filters = !string.IsNullOrWhiteSpace(request.Level1Id) ? new[] { request.Level1Id } : null,
                SectorSubjectAreaTier2Filters = !string.IsNullOrWhiteSpace(request.Level2Id) ? new[] { request.Level2Id } : null,
                CertificationEndDateFilter = DateTimeOffset.UtcNow,
                Facets = new[] { nameof(Lars.NotionalNVQLevelv2), nameof(Lars.AwardOrgAimRef) },
                PageSize = _larsSearchSettings.ItemsPerPage,
                PageNumber = request.PageNo
            });

            var viewModel = new ZCodeSearchResultModel
            {
                Level1Id = request.Level1Id,
                Level2Id = request.Level2Id,
                Items = result.Results.Select(ZCodeSearchResultItemModel.FromLars),
                Filters = new[]
                {
                    new LarsSearchFilterModel
                    {
                        Title = "Level",
                        Items = result.Facets[nameof(Lars.NotionalNVQLevelv2)]
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
                        Title = "Category",
                        Items = result.Facets[nameof(Lars.AwardOrgAimRef)]
                            .Where(f => Categories.AllCategories.ContainsKey(f.Key.ToString()))
                            .Select((f, i) =>
                                new LarsSearchFilterItemModel
                                {
                                    Id = $"{nameof(LarsSearchRequestModel.AwardOrgAimRefFilter)}-{i}",
                                    Name = nameof(LarsSearchRequestModel.AwardOrgAimRefFilter),
                                    Text = Categories.AllCategories[f.Key.ToString()],
                                    Value = f.Key.ToString(),
                                    Count = (int)(f.Value ?? 0),
                                    IsSelected = request.AwardOrgAimRefFilter.Contains(f.Key.ToString())
                                })
                            .OrderBy(f => f.Text).ToArray()
                    }
                },
                TotalCount = (int)(result.TotalCount ?? 0),
                CurrentPage = request.PageNo,
                ItemsPerPage = _larsSearchSettings.ItemsPerPage,
                PageParamName = _larsSearchSettings.PageParamName,
                Url = Request.GetDisplayUrl()
            };

            return ViewComponent(nameof(ZCodeSearchResult), viewModel);
        }

        private void RemoveSessionVariables()
        {
            Session.Remove(SessionAddCourseSection1);
            Session.Remove(SessionAddCourseSection2);
        }
    }
}
