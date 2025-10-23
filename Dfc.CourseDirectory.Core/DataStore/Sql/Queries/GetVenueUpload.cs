using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetVenueUpload : ISqlQuery<VenueUpload>
    {
        public Guid VenueUploadId { get; set; }
    }
}
