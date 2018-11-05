using Newtonsoft.Json.Serialization;

namespace Dfc.CourseDirectory.Services
{
    public class LarsSearchCriteriaContractResolver : PrivateSetterContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }
}