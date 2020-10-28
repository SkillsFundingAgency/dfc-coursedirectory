
using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Spatial;
using Dfc.ProviderPortal.FindACourse.Models;


namespace Dfc.ProviderPortal.FindACourse.Interfaces
{
    public interface IAzureSearchCourseDetail //: ICourse
    {
        Course Course { get; set; }
        dynamic Provider { get; set; }
        dynamic Venue { get; set; }
        dynamic Qualification { get; set; }
    }
}
