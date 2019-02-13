using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Auth
{
    public interface IAuthUserDetails
    {
        Guid UserId { get; set; }
        string Email { get; set; }
        string UserName { get; set; }
        string NameOfUser { get; set; }
        Guid RoleId { get; set; }
        string RoleName { get; set; }
        string UKPRN { get; set; }
    }
}
