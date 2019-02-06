using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.Areas.Identity.Data
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string UKPRN { get; set; }
        public string ObjectId { get; set; }
        public string TenantId { get; set; }
        public bool IsAdmin { get; set; }
        public string UserPrincipalName { get; set; }
    }
}
