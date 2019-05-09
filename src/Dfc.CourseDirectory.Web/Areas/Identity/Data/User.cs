using Microsoft.AspNetCore.Identity;

namespace Dfc.CourseDirectory.Web.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the User class
    public class User : IdentityUser
    {
        public bool PasswordResetRequired { get; set; }
    }
}
