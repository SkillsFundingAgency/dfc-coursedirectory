using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.Security
{
    public interface ISignInAction
    {
        Task OnUserSignedIn(SignInContext context);
    }
}
