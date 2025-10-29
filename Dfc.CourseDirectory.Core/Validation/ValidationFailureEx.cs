using FluentValidation.Results;

namespace Dfc.CourseDirectory.Core.Validation
{
    public static class ValidationFailureEx
    {
        public static ValidationFailure CreateFromErrorCode(string propertyName, string errorCode) =>
            new ValidationFailure(propertyName, ErrorRegistry.All[errorCode].GetMessage())
            {
                ErrorCode = errorCode
            };
    }
}
