using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class NonLarsCourseDataUploadRowInfoCollection : DataUploadRowInfoCollection<CsvNonLarsCourseRow, NonLarsCourseDataUploadRowInfo>
    {
        public NonLarsCourseDataUploadRowInfoCollection(params NonLarsCourseDataUploadRowInfo[] rows) :
            this(rows.AsEnumerable())
        {
        }

        public NonLarsCourseDataUploadRowInfoCollection(IEnumerable<NonLarsCourseDataUploadRowInfo> rows) :
            base(rows)
        {
        }
    }

    public class NonLarsCourseDataUploadRowInfo : DataUploadRowInfo<CsvNonLarsCourseRow>
    {
        public NonLarsCourseDataUploadRowInfo(CsvNonLarsCourseRow data, int rowNumber, Guid courseId, Guid? venueIdHint = null)
            : base(data, rowNumber)
        {
            CourseId = courseId;
            VenueIdHint = venueIdHint;
        }

        public Guid CourseId { get; set; }

        public Guid? VenueIdHint { get; set; }
    }
}
