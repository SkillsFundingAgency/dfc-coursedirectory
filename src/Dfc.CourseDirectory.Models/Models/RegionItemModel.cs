﻿using System.Collections.Generic;

namespace Dfc.CourseDirectory.Models.Models
{
    public class RegionItemModel
    {
        public string Id { get; set; }
        public string RegionName { get; set; }
        public bool? Checked { get; set; }

        public Dictionary<string, string> SubRegionName { get; set; }
    }
}