
using System;
using Microsoft.AspNetCore.Identity;


namespace Dfc.ProviderPortal.FindACourse.Models
{
    public class APIUser : IdentityUser {
        public string Password { get; set; }         // Only populated for API users from JSON file, empty otherwise
    }

}
