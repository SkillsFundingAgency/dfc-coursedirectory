using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Dfc.CourseDirectory.WebV2
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RequiresProviderContextAttribute : Attribute, IActionModelConvention, IControllerModelConvention
    {
        private void AddMetadataToAction(ActionModel action)
        {
            action.Properties.Add(typeof(RequiresProviderContextMarker), RequiresProviderContextMarker.Instance);
        }

        void IActionModelConvention.Apply(ActionModel action)
        {
            AddMetadataToAction(action);
        }

        void IControllerModelConvention.Apply(ControllerModel controller)
        {
            foreach (var action in controller.Actions)
            {
                AddMetadataToAction(action);
            }
        }
    }

    public sealed class RequiresProviderContextMarker
    {
        private RequiresProviderContextMarker() { }

        public static RequiresProviderContextMarker Instance { get; } = new RequiresProviderContextMarker();
    }
}
