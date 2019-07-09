using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Auth;

namespace Dfc.CourseDirectory.Services.Interfaces.AuthService
{
    public interface IAuthService
    {
        Task<AuthUserDetails> GetDetailsByEmail(string email);
        Task<string> GetProviderType(string UKPRN, string roleName);
    }
}
