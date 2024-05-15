using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.Services
{
    public interface IWebRiskService
    {
        Task<bool> CheckForSecureUri(string url);
    }
}