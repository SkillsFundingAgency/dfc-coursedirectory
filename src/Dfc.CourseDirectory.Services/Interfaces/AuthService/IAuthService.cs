using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.Interfaces.AuthService
{
    public interface IAuthService
    {
        Task<string> GetProviderType(string UKPRN, string roleName);
    }
}
