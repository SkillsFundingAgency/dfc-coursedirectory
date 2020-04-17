using System;
using FluentValidation.Results;

namespace Dfc.CourseDirectory.Core.Validation
{
    public class ModelWithErrors<T>
    {
        public ModelWithErrors(T model, ValidationResult validationResult)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (validationResult == null)
            {
                throw new ArgumentNullException(nameof(validationResult));
            }

            Model = model;
            ValidationResult = validationResult;
        }

        public T Model { get; }

        public ValidationResult ValidationResult { get; }
    }
}
