using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.UnregulatedProvision;
using Dfc.CourseDirectory.Web.ViewComponents.ZCodeSearchResult;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class UnregulatedCoursesController : Controller
    {
        [Authorize]
        public IActionResult Index(string NotificationTitle, string NotificationMessage)
        {
            var model = new UnRegulatedSearchViewModel()
                    {NotificationTitle = NotificationTitle, NotificationMessage = NotificationMessage};
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Index(UnRegulatedSearchViewModel model)
        {
            if (model.Search.ToLower() == "z9999999")
            {
                return RedirectToAction("Index", "UnregulatedCourses",
                    new
                    {
                        NotificationTitle = "Z code does not exist",
                        NotificationMessage = "Check the code you have entered and try again"
                    });
            }

            var resultsModel = new ZCodeSearchResultModel()
            {
                Items = new List<ZCodeSearchResultItemModel>()
                {
                    new ZCodeSearchResultItemModel()
                    {
                        NotionalNVQLevelv2 = "E",
                        LearnAimRef = "Z00004395",
                        LearnAimRefTitle =
                            "Non regulated Adult skills formula funded provision, Pre-Entry Level, Maths, 93 to 100 hrs",
                        AwardOrgCode = "BTEC",
                        LearnAimRefTypeDesc = "Test Qualification Title"
                    }
                }

            };

            return View("ZCodeResults", resultsModel);
        }


        [Authorize(Policy = "ElevatedUserRole")]
        public async Task<List<SelectListItem>> GetSSALevelTwo(string Level1Id)
        {
            List<SelectListItem> levelTwos = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(Level1Id))
            {
                SectorSubjectAreaTier s = new SectorSubjectAreaTier();
                var ssaLevel2 = s.SectorSubjectAreaTierAll.Where(t=>t.Id==Level1Id).Select(y => y.SectorSubjectAreaTier2);

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