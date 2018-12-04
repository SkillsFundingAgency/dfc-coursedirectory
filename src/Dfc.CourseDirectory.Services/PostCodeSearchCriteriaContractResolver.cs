using Newtonsoft.Json.Serialization;

namespace Dfc.CourseDirectory.Services
{
    public class PostCodeSearchCriteriaContractResolver : PrivateSetterContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }
}