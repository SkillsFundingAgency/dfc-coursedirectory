using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLatestVenueUploadForProviderWithStatus : ISqlQuery<VenueUpload>
    {
        public Guid ProviderId { get; set; }
        public IEnumerable<UploadStatus> Statuses { get; set; }
    }
}
