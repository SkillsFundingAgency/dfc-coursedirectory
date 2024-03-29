﻿using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLatestUnpublishedCourseUploadForProvider : ISqlQuery<CourseUpload>
    {
        public Guid ProviderId { get; set; }
        public bool IsNonLars { get; set; }
    }
}
