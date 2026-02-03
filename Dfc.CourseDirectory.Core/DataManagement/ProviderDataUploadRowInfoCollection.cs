using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class ProviderDataUploadRowInfoCollection : DataUploadRowInfoCollection<CsvProviderRow, ProviderDataUploadRowInfo>
    {
        public ProviderDataUploadRowInfoCollection(params ProviderDataUploadRowInfo[] rows) :
           this(rows.AsEnumerable())
        {
        }

        public ProviderDataUploadRowInfoCollection(IEnumerable<ProviderDataUploadRowInfo> rows) :
            base(rows)
        {
        }
    }
    public class ProviderDataUploadRowInfo : DataUploadRowInfo<CsvProviderRow>
    {
        public ProviderDataUploadRowInfo(CsvProviderRow data, int rowNumber, Guid providerId)
            : base(data, rowNumber)
        {
            ProviderId = providerId;
        }

        public Guid ProviderId { get; set; }
    }
}
