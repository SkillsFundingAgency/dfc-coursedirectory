using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewComponents.VenueName
{
    public class VenueNameModel
    {
      
        public string VenueName { get; set; }
        public string LabelText { get; set; }
        public string HintText { get; set; }
        public string AriaDescribedBy { get; set; }
        public bool DuplicateFound { get; set; }
    }
}
