using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction.Json
{
    public class ContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (!property.Writable)
            {
                var propertyInfo = member as PropertyInfo;
                var hasPrivateSetter = propertyInfo?.GetSetMethod(true) != null;
                property.Writable = hasPrivateSetter;
            }

            return property;
        }
    }
}
