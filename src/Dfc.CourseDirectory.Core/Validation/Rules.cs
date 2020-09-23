using System;
using System.Text.RegularExpressions;
using FluentValidation;

namespace Dfc.CourseDirectory.Core.Validation
{
    public static class Rules
    {
        private static readonly Regex _ukPhoneNumberPattern = new Regex(
            @"^((\(?0\d{4}\)?\s?\d{3}\s?\d{3})|(\(?0\d{3}\)?\s?\d{3}\s?\d{4})|(\(?0\d{2}\)?\s?\d{4}\s?\d{4}))(\s?\#(\d{4}|\d{3}))?$",
            RegexOptions.Compiled);

        private static readonly Regex _ukPostcodePattern = new Regex(
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
                .Matches(_ukPhoneNumberPattern);

        public static IRuleBuilderOptions<T, string> Postcode<T>(IRuleBuilder<T, string> field) =>
            field
                .Matches(_ukPostcodePattern);

        public static IRuleBuilderOptions<T, string> Website<T>(IRuleBuilder<T, string> field) =>
            field
                .Must(url =>
                {
                    if (string.IsNullOrEmpty(url))
                    {
                        return true;
                    }

                    var withPrefix = !url.Contains("://") ? $"http://{url}" : url;

                    return Uri.TryCreate(withPrefix, UriKind.Absolute, out var uri) &&
                        (uri.Scheme == "http" || uri.Scheme == "https");
                });
    }
}
