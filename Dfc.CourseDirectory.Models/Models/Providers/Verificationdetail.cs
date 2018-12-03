using Dfc.CourseDirectory.Models.Interfaces.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Providers
{
    public class Verificationdetail : IVerificationdetail
    {
        public string VerificationAuthority { get; set; }
        public string VerificationID { get; set; }
    }
}
