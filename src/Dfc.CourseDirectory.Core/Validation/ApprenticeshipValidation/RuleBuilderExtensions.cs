﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using FluentValidation;
using FluentValidation.Results;

namespace Dfc.CourseDirectory.Core.Validation.ApprenticeshipValidation
{
    public static class RuleBuilderExtensions
    {
        public static void ContactEmail<T>(this IRuleBuilderInitial<T, string> field) =>
            field
                .NotEmpty()
                    .WithMessageFromErrorCode("APPRENTICESHIP_EMAIL_REQUIRED")
                .EmailAddress()
                    .WithMessageFromErrorCode("APPRENTICESHIP_EMAIL_FORMAT");


        public static void ContactTelephone<T>(this IRuleBuilderInitial<T, string> field) =>
            field
                .NormalizeWhitespace()
                .NotEmpty()
                    .WithMessageFromErrorCode("APPRENTICESHIP_TELEPHONE_REQUIRED")
                .Apply(Rules.PhoneNumber)
                    .WithMessageFromErrorCode("APPRENTICESHIP_TELEPHONE_FORMAT");

        public static void ContactWebsite<T>(this IRuleBuilderInitial<T, string> field) =>
            field
                .Apply(Rules.Website)
                    .WithMessageFromErrorCode("APPRENTICESHIP_CONTACTUS_FORMAT");

        public static void MarketingInformation<T>(this IRuleBuilderInitial<T, string> field) =>
            field
                .NotEmpty()
                    .WithMessageFromErrorCode("APPRENTICESHIP_INFORMATION_REQUIRED")
                .ValidHtml(maxLength: Constants.MarketingInformationStrippedMaxLength)
                .WithMessageFromErrorCode("APPRENTICESHIP_INFORMATION_MAXLENGTH");

        public static void Website<T>(this IRuleBuilderInitial<T, string> field) =>
            field
                .Apply(Rules.Website)
                    .WithMessageFromErrorCode("APPRENTICESHIP_WEBSITE_FORMAT");

        public static void StandardCode<T>(this IRuleBuilderInitial<T, int?> field, IList<ParsedCsvApprenticeshipRow> allRows, Func<T, ApprenticeshipLocationType?> getDeliveryMethod) =>
            field
            .NotNull()
            .WithMessageFromErrorCode("APPRENTICESHIP_STANDARD_CODE_REQUIRED")
            .Custom((v, ctx) =>
             {
                 var obj = (T)ctx.InstanceToValidate;
                 var deliveryMethod = getDeliveryMethod(obj);
                 var count = allRows.Count(c => c.StandardCode == v?.ToString() && c.ResolvedDeliveryMethod == deliveryMethod && (deliveryMethod == ApprenticeshipLocationType.EmployerBased || deliveryMethod == ApprenticeshipLocationType.ClassroomBasedAndEmployerBased));
                 if (count > 1)
                 {
                     ctx.AddFailure(CreateFailure("APPRENTICESHIP_DUPLICATE_STANDARDCODE"));
                 }


                 ValidationFailure CreateFailure(string errorCode) =>
                    ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
             });


        public static void StandardVersion<T>(this IRuleBuilderInitial<T, int?> field) =>
            field
                .NotNull()
                    .WithMessageFromErrorCode("APPRENTICESHIP_STANDARD_VERSION_REQUIRED");

        public static void YourVenueReference<T>(
              this IRuleBuilderInitial<T, string> field,
              Func<T, ApprenticeshipLocationType?> getDeliveryMode,
              Func<T, string> getVenueName,
              Guid? matchedVenueId)
        {
            field
                .Cascade(CascadeMode.Stop)
                .NormalizeWhitespace()
                .Custom((v, ctx) =>
                {
                    var obj = (T)ctx.InstanceToValidate;

                    var deliveryMode = getDeliveryMode(obj);
                    var isSpecified = !string.IsNullOrEmpty(v);

                    if (deliveryMode != ApprenticeshipLocationType.ClassroomBased)
                    {
                        if (isSpecified)
                            ctx.AddFailure(CreateFailure("APPRENTICESHIP_PROVIDER_VENUE_REF_NOT_ALLOWED"));
                        return;
                    }

                    // If not specified and Venue Name isn't specified then it's required
                    if (!isSpecified && string.IsNullOrEmpty(getVenueName(obj)))
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_VENUE_REQUIRED"));
                        return;
                    }

                    // If specified then it must match a venue
                    if (isSpecified && !matchedVenueId.HasValue)
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_PROVIDER_VENUE_REF_INVALID"));
                        return;
                    }

                    ValidationFailure CreateFailure(string errorCode) =>
                        ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                });
        }


        public static void Venue<T>(
            this IRuleBuilderInitial<T, string> field,
            Func<T, ApprenticeshipLocationType?> getDeliveryMode)
        {
            field
                .NormalizeWhitespace()
                .Custom((v, ctx) =>
                {
                    var obj = (T)ctx.InstanceToValidate;

                    var deliveryMode = getDeliveryMode(obj);
                    var isSpecified = !string.IsNullOrEmpty(v);

                    // Not allowed for delivery modes other than classroom based
                    if (isSpecified && deliveryMode != ApprenticeshipLocationType.ClassroomBased)
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_VENUE_NOT_ALLOWED"));
                        return;
                    }

                    if (deliveryMode != ApprenticeshipLocationType.ClassroomBased)
                    {
                        return;
                    }

                    ValidationFailure CreateFailure(string errorCode) =>
                        ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                });
        }

        public static void Radius<T>(this IRuleBuilderInitial<T, int?> field,
            Func<T, ApprenticeshipLocationType?> getDeliveryMethod)
        {
            field
                 .Custom((v, ctx) =>
                 {
                     var obj = (T)ctx.InstanceToValidate;
                     var deliveryMethod = getDeliveryMethod(obj);
                     if (deliveryMethod == ApprenticeshipLocationType.ClassroomBased ||
                        deliveryMethod == ApprenticeshipLocationType.ClassroomBasedAndEmployerBased)
                     {
                         if (!v.HasValue)
                         {
                             ctx.AddFailure(CreateFailure("APPRENTICESHIP_RADIUS_REQUIRED"));
                         }
                         else if (v.Value < Constants.RadiusRangeMin || v.Value > Constants.RadiusRangeMax)
                             ctx.AddFailure(CreateFailure("APPRENTICESHIP_RADIUS_INVALID"));
                     }
                     else
                     {
                         if (v.HasValue)
                         {
                             ctx.AddFailure(CreateFailure("APPRENTICESHIP_RADIUS_NOT_ALLOWED"));

                         }
                     }
                     ValidationFailure CreateFailure(string errorCode) =>
                         ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                 });
        }

        public static void DeliveryMode<T>(this IRuleBuilderInitial<T, IEnumerable<ApprenticeshipDeliveryMode>> field, Func<T, ApprenticeshipLocationType?> getDeliveryMethod)
        {
            field
                 .Custom((v, ctx) =>
                 {
                     var obj = (T)ctx.InstanceToValidate;
                     var deliveryMethod = getDeliveryMethod(obj);

                     if (deliveryMethod == ApprenticeshipLocationType.ClassroomBased ||
                        deliveryMethod == ApprenticeshipLocationType.ClassroomBasedAndEmployerBased)
                     {
                         if (!v.Contains(ApprenticeshipDeliveryMode.BlockRelease) && !v.Contains(ApprenticeshipDeliveryMode.DayRelease))
                         {
                             ctx.AddFailure(CreateFailure("APPRENTICESHIP_DELIVERYMODE_MUSTBE_DAY_OR_BLOCK"));
                         }
                     }

                     if (deliveryMethod == ApprenticeshipLocationType.EmployerBased)
                     {
                         // Allow Employer to be specified but nothing else
                         if (v != null && v.Any() && !v.SequenceEqual(new[] { ApprenticeshipDeliveryMode.EmployerAddress }))
                         {
                             ctx.AddFailure(CreateFailure("APPRENTICESHIP_DELIVERYMODE_NOT_ALLOWED"));
                         }
                     }

                     ValidationFailure CreateFailure(string errorCode) =>
                         ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                 });

        }

        public static void NationalDelivery<T>(this IRuleBuilderInitial<T, bool?> field, Func<T, ApprenticeshipLocationType?> getDeliveryMethod)
        {
            field
                 .Custom((v, ctx) =>
                 {
                     var obj = (T)ctx.InstanceToValidate;
                     var deliveryMethod = getDeliveryMethod(obj);
                     if (deliveryMethod != ApprenticeshipLocationType.EmployerBased)
                     {
                         if (v.HasValue)
                         {
                             ctx.AddFailure(CreateFailure("APPRENTICESHIP_NATIONALDELIVERY_NOT_ALLOWED"));
                         }
                         else
                             return;
                     }

                     ValidationFailure CreateFailure(string errorCode) =>
                         ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                 });
        }


        public static void DeliveryMethod<T>(this IRuleBuilderInitial<T, ApprenticeshipLocationType?> field)
        {
            field
                .NotNull()
                    .WithErrorCode("APPRENTICESHIP_DELIVERY_METHOD_REQUIRED");
        }

        public static void SubRegions<T>(
            this IRuleBuilderInitial<T, IReadOnlyCollection<Region>> field,
            Func<T, bool> subRegionsWereSpecified,
            Func<T, ApprenticeshipLocationType?> getDeliveryMethod,
            Func<T, bool> getNationalDelivery
            )
        {
            field
                .Custom((v, ctx) =>
                {
                    var obj = (T)ctx.InstanceToValidate;

                    var deliveryMethod = getDeliveryMethod(obj);
                    var isSpecified = subRegionsWereSpecified(obj);
                    var isNationalDelivery = getNationalDelivery(obj);


                    if (isSpecified && (deliveryMethod != ApprenticeshipLocationType.EmployerBased || isSpecified && isNationalDelivery))
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_SUBREGIONS_NOT_ALLOWED"));
                    }
                    if (deliveryMethod != ApprenticeshipLocationType.EmployerBased)
                    {
                        return;
                    }
                    if (isSpecified && (v == null || v.Count == 0))
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_SUBREGIONS_INVALID"));
                    }

                    if (!isSpecified && (v == null || v.Count == 0) && deliveryMethod == ApprenticeshipLocationType.EmployerBased && !isNationalDelivery)
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_SUBREGIONS_REQUIRED"));
                    }

                    ValidationFailure CreateFailure(string errorCode) =>
                        ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                });
        }

        public static void VenueName<T>(
            this IRuleBuilderInitial<T, string> field,
            Func<T, ApprenticeshipLocationType?> getDeliveryMethod,
            Func<T, string> getProviderVenueRef,
            Guid? matchedVenueId)
        {
            field
                .NormalizeWhitespace()
                .Custom((v, ctx) =>
                {
                    var obj = (T)ctx.InstanceToValidate;

                    var deliveryMode = getDeliveryMethod(obj);
                    var isSpecified = !string.IsNullOrEmpty(v);

                    // Not allowed for delivery modes other than classroom based
                    if (isSpecified && deliveryMode != ApprenticeshipLocationType.ClassroomBased && deliveryMode != ApprenticeshipLocationType.ClassroomBasedAndEmployerBased)
                    {
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_VENUE_NAME_NOT_ALLOWED"));
                        return;
                    }

                    if (deliveryMode != ApprenticeshipLocationType.ClassroomBased)
                    {
                        return;
                    }

                    if (isSpecified && !matchedVenueId.HasValue)
                    {
                        // We don't want both a ref and a name but if the ref resolves a venue and that venue's name
                        // matches this name then we let it go. If it doesn't match then yield an error.
                        if (!string.IsNullOrEmpty(getProviderVenueRef(obj)))
                        {
                            ctx.AddFailure(CreateFailure("APPRENTICESHIP_VENUE_NAME_NOT_ALLOWED_WITH_REF"));
                            return;
                        }

                        // Couldn't find a match from name
                        ctx.AddFailure(CreateFailure("APPRENTICESHIP_VENUE_NAME_INVALID"));
                        return;
                    }

                    ValidationFailure CreateFailure(string errorCode) =>
                        ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, errorCode);
                });
        }

    }
}
