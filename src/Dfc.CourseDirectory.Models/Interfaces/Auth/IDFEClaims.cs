using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Auth
{
    public interface IDFEClaims
    {
        Guid ServiceId { get; }
        Guid OrganisationId { get; }
        Guid UserId { get; }
        string UserName { get; }
        string RoleType { get; }
        int? UKPRN { get; }
    }
}
