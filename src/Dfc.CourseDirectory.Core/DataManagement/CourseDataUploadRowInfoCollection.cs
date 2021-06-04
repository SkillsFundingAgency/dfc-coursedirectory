using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class CourseDataUploadRowInfoCollection : DataUploadRowInfoCollection<CsvCourseRow, CourseDataUploadRowInfo>
    {
        public CourseDataUploadRowInfoCollection(params CourseDataUploadRowInfo[] rows) :
            this(rows.AsEnumerable())
        {
        }

        public CourseDataUploadRowInfoCollection(IEnumerable<CourseDataUploadRowInfo> rows) :
            base(rows)
        {
        }
    }

    public class CourseDataUploadRowInfo : DataUploadRowInfo<CsvCourseRow>
    {
        public CourseDataUploadRowInfo(CsvCourseRow data, int rowNumber)
            : base(data, rowNumber)
        {
        }
    }
}
