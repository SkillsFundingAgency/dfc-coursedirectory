using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class CourseDataUploadRowInfoCollection : DataUploadRowInfoCollection<CsvCourseRow, CourseDataUploadRowInfo>
    {
        public CourseDataUploadRowInfoCollection(int lastRowNumber, params CourseDataUploadRowInfo[] rows) :
            this(lastRowNumber, rows.AsEnumerable())
        {
        }

        public CourseDataUploadRowInfoCollection(int lastRowNumber, IEnumerable<CourseDataUploadRowInfo> rows) :
            base(lastRowNumber, rows)
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
