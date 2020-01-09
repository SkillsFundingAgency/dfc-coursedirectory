namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class AuthenticatedUserInfo
    {
        public bool IsAuthenticated { get; private set; }

        public void Reset()
        {
            IsAuthenticated = true;
        }

        public void SetNotAuthenticated() => IsAuthenticated = false;
    }
}
