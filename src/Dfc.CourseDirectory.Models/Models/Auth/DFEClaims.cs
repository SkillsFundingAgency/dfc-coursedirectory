using Dfc.CourseDirectory.Models.Interfaces.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Models.Auth
{
    public class DFEClaims : IDFEClaims
    {
        public Guid ServiceId { get; set; }
        public Guid OrganisationId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string RoleType { get; set; }
        public int? UKPRN { get; set; }
    }
}
