using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public class Address : IAddress
    {
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string County { get; set; }

        public string Email { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string Phone { get; set; }

        public string Postcode { get; set; }

        public string Town { get; set; }

        public string Website { get; set; }


    }
}
