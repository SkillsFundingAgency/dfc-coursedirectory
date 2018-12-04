using Dfc.CourseDirectory.Models.Interfaces.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Providers
{
    public class Contactpersonaldetails : IContactpersonaldetails
    {
        public string[] PersonNameTitle { get; set; }
        public string[] PersonGivenName { get; set; }
        public string PersonFamilyName { get; set; }
        public object PersonNameSuffix { get; set; }
        public object PersonRequestedName { get; set; }
    }
}
