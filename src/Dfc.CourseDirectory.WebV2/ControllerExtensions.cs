using Dfc.CourseDirectory.WebV2.Validation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2
{
    public static class ControllerExtensions
    {
        public static ViewResult ViewFromErrors<T>(this Controller controller, ModelWithErrors<T> failed)
        {
            failed.ValidationResult.AddToModelState(controller.ModelState, null);

            var result = controller.View(failed.Model);
            result.StatusCode = 400;
            return result;
        }
    }
}
