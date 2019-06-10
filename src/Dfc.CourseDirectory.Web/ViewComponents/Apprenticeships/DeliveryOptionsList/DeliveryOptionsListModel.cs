using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dfc.CourseDirectory.Models.Models.Courses;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships
{
    public class DeliveryOptionsListModel
    {

        public bool? SummaryPage { get; set; }
        public List<DeliveryOptionsListItemModel> DeliveryOptionsListItemModel { get; set; }




    }
}
