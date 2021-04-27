﻿using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetVenueUploadPublished : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid VenueUploadId { get; set; }
        public DateTime PublishedOn { get; set; }
    }
}
