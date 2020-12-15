using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Dfc.CourseDirectory.WebV2
{
    public class AddFeaturePropertyModelConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var assmNamespace = GetType().Namespace;
            var featuresNamespace = assmNamespace + ".Features";

            var controllerFullName = controller.ControllerType.FullName;

            if (controllerFullName.StartsWith(featuresNamespace))
            {
                var featureName = controllerFullName[(featuresNamespace.Length + 1)..^(controller.ControllerType.Name.Length + 1)];

                controller.Properties.Add("Feature", featureName);
            }
        }
    }

    public class FeatureViewLocationExpander : IViewLocationExpander
    {
        private static readonly string[] _featureViewLocations = new[]
        {
            "/Features/{3}/{0}.cshtml",
            "/Features/{3}/View.cshtml"
        };

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.Values.ContainsKey("Feature") &&
                !context.ViewName.StartsWith("ViewComponents/") &&
                context.IsMainPage)
            {
                var featurePath = context.Values["Feature"].Replace(".", "/");

                return viewLocations.Concat(_featureViewLocations.Select(l => l.Replace("{3}", featurePath)));
            }
            else
            {
                return viewLocations;
            }
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            var controllerActionDescriptor = context.ActionContext.ActionDescriptor as ControllerActionDescriptor;

            if (controllerActionDescriptor != null
                && controllerActionDescriptor.Properties.ContainsKey("Feature"))
            {
                var featureName = (string)controllerActionDescriptor.Properties["Feature"];

                context.Values.Add("Feature", featureName);
            }
        }
    }
}
