using System.Globalization;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfc.CourseDirectory.WebV2.ModelBinding
{
    public class DeliveryModeModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(CourseDeliveryMode))
            {
                return Task.CompletedTask;
            }

            var valueProvider = new QueryStringValueProvider(
                BindingSource.Query,
                bindingContext.HttpContext.Request.Query,
                CultureInfo.InvariantCulture);

            var valueProviderResult = valueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult.Length == 0)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "No DeliveryMode provided.");
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            CourseDeliveryMode? deliveryMode = valueProviderResult.FirstValue.ToLowerInvariant() switch
            {
                "classroom" => CourseDeliveryMode.ClassroomBased,
                "classroombased" => CourseDeliveryMode.ClassroomBased,
                "classroom-based" => CourseDeliveryMode.ClassroomBased,
                "classroom based" => CourseDeliveryMode.ClassroomBased,
                "online" => CourseDeliveryMode.Online,
                "work" => CourseDeliveryMode.WorkBased,
                "workbased" => CourseDeliveryMode.WorkBased,
                "work-based" => CourseDeliveryMode.WorkBased,
                "work based" => CourseDeliveryMode.WorkBased,
                _ => null
            };

            if (deliveryMode == null)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Invalid DeliveryMode provided: '{valueProviderResult.FirstValue}'.");
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(deliveryMode.Value);
            return Task.CompletedTask;
        }
    }
}
