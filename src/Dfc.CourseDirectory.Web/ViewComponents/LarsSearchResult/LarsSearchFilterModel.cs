﻿using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewComponents.LarsSearchResult
{
    public class LarsSearchFilterModel
    {
        public string Title { get; set; }
        public IEnumerable<LarsFilterItemModel> Items { get; set; }
    }
}