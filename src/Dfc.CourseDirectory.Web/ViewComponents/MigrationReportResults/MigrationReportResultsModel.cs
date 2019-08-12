using Dfc.CourseDirectory.Models.Models.Courses;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewComponents.MigrationReportResults
{
    public class MigrationReportResultsModel
    {
        public IList<DfcMigrationReport> Rows { get; set; }

        public MigrationReportResultsModel(IList<DfcMigrationReport> rows = null)
        {
            Rows = rows;
        }
    }
}
