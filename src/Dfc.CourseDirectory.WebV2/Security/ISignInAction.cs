using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.Security
{
    public interface ISignInAction
    {
        Task OnUserSignedIn(SignInContext context);
    }
}
