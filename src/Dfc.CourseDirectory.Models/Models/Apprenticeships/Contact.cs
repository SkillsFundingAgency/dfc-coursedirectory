using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Apprenticeships
{
    public class Contact : IContact
    {
        public string ContactUsUrl { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
    }
}
