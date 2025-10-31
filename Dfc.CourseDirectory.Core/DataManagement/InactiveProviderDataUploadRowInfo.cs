using System;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class InactiveProviderDataUploadRowInfo : DataUploadRowInfo<CsvInactiveProviderRow>
    {
        public InactiveProviderDataUploadRowInfo(CsvInactiveProviderRow data, int rowNumber, Guid providerId)
            : base(data, rowNumber)
        {
            ProviderId = providerId;
        }

        public Guid ProviderId { get; set; }
    }
}
