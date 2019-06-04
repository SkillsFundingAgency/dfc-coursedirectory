using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dfc.CourseDirectory.Models.Models.Courses;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships
{


    public class DeliveryOptionsListItemModel
    {
        public string LocationId { get; set; }

        public string LocationName { get; set; }

        public string PostCode { get; set; }


        public string Delivery { get; set; }
        public string Radius { get; set; }

        public bool? National { get; set; }


    }
}
