namespace Dfc.CourseDirectory.WebV2.Security
{
    public interface ICurrentUserProvider
    {
        AuthenticatedUserInfo GetCurrentUser();
    }
}
