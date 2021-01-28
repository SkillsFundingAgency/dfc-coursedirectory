using System;
using FluentValidation;

namespace Dfc.CourseDirectory.Core.Validation
{
    public static class Rules
    {
        public static IRuleBuilderOptions<T, R> Apply<T, R>(
            this IRuleBuilder<T, R> field,
            Func<IRuleBuilder<T, R>, IRuleBuilderOptions<T, R>> addRule)
        {
            return addRule(field);
        }

        public static IRuleBuilderOptions<T, string> PhoneNumber<T>(IRuleBuilder<T, string> field) =>
            field
                .Must(v => string.IsNullOrEmpty(v) || PhoneNumberHelper.IsValid(v));

        public static IRuleBuilderOptions<T, string> Postcode<T>(IRuleBuilder<T, string> field) =>
            field
                .Matches(PostcodeHelper.UkPostcodePattern);

        public static IRuleBuilderOptions<T, string> Website<T>(IRuleBuilder<T, string> field) =>
            field
                .Must(url =>
                {
                    if (string.IsNullOrEmpty(url))
                    {
                        return true;
                    }

                    var withPrefix = !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                        ? $"https://{url}"
                        : url;

                    return Uri.TryCreate(withPrefix, UriKind.Absolute, out var uri)
                        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                        && uri.Host.Contains('.')
                        && !uri.Host.StartsWith('.')
                        && !uri.Host.EndsWith('.');
                });
    }
}
