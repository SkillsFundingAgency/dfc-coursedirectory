namespace Dfc.CourseDirectory.Core.Security
{
    public interface ICurrentUserProvider
    {
        AuthenticatedUserInfo GetCurrentUser();
    }
}
