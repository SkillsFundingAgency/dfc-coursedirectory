using System;
using System.Diagnostics;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Services;
using FluentValidation;

namespace Dfc.CourseDirectory.Core.Validation.TLevelValidation
{
    public static class RuleBuilderExtensions
    {
        public static void EntryRequirements<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.EntryRequirementsMaxLength)
                    .WithMessage($"Entry requirements must be {Constants.EntryRequirementsMaxLength} characters or fewer");
        }

        public static void HowYoullBeAssessed<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.HowYoullBeAssessedMaxLength)
                    .WithMessage($"How you'll be assessed must be {Constants.HowYoullBeAssessedMaxLength} characters or fewer");
        }

        public static void HowYoullLearn<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.HowYoullLearnMaxLength)
                    .WithMessage($"How you'll learn must be {Constants.HowYoullLearnMaxLength} characters or fewer");
        }

        public static void StartDate<T>(
            this IRuleBuilderInitial<T, DateInput> field,
            Guid? tLevelId,
            Guid providerId,
            Guid tLevelDefinitionId,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            field
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                    .WithMessage("Enter a start date")
                // Must be valid
                .Custom((date, ctx) => {                    
                    if (date == null || date.IsValid)
                    {
                        return;
                    }
                    var displayName = "Start date";
                    Debug.Assert(date.InvalidReasons != InvalidDateInputReasons.None);

                    // A date was provided but it's invalid; figure out an appropriate message.
                    // We expect to have InvalidReasons to be either: InvalidDate OR
                    // 1-2 of MissingDay, MissingMonth and MissingYear.

                    if ((date.InvalidReasons & InvalidDateInputReasons.InvalidDate) != 0)
                    {
                        ctx.AddFailure($"{displayName} must be a real date");
                        return;
                    }

                    var missingFields = EnumHelper.SplitFlags(date.InvalidReasons)
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
                })
                .CustomAsync(async (v, ctx, _) =>
                {
                    var existingTLevels = await sqlQueryDispatcher.ExecuteQuery(
                        new GetTLevelsForProvider() { ProviderId = providerId });

                    if (existingTLevels.Any(tl =>
                        tl.TLevelDefinition.TLevelDefinitionId == tLevelDefinitionId &&
                        tl.StartDate == v.Value &&
                        tl.TLevelId != tLevelId))
                    {
                        ctx.AddFailure("Start date already exists");
                    }
                });
        }

        public static void StartDate<T>(
            this IRuleBuilderInitial<T, DateTime?> field,
            Guid? tLevelId,
            Guid providerId,
            Guid tLevelDefinitionId,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            field
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                    .WithMessage("Enter a start date")
                // Must be valid
                .Custom((v, ctx) => {
                    var date = new DateInput(v.Value);
                    if (v == null || date.IsValid)
                    {
                        return;
                    }
                    var displayName = "Start date";
                    Debug.Assert(date.InvalidReasons != InvalidDateInputReasons.None);

                    // A date was provided but it's invalid; figure out an appropriate message.
                    // We expect to have InvalidReasons to be either: InvalidDate OR
                    // 1-2 of MissingDay, MissingMonth and MissingYear.

                    if ((date.InvalidReasons & InvalidDateInputReasons.InvalidDate) != 0)
                    {
                        ctx.AddFailure($"{displayName} must be a real date");
                        return;
                    }

                    var missingFields = EnumHelper.SplitFlags(date.InvalidReasons)
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
                })
                .CustomAsync(async (v, ctx, _) =>
                {
                    var existingTLevels = await sqlQueryDispatcher.ExecuteQuery(
                        new GetTLevelsForProvider() { ProviderId = providerId });

                    if (existingTLevels.Any(tl =>
                        tl.TLevelDefinition.TLevelDefinitionId == tLevelDefinitionId &&
                        tl.StartDate == v.Value &&
                        tl.TLevelId != tLevelId))
                    {
                        ctx.AddFailure("Start date already exists");
                    }
                });
        }

        public static void Website<T>(this IRuleBuilderInitial<T, string> field, IWebRiskService webRiskService)
        {
            field
                .NotEmpty()
                    .WithMessage("Enter a webpage")
                .Apply(Rules.Website)
                    .WithMessage("Website must be a real webpage")
                .MaximumLength(Constants.WebsiteMaxLength)
                    .WithMessage($"T Level webpage must be {Constants.WebsiteMaxLength} characters or fewer")        
                .Apply(Rules.SecureWebsite<T>(webRiskService))
                    .WithMessageFromErrorCode("GENERIC_WEBSITE_INSECURE");
        }

        public static void WhatYouCanDoNext<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.WhatYouCanDoNextMaxLength)
                    .WithMessage($"What you can do next must be {Constants.WhatYouCanDoNextMaxLength} characters or fewer");
        }

        public static void WhatYoullLearn<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.WhatYoullLearnMaxLength)
                    .WithMessage($"What you'll learn must be {Constants.WhatYoullLearnMaxLength} characters or fewer");
        }

        public static void WhoFor<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .NotEmpty()
                    .WithMessage("Enter who this T Level is for")
                .MaximumLength(Constants.WhoForMaxLength)
                    .WithMessage($"Who this T Level is for must be {Constants.WhoForMaxLength} characters or fewer");
        }

        public static void YourReference<T>(this IRuleBuilderInitial<T, string> field)
        {
            field
                .MaximumLength(Constants.YourReferenceMaxLength)
                    .WithMessage($"Your reference must be {Constants.YourReferenceMaxLength} characters or fewer");
        }
    }
}
