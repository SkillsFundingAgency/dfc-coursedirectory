using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class InactiveProviderDataUploadRowInfoCollection : DataUploadRowInfoCollection<CsvInactiveProviderRow, InactiveProviderDataUploadRowInfo>
    {
        public InactiveProviderDataUploadRowInfoCollection(params InactiveProviderDataUploadRowInfo[] rows) :
           this(rows.AsEnumerable())
        {
        }

        public InactiveProviderDataUploadRowInfoCollection(IEnumerable<InactiveProviderDataUploadRowInfo> rows) :
            base(rows)
        {
        }
    }
}
