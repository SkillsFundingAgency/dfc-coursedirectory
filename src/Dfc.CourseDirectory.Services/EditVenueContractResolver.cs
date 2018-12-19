using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services
{
    public class EditVenueContractResolver : PrivateSetterContractResolver
    {
        //protected override string ResolvePropertyName(string propertyName)
        //{
        //    var names = new Dictionary<string, string>
        //    {
        //        { "Id", "id" },
        //        { "Text", "text"}
        //    };

        //    if (names.ContainsKey(propertyName))
        //    {
        //        names.TryGetValue(propertyName, out string name);
        //        return name;
        //    }

        //    return propertyName;
        //}
    }
}