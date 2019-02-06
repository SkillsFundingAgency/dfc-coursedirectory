using Dfc.CourseDirectory.Web.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfc.CourseDirectory.Web.Areas.Identity
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> User { get; set; }

    }
    
}
