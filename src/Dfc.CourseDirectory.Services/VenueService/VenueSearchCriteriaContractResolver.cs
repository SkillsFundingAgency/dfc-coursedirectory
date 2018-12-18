using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.VenueService
{
    public class VenueSearchCriteriaContractResolver : PrivateSetterContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToUpper();
        }
    }
}
