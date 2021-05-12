using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLatestUnpublishedVenueUploadForProvider : ISqlQuery<VenueUpload>
    {
        public Guid ProviderId { get; set; }
    }
}
