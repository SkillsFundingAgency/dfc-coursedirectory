﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.CostDescription
{
    public class CostDescriptionModel
    {
        [MaxLength(255, ErrorMessage = "Cost description must be 255 characters or less")]
        public string CostDescription { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
