using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.Duration
{
    public class Duration : ViewComponent
    {
        public IViewComponentResult Invoke(DurationModel model)
        {
            var defaultItem = new SelectListItem { Text = "Select", Value = "" };

            model.DurationUnits = new List<SelectListItem>();
            foreach (CourseDurationUnit eVal in Enum.GetValues(typeof(CourseDurationUnit)))
            {
                var item = new SelectListItem { Text = eVal.ToString() };
                model.DurationUnits.Add(item);
            };

            model.DurationUnits.Insert(0,model.DurationUnits.FirstOrDefault(x => x.Text.ToUpper()=="HOURS"));
            model.DurationUnits.RemoveAt(5);

            model.DurationUnits.Insert(0, defaultItem);

            return View("~/ViewComponents/Courses/Duration/Default.cshtml", model);
        }
    }
}
