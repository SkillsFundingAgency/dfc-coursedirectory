using Newtonsoft.Json.Serialization;

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
