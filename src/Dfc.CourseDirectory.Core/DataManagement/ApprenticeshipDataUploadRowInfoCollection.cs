using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class ApprenticeshipDataUploadRowInfoCollection : DataUploadRowInfoCollection<CsvApprenticeshipRow, ApprenticeshipDataUploadRowInfo>
    {
        public ApprenticeshipDataUploadRowInfoCollection(params ApprenticeshipDataUploadRowInfo[] rows) :
            this(rows.AsEnumerable())
        {
        }

        public ApprenticeshipDataUploadRowInfoCollection(IEnumerable<ApprenticeshipDataUploadRowInfo> rows) :
            base(rows)
        {
        }
    }

    public class ApprenticeshipDataUploadRowInfo : DataUploadRowInfo<CsvApprenticeshipRow>
    {
        public ApprenticeshipDataUploadRowInfo(CsvApprenticeshipRow data, int rowNumber, Guid apprenticeshipId, Guid? venueIdHint = null)
            : base(data, rowNumber)
        {
            ApprenticeshipId = apprenticeshipId;
            VenueIdHint = venueIdHint;
        }

        public Guid ApprenticeshipId { get; set; }

        public Guid? VenueIdHint { get; set; }
    }
}
