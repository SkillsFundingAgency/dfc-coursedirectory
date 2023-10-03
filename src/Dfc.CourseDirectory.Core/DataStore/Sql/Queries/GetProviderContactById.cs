﻿using System;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderContactById : ISqlQuery<ProviderContact>
    {
        public Guid ProviderId { get; set; }
    }
}
