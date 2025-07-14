using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.WebV2.ViewComponents.Courses.Duration
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
