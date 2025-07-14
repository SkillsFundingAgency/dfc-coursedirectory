using System;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class ProviderUkprn
    {
        public Guid ProviderId { get; set; }
        public int Ukprn { get; set; }
        public int RowNum { get; set; }
    }
}
