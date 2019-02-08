using Dfc.CourseDirectory.Models.Models.Auth;

namespace Dfc.CourseDirectory.Services.Interfaces.AuthService
{
    public interface IAuthService
    {
        AuthUserDetails GetDetailsByEmail(string email);
    }
}
