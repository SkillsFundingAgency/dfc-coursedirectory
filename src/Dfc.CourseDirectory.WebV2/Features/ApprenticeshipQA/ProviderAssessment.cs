using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.WebV2.Validation;
using FluentValidation;
using Mapster;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.ProviderAssessment
{
    public enum ErrorReason
    {
        ProviderDoesNotExist,
        NoValidSubmission,
    }

    public class Query : IRequest<OneOf<Error<ErrorReason>, ViewModel>>
    {
        public Guid ProviderId { get; set; }
    }

    public class ViewModel : Command
    {
        public string ProviderName { get; set; }
        public string BriefOverview { get; set; }
    }

    public class Command : IRequest<OneOf<ConfirmationViewModel, ModelWithErrors<ViewModel>>>
    {
        public Guid ProviderId { get; set; }
        public bool? CompliancePassed { get; set; }
        public ApprenticeshipQAProviderComplianceFailedReasons? ComplianceFailedReasons { get; set; }
        public string ComplianceComments { get; set; }
        public bool? StylePassed { get; set; }
        public ApprenticeshipQAProviderStyleFailedReasons? StyleFailedReasons { get; set; }
        public string StyleComments { get; set; }
    }

    public class ConfirmationViewModel
    {
        public Guid ProviderId { get; set; }
        public bool CompliancePassed { get; set; }
        public ApprenticeshipQAProviderComplianceFailedReasons ComplianceFailedReasons { get; set; }
        public string ComplianceComments { get; set; }
        public bool StylePassed { get; set; }
        public ApprenticeshipQAProviderStyleFailedReasons StyleFailedReasons { get; set; }
        public string StyleComments { get; set; }
        public bool Passed { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, OneOf<Error<ErrorReason>, ViewModel>>,
        IRequestHandler<Command, OneOf<ConfirmationViewModel, ModelWithErrors<ViewModel>>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
        }

        public Task<OneOf<Error<ErrorReason>, ViewModel>> Handle(Query request, CancellationToken cancellationToken) =>
            CreateViewModel(request.ProviderId);

        public async Task<OneOf<ConfirmationViewModel, ModelWithErrors<ViewModel>>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var errorVm = (await CreateViewModel(request.ProviderId)).AsT1;
                request.Adapt(errorVm);

                return new ModelWithErrors<ViewModel>(errorVm, validationResult);
            }

            // Sanitize request
            if (request.CompliancePassed == true)
            {
                request.ComplianceFailedReasons = ApprenticeshipQAProviderComplianceFailedReasons.None;
                request.ComplianceComments = null;
            }
            if (request.StylePassed == true)
            {
                request.StyleFailedReasons = ApprenticeshipQAProviderStyleFailedReasons.None;
                request.StyleComments = null;
            }

            var currentUserId = _currentUserProvider.GetCurrentUser().UserId;

            var passed = IsQAPassed(request.CompliancePassed.Value, request.StylePassed.Value);

            var submission = (await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = request.ProviderId
                })).AsT1;

            var overallPassed = submission.ApprenticeshipAssessmentsPassed.HasValue ?
                submission.ApprenticeshipAssessmentsPassed.Value && passed :
                (bool?)null;

            await _sqlQueryDispatcher.ExecuteQuery(
                new SetProviderApprenticeshipQAStatus()
                {
                    ProviderId = request.ProviderId,
                    ApprenticeshipQAStatus = ApprenticeshipQAStatus.InProgress
                });

            await _sqlQueryDispatcher.ExecuteQuery(
                new CreateApprenticeshipQAProviderAssessment()
                {
                    ApprenticeshipQASubmissionId = submission.ApprenticeshipQASubmissionId,
                    AssessedByUserId = currentUserId,
                    AssessedOn = _clock.UtcNow,
                    ComplianceComments = request.ComplianceComments,
                    ComplianceFailedReasons = request.ComplianceFailedReasons ?? ApprenticeshipQAProviderComplianceFailedReasons.None,
                    CompliancePassed = request.CompliancePassed.Value,
                    Passed = passed,
                    StyleComments = request.StyleComments,
                    StyleFailedReasons = request.StyleFailedReasons ?? ApprenticeshipQAProviderStyleFailedReasons.None,
                    StylePassed = request.StylePassed
                });

            await _sqlQueryDispatcher.ExecuteQuery(
                new UpdateApprenticeshipQASubmission()
                {
                    ApprenticeshipQASubmissionId = submission.ApprenticeshipQASubmissionId,
                    Passed = overallPassed,
                    LastAssessedByUserId = currentUserId,
                    LastAssessedOn = _clock.UtcNow,
                    ProviderAssessmentPassed = passed,
                    ApprenticeshipAssessmentsPassed = submission.ApprenticeshipAssessmentsPassed
                });

            var vm = request.Adapt<ConfirmationViewModel>();
            vm.Passed = IsQAPassed(request.CompliancePassed.Value, request.StylePassed.Value);

            return vm;
        }

        private async Task<OneOf<Error<ErrorReason>, ViewModel>> CreateViewModel(Guid providerId)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = providerId
                });

            if (provider == null)
            {
                return new Error<ErrorReason>(ErrorReason.ProviderDoesNotExist);
            }

            var qaStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId
                });

            if (qaStatus != ApprenticeshipQAStatus.Submitted &&
                qaStatus != ApprenticeshipQAStatus.InProgress)
            {
                return new Error<ErrorReason>(ErrorReason.NoValidSubmission);
            }

            var maybeLatestSubmission = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = providerId
                });

            if (maybeLatestSubmission.Value is None)
            {
                return new Error<ErrorReason>(ErrorReason.NoValidSubmission);
            }

            var latestSubmission = maybeLatestSubmission.AsT1;

            var assessment = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQAProviderAssessmentForSubmission()
                {
                    ApprenticeshipQASubmissionId = latestSubmission.ApprenticeshipQASubmissionId
                });

            return new ViewModel()
            {
                ProviderId = providerId,
                BriefOverview = latestSubmission.ProviderBriefOverview,
                ProviderName = provider.ProviderName,
                ComplianceComments = assessment.Match(_ => string.Empty, v => v.ComplianceComments),
                ComplianceFailedReasons = assessment.Match(_ => ApprenticeshipQAProviderComplianceFailedReasons.None, v => v.ComplianceFailedReasons),
                CompliancePassed = assessment.Match(_ => null, v => v.CompliancePassed),
                StyleComments = assessment.Match(_ => string.Empty, v => v.StyleComments),
                StyleFailedReasons = assessment.Match(_ => ApprenticeshipQAProviderStyleFailedReasons.None, v => v.StyleFailedReasons),
                StylePassed = assessment.Match(_ => null, v => v.StylePassed),
            };
        }

        private static bool IsQAPassed(bool compliancePassed, bool stylePassed) =>
            compliancePassed && stylePassed;

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.CompliancePassed)
                    .NotNull()
                    .WithMessageForAllRules("PLACEHOLDER");

                RuleFor(c => c.StylePassed)
                    .NotNull()
                    .WithMessageForAllRules("PLACEHOLDER");

                RuleFor(c => c.ComplianceFailedReasons)
                    .NotNull()
                    .NotEqual(ApprenticeshipQAProviderComplianceFailedReasons.None)
                    .When(c => c.CompliancePassed == false)
                    .WithMessageForAllRules("PLACEHOLDER");

                RuleFor(c => c.StyleFailedReasons)
                    .NotNull()
                    .NotEqual(ApprenticeshipQAProviderStyleFailedReasons.None)
                    .When(c => c.StylePassed == false)
                    .WithMessageForAllRules("PLACEHOLDER");
            }
        }
    }
}
