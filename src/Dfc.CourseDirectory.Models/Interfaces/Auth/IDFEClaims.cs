using Dfc.CourseDirectory.Models.Models.Auth;
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
        string RoleName { get; }
        IEnumerable<Role> Roles { get; }
        string UKPRN { get; }
    }

    public interface IRole
    {
        Guid Id { get; }
        string Name { get; }
        string Code { get; }

    }
}
