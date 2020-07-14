using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Dfc.CourseDirectory.WebV2
{
    public class V2ActionModelConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            // Check by namespace - can't check assembly as we need to include test project actions too
            var isV2Action = action.ActionMethod.DeclaringType.Namespace.StartsWith("Dfc.CourseDirectory.WebV2");

            if (isV2Action)
            {
                // All actions require authenticated users by default
                action.Filters.Add(new AuthorizeFilter());
            }
        }
    }
}
