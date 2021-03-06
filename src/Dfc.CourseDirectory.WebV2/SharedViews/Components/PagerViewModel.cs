﻿using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class PagerViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public Func<int, string> GetPageUrl { get; set; }
        public IReadOnlyList<int> PageNumbers { get; set; }
    }
}
