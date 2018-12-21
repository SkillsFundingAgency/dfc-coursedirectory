using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.WhatYouNeed
{
    public class WhatYouNeedModel
    {
        [MaxLength(500, ErrorMessage = "What you’ll need to bring must be 500 characters or less")]
        public string WhatYouNeed { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
