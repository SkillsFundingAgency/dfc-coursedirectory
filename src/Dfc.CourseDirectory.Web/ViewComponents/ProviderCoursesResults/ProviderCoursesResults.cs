using System.Collections.Generic;
using Dfc.CourseDirectory.Web.ViewModels.ProviderCourses;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.ProviderCoursesResults
{
    public class ProviderCoursesResults : ViewComponent
    {
        public IViewComponentResult Invoke(ProviderCoursesViewModel model)
        {
            var actualModel = model ?? new ProviderCoursesViewModel();



            //var Items = new List<ZCodeSearchResultItemModel>()
            //{
            //    new ZCodeSearchResultItemModel()
            //    {

            //        NotionalNVQLevelv2 = "E",
            //        LearnAimRef = "Z00004395",
            //        LearnAimRefTitle =
            //            "Non regulated Adult skills formula funded provision, Pre-Entry Level, Maths, 93 to 100 hrs",
            //        AwardOrgCode = "BTEC",
            //        LearnAimRefTypeDesc = "Test Qualification Title"


            //    }
            //};

            //actualModel.Items = Items;

            return View("~/ViewComponents/ProviderCoursesResults/Default.cshtml", actualModel);
        }
    }
}