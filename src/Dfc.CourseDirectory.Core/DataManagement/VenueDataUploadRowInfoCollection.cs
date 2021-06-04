using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class VenueDataUploadRowInfoCollection : DataUploadRowInfoCollection<CsvVenueRow, VenueDataUploadRowInfo>
    {
        public VenueDataUploadRowInfoCollection(int lastRowNumber, params VenueDataUploadRowInfo[] rows) :
            this(lastRowNumber, rows.AsEnumerable())
        {
        }

        public VenueDataUploadRowInfoCollection(int lastRowNumber, IEnumerable<VenueDataUploadRowInfo> rows) :
            base(rows)
        {
            LastRowNumber = lastRowNumber;
        }

        public int LastRowNumber { get; }
    }

    public class VenueDataUploadRowInfo : DataUploadRowInfo<CsvVenueRow>
    {
        public VenueDataUploadRowInfo(CsvVenueRow data, int rowNumber, bool isSupplementary)
            : base(data, rowNumber)
        {
            IsSupplementary = isSupplementary;
        }

        public bool IsSupplementary { get; }
    }
}
