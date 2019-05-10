
using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;


namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class ProviderDetailsAddOrEditViewModel
    {


        public string UKPRN { get; set; }


        public string AliasName { get; set; }

        public string BriefOverview { get; set; }



    }
}
