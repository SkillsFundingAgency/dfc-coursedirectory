
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace Dfc.CourseDirectory.Web.ViewComponents.CourseProviderReference
{
    public class CourseProviderReferenceModel
    {
        public string CourseProviderReference { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
    }
}
