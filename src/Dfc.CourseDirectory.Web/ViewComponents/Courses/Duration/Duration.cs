using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.Duration
{
    public class Duration : ViewComponent
    {
        public IViewComponentResult Invoke(DurationModel model)
        {
            var defaultItem = new SelectListItem { Text = "Select", Value = "" };

            model.DurationUnits = new List<SelectListItem>();
            foreach (DurationUnit eVal in DurationUnit.GetValues(typeof(DurationUnit)))
            {
                if (eVal.ToString().ToUpper() != "UNDEFINED")
                {
                    var item = new SelectListItem { Text = WebHelper.GetEnumDescription(eVal) };
                    model.DurationUnits.Add(item);
                }
            };

            model.DurationUnits.Insert(0,model.DurationUnits.FirstOrDefault(x => x.Text.ToUpper()=="HOURS"));
            model.DurationUnits.RemoveAt(5);

            model.DurationUnits.Insert(0, defaultItem);

            return View("~/ViewComponents/Courses/Duration/Default.cshtml", model);
        }
    }
}
