using FluentValidation;
using FluentValidation.Internal;

namespace Dfc.CourseDirectory.Core.Validation
{
    public static class RuleBuilderExtensions
    {
        public static IRuleBuilderInitial<T, string> NormalizeWhitespace<T>(this IRuleBuilderInitial<T, string> rule)
        {
            // Convert empty strings and whitespace to null
            return rule.Transform(v => string.IsNullOrWhiteSpace(v) ? null : v);
        }

        public static IRuleBuilderOptions<T, TProperty> WithMessageForAllRules<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> rule,
            string errorMessage)
        {
            foreach (var item in (rule as RuleBuilder<T, TProperty>).Rule.Validators)
            {
                item.Options.SetErrorMessage(errorMessage);
            }

            return rule;
        }

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
