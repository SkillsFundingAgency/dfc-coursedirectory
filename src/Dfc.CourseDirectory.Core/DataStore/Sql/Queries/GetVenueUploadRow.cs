using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetVenueUploadRow : ISqlQuery<VenueUploadRow>
    {
        public int VenueUploadRowId { get; set; }
    }
}
