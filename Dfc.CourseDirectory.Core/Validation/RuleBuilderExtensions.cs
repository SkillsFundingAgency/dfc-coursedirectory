using FluentValidation;

namespace Dfc.CourseDirectory.Core.Validation
{
    public static class RuleBuilderExtensions
    {
        public static IRuleBuilderOptions<T, TProperty> WithMessageFromError<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> rule,
            Error error)
        {
            return rule
                .WithErrorCode(error.ErrorCode)
                .WithMessage(error.GetMessage());
        }

        public static IRuleBuilderOptions<T, TProperty> WithMessageFromErrorCode<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> rule,
            string errorCode)
        {
            return WithMessageFromError(rule, ErrorRegistry.All[errorCode]);
        }
    }
}
