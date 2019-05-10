
using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;


namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class ProviderDetailsViewModel
    {
        public string ProviderName { get; set; }

        public string UKPRN { get; set; }

        public string Status { get; set; }

        public string LegalName { get; set; }

        public string TradingName { get; set; }
        public string AliasName { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string TownCity { get; set; }

        public string County { get; set; }

        public string PostCode { get; set; }

        public string Web { get; set; }

        public string Email { get; set; }

        public string Telephone { get; set; }

        public string BriefOverview { get; set; }



    }
}
