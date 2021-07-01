using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class SubRegionSelectorViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(
            string id,
            string name,
            IEnumerable<string> selectedSubRegionIds)
        {
            var vm = new SubRegionSelectorViewModel()
            {
                Id = id,
                Name = name,
                SelectedSubRegionIds = selectedSubRegionIds?.ToArray() ?? Array.Empty<string>()
            };

            return View("~/SharedViews/Components/SubRegionSelector.cshtml", vm);
        }
    }
}
