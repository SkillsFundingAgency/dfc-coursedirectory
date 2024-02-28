﻿using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetNonLarsSubTypeForProvider : ISqlQuery<IReadOnlyCollection<NonLarsSubType>>
    {
        public Guid ProviderId { get; set; }
    }
}
