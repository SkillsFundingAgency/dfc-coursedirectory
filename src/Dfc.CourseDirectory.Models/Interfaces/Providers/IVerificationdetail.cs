using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Providers
{
    public interface IVerificationdetail
    {
        string VerificationAuthority { get; set; }
        string VerificationID { get; set; }
    }
}
