using System;
using System.Diagnostics;
using System.Linq;
using Dfc.CourseDirectory.Core.Models;
using FluentValidation;

namespace Dfc.CourseDirectory.Core.Validation
{
    public static class Rules
    {
        public static IRuleBuilderInitial<T, R> Apply<T, R>(
            this IRuleBuilder<T, R> field,
            Func<IRuleBuilder<T, R>, IRuleBuilderInitial<T, R>> addRule)
        {
            return addRule(field);
        }

        public static IRuleBuilderOptions<T, R> Apply<T, R>(
           this IRuleBuilder<T, R> field,
           Func<IRuleBuilder<T, R>, IRuleBuilderOptions<T, R>> addRule)
        {
            return addRule(field);
        }

        public static IRuleBuilderInitial<T, DateInput> Date<T>(
                IRuleBuilder<T, DateInput> field,
                string displayName) =>
            field
                .Custom((v, ctx) =>
                {
                    if (v == null || v.IsValid)
                    {
                        return;
                    }

                    Debug.Assert(v.InvalidReasons != InvalidDateInputReasons.None);

                    // A date was provided but it's invalid; figure out an appropriate message.
                    // We expect to have InvalidReasons to be either: InvalidDate OR
                    // 1-2 of MissingDay, MissingMonth and MissingYear.

                    if ((v.InvalidReasons & InvalidDateInputReasons.InvalidDate) != 0)
                    {
                        ctx.AddFailure($"{displayName} must be a real date");
                        return;
                    }

                    var missingFields = EnumHelper.SplitFlags(v.InvalidReasons)
                        .Aggregate(
                            Enumerable.Empty<string>(),
                            (acc, r) =>
                            {
                                var elementName = r switch
                                {
                                    InvalidDateInputReasons.MissingDay => "day",
                                    InvalidDateInputReasons.MissingMonth => "month",
                                    InvalidDateInputReasons.MissingYear => "year",
                                    _ => throw new NotSupportedException($"Unexpected {nameof(InvalidDateInputReasons)}: '{r}'.")
                                };

                                return acc.Append(elementName);
                            })
                        .ToArray();

                    Debug.Assert(missingFields.Length <= 2 && missingFields.Length > 0);

                    ctx.AddFailure($"{displayName} must include a {string.Join(" and ", missingFields)}");
                });

        public static IRuleBuilderOptions<T, string> PhoneNumber<T>(IRuleBuilder<T, string> field) =>
            field
                .Must(v => v == null || PhoneNumberHelper.IsValid(v));

        public static IRuleBuilderOptions<T, string> Postcode<T>(IRuleBuilder<T, string> field) =>
            field
                .Must(v => v == null || Models.Postcode.TryParse(v, out _));

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

                    return Uri.IsWellFormedUriString(withPrefix, UriKind.Absolute)
                        && Uri.TryCreate(withPrefix, UriKind.Absolute, out var uri)
                        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                        && uri.Host.Contains('.')
                        && !uri.Host.StartsWith('.')
                        && !uri.Host.EndsWith('.');
                });
    }
}
