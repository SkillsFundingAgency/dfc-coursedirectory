using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatWillLearn
{
    public class WhatWillLearnModel
    {
        [MaxLength(500, ErrorMessage = "What you will learn must be 500 characters or less")]
        public string WhatWillLearn { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
