﻿using Dfc.CourseDirectory.Models.Interfaces.ApprenticeshipsSearch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.ApprenticeshipsSearch
{
    public class BulkProviderModel : IBulkProviderModel
    {
        public int id { get; set; }
        public int ukprn { get; set; }
        public string name { get; set; }
        public string tradingName { get; set; }
        public Boolean nationalProvider { get; set; }
        public string marketingInfo { get; set; }
        public string email { get; set; }
        public string website { get; set; }
        public string phone { get; set; }
        public double? learnerSatisfaction { get; set; }
        public double? employerSatisfaction { get; set; }
        public List<BulkProviderStandardModel> standards { get; set; }
        public List<BulkProviderFrameworkModel> frameworks { get; set; }
        public List<BulkProviderLocationModel> locations { get; set; }

        public BulkProviderModel()
        {
            standards = new List<BulkProviderStandardModel>();
            frameworks = new List<BulkProviderFrameworkModel>();
            locations = new List<BulkProviderLocationModel>();
        }
    }
}
