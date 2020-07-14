using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Dfc.CourseDirectory.WebV2.Validation
{
    public abstract class ValidatorBase<T> : AbstractValidator<T>
    {
        protected ValidatorBase(IActionContextAccessor actionContextAccessor)
        {
            ActionContext = actionContextAccessor.ActionContext;
        }

        public ActionContext ActionContext { get; }
    }
}
