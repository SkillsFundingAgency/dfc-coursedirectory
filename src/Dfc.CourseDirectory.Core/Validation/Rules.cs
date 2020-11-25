using System;
using System.Text.RegularExpressions;
using FluentValidation;

namespace Dfc.CourseDirectory.Core.Validation
{
    public static class Rules
    {
        public static readonly Regex UkPostcodePattern = new Regex(
            @"^([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\s?[0-9][A-Za-z]{2})$",
            RegexOptions.Compiled);

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
                .Matches(UkPostcodePattern);

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
