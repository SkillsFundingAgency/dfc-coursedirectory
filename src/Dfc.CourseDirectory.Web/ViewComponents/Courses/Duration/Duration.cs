using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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
            model.DurationUnits.Add(defaultItem);
            model.DurationUnits.Add(new SelectListItem { Text = CourseDurationUnit.Minutes.ToString() });
            model.DurationUnits.Add(new SelectListItem { Text = CourseDurationUnit.Hours.ToString() });
            model.DurationUnits.Add(new SelectListItem { Text = CourseDurationUnit.Days.ToString() });
            model.DurationUnits.Add(new SelectListItem { Text = CourseDurationUnit.Weeks.ToString() });
            model.DurationUnits.Add(new SelectListItem { Text = CourseDurationUnit.Months.ToString() });
            model.DurationUnits.Add(new SelectListItem { Text = CourseDurationUnit.Years.ToString() });
            return View("~/ViewComponents/Courses/Duration/Default.cshtml", model);
        }
    }
}
