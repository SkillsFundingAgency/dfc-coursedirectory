using Newtonsoft.Json.Serialization;

namespace Dfc.CourseDirectory.Services
{
    public class LarsSearchCriteriaContractResolver : PrivateSetterContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            if (propertyName.ToLower()=="searchfields")
            {
                return "searchFields";
            }
            return propertyName.ToLower();
        }
    }
}