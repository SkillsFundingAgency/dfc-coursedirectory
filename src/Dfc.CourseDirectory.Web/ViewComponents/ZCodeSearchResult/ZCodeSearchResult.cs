using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.ViewComponents.ZCodeSearchResult
{
    public class ZCodeSearchResult : ViewComponent
    {
        public IViewComponentResult Invoke(ZCodeSearchResultModel model)
        {
            var actualModel = model ?? new ZCodeSearchResultModel();



            var Items = new List<ZCodeSearchResultItemModel>()
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
            };

            actualModel.Items = Items;

            return View("~/ViewComponents/ZCodeSearchResult/Default.cshtml", actualModel);
        }
    }
}