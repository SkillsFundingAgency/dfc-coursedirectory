using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.RequestModels
{
    public class AddCourseSection1RequestModel
    {
        public string CourseFor { get; set; }
        public string EntryRequirements { get; set; }

        public string WhatWillLearn { get; set; }
    }
}
