﻿using System;

namespace Dfc.CourseDirectory.WebV2.Models
{
    public class Framework
    {
        public Guid CosmosId { get; set; }
        public int FrameworkCode { get; set; }
        public int ProgType { get; set; }
        public int PathwayCode { get; set; }
        public string NasTitle { get; set; }
    }
}
