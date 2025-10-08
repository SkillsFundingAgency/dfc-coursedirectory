using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetVenueUploadRows : ISqlQuery<(IReadOnlyCollection<VenueUploadRow> Rows, int LastRowNumber)>
    {
        public Guid VenueUploadId { get; set; }
    }
}