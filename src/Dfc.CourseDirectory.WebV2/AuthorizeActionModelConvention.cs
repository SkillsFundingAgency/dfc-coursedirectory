using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Dfc.CourseDirectory.WebV2
{
    public class AuthorizeActionModelConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            var v2Assembly = typeof(AuthorizeActionModelConvention).Assembly;
            if (action.Controller.ControllerType.Assembly == v2Assembly)
            {
                action.Filters.Add(new AuthorizeFilter());
            }
        }
    }
}
