using Dfc.CourseDirectory.Services.Models;

namespace Dfc.CourseDirectory.Web.ViewModels.Migration
{
    public class DeleteViewModel
    {
        public MigrationDeleteOptions MigrationDeleteOptions { get; set; }
        public int? CourseErrors { get; set; }
    }
}
