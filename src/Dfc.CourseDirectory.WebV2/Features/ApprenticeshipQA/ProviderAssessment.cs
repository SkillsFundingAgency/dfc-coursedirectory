using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation;
using FormFlow;
using Mapster;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.ProviderAssessment
{
    using CommandResponse = OneOf<ModelWithErrors<ViewModel>, Success>;

    public struct NoValidSubmission
    {
    }

    [JourneyState]
    public class JourneyModel
    {
        public Guid ProviderId { get; set; }
        public bool IsReadOnly { get; set; }
        public bool? CompliancePassed { get; set; }
        public ApprenticeshipQAProviderComplianceFailedReasons? ComplianceFailedReasons { get; set; }
        public string ComplianceComments { get; set; }
        public bool? StylePassed { get; set; }
        public ApprenticeshipQAProviderStyleFailedReasons? StyleFailedReasons { get; set; }
        public string StyleComments { get; set; }

        public bool GotAssessmentOutcome { get; set; }

        public bool IsProviderAssessmentPassed() => CompliancePassed.Value && StylePassed.Value;

        public bool? IsSubmissionPassed(bool? apprenticeshipAssessmentPassed) =>
            apprenticeshipAssessmentPassed.HasValue ?
                IsProviderAssessmentPassed() && apprenticeshipAssessmentPassed.Value :
                (bool?)null;

        public void SetAssessmentOutcome(
            bool compliancePassed,
            ApprenticeshipQAProviderComplianceFailedReasons complianceFailedReasons,
            string complianceComments,
            bool stylePassed,
            ApprenticeshipQAProviderStyleFailedReasons styleFailedReasons,
            string styleComments)
        {
            CompliancePassed = compliancePassed;
            ComplianceFailedReasons = !compliancePassed ? complianceFailedReasons : ApprenticeshipQAProviderComplianceFailedReasons.None;
            ComplianceComments = !compliancePassed ? complianceComments : null;
            StylePassed = stylePassed;
            StyleFailedReasons = !stylePassed ? styleFailedReasons : ApprenticeshipQAProviderStyleFailedReasons.None;
            StyleComments = !stylePassed ? styleComments : null;
            GotAssessmentOutcome = true;
        }
    }

    public class JourneyModelInitializer
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public JourneyModelInitializer(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<JourneyModel> Initialize(Guid providerId)
        {
            var qaStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId
                });

            var latestSubmission = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = providerId
                });

            if (latestSubmission == null)
            {
                throw new InvalidStateException(InvalidStateReason.NoValidApprenticeshipQASubmission);
            }

            var latestAssessment = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQAProviderAssessmentForSubmission()
                {
                    ApprenticeshipQASubmissionId = latestSubmission.ApprenticeshipQASubmissionId
                });

            return new JourneyModel()
            {
                ProviderId = providerId,
                ComplianceComments = latestAssessment?.ComplianceComments,
                ComplianceFailedReasons = latestAssessment?.ComplianceFailedReasons,
                CompliancePassed = latestAssessment?.CompliancePassed,
                StyleComments = latestAssessment?.StyleComments,
                StyleFailedReasons = latestAssessment?.StyleFailedReasons,
                StylePassed = latestAssessment?.StylePassed,
                IsReadOnly = !(qaStatus == ApprenticeshipQAStatus.Submitted || qaStatus == ApprenticeshipQAStatus.InProgress),
            };
        }
    }

    public class Query : IRequest<ViewModel>
    {
    }

    public class ViewModel : Command
    {
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string MarketingInformation { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class Command : IRequest<CommandResponse>
    {
        public bool? CompliancePassed { get; set; }
        public ApprenticeshipQAProviderComplianceFailedReasons? ComplianceFailedReasons { get; set; }
        public string ComplianceComments { get; set; }
        public bool? StylePassed { get; set; }
        public ApprenticeshipQAProviderStyleFailedReasons? StyleFailedReasons { get; set; }
        public string StyleComments { get; set; }
    }

    public class ConfirmationQuery : IRequest<ConfirmationViewModel>
    {
    }

    public class ConfirmationViewModel : IRequest<ConfirmationCommand>
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

    public class ConfirmationCommand : IRequest<Success>
    {
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRestrictQAStatus<Query>,
        IRequestHandler<Command, CommandResponse>,
        IRestrictQAStatus<Command>,
        IRequestHandler<ConfirmationQuery, ConfirmationViewModel>,
        IRestrictQAStatus<ConfirmationQuery>,
        IRequestHandler<ConfirmationCommand, Success>,
        IRestrictQAStatus<ConfirmationCommand>
    {
        private static readonly IEnumerable<ApprenticeshipQAStatus> _submittableStatuses = new[]
        {
            ApprenticeshipQAStatus.Submitted,
            ApprenticeshipQAStatus.InProgress
        };

        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;
        private readonly JourneyInstance<JourneyModel> _journeyInstance;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock,
            JourneyInstance<JourneyModel> journeyInstance)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
            _journeyInstance = journeyInstance;
        }

        IEnumerable<ApprenticeshipQAStatus> IRestrictQAStatus<Command>.PermittedStatuses => _submittableStatuses;

        IEnumerable<ApprenticeshipQAStatus> IRestrictQAStatus<Query>.PermittedStatuses { get; } = new[]
        {
            ApprenticeshipQAStatus.Submitted,
            ApprenticeshipQAStatus.InProgress,
            ApprenticeshipQAStatus.Failed,
            ApprenticeshipQAStatus.Passed,
            ApprenticeshipQAStatus.UnableToComplete
        };

        IEnumerable<ApprenticeshipQAStatus> IRestrictQAStatus<ConfirmationQuery>.PermittedStatuses => _submittableStatuses;

        IEnumerable<ApprenticeshipQAStatus> IRestrictQAStatus<ConfirmationCommand>.PermittedStatuses => _submittableStatuses;

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken) => CreateViewModel();

        public async Task<CommandResponse> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = await CreateViewModel();
                request.Adapt(vm);

                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            _journeyInstance.UpdateState(s => s.SetAssessmentOutcome(
                request.CompliancePassed.Value,
                request.ComplianceFailedReasons ?? ApprenticeshipQAProviderComplianceFailedReasons.None,
                request.ComplianceComments,
                request.StylePassed.Value,
                request.StyleFailedReasons ?? ApprenticeshipQAProviderStyleFailedReasons.None,
                request.StyleComments));

            return new Success();
        }

        public Task<ConfirmationViewModel> Handle(ConfirmationQuery request, CancellationToken cancellationToken)
        {
            if (!_journeyInstance.State.GotAssessmentOutcome)
            {
                throw new InvalidStateException();
            }

            var vm = new ConfirmationViewModel()
            {
                ProviderId = _journeyInstance.State.ProviderId,
                ComplianceComments = _journeyInstance.State.ComplianceComments,
                ComplianceFailedReasons = _journeyInstance.State.ComplianceFailedReasons.Value,
                CompliancePassed = _journeyInstance.State.CompliancePassed.Value,
                Passed = _journeyInstance.State.IsProviderAssessmentPassed(),
                StyleComments = _journeyInstance.State.StyleComments,
                StyleFailedReasons = _journeyInstance.State.StyleFailedReasons.Value,
                StylePassed = _journeyInstance.State.StylePassed.Value
            };

            return Task.FromResult(vm);
        }

        public async Task<Success> Handle(ConfirmationCommand request, CancellationToken cancellationToken)
        {
            if (!_journeyInstance.State.GotAssessmentOutcome)
            {
                throw new InvalidStateException();
            }

            var currentUserId = _currentUserProvider.GetCurrentUser().UserId;
            var now = _clock.UtcNow;

            var submission = await GetSubmission();

            var overallPassed = _journeyInstance.State.IsSubmissionPassed(submission.ApprenticeshipAssessmentsPassed);

            await _sqlQueryDispatcher.ExecuteQuery(
                new SetProviderApprenticeshipQAStatus()
                {
                    ProviderId = _journeyInstance.State.ProviderId,
                    ApprenticeshipQAStatus = ApprenticeshipQAStatus.InProgress
                });

            await _sqlQueryDispatcher.ExecuteQuery(
                new CreateApprenticeshipQAProviderAssessment()
                {
                    ApprenticeshipQASubmissionId = submission.ApprenticeshipQASubmissionId,
                    AssessedByUserId = currentUserId,
                    AssessedOn = now,
                    ComplianceComments = _journeyInstance.State.ComplianceComments,
                    ComplianceFailedReasons = _journeyInstance.State.ComplianceFailedReasons ?? ApprenticeshipQAProviderComplianceFailedReasons.None,
                    CompliancePassed = _journeyInstance.State.CompliancePassed.Value,
                    Passed = _journeyInstance.State.IsProviderAssessmentPassed(),
                    StyleComments = _journeyInstance.State.StyleComments,
                    StyleFailedReasons = _journeyInstance.State.StyleFailedReasons ?? ApprenticeshipQAProviderStyleFailedReasons.None,
                    StylePassed = _journeyInstance.State.StylePassed
                });

            await _sqlQueryDispatcher.ExecuteQuery(
                new UpdateApprenticeshipQASubmission()
                {
                    ApprenticeshipQASubmissionId = submission.ApprenticeshipQASubmissionId,
                    Passed = overallPassed,
                    LastAssessedByUserId = currentUserId,
                    LastAssessedOn = now,
                    ProviderAssessmentPassed = _journeyInstance.State.IsProviderAssessmentPassed(),
                    ApprenticeshipAssessmentsPassed = submission.ApprenticeshipAssessmentsPassed
                });

            await _sqlQueryDispatcher.ExecuteQuery(
                new SetProviderApprenticeshipQAStatus()
                {
                    ProviderId = _journeyInstance.State.ProviderId,
                    ApprenticeshipQAStatus = ApprenticeshipQAStatus.InProgress
                });

            return new Success();
        }

        private async Task<ViewModel> CreateViewModel()
        {
            var providerId = _journeyInstance.State.ProviderId;

            var provider = await _sqlQueryDispatcher.ExecuteQuery(
                 new GetProviderById()
                 {
                     ProviderId = providerId
                 });

            var submission = await GetSubmission();

            return new ViewModel()
            {
                ProviderId = providerId,
                MarketingInformation = Html.SanitizeHtml(submission.ProviderMarketingInformation),
                ProviderName = provider.ProviderName,
                ComplianceComments = _journeyInstance.State.ComplianceComments,
                ComplianceFailedReasons = _journeyInstance.State.ComplianceFailedReasons,
                CompliancePassed = _journeyInstance.State.CompliancePassed,
                StyleComments = _journeyInstance.State.StyleComments,
                StyleFailedReasons = _journeyInstance.State.StyleFailedReasons,
                StylePassed = _journeyInstance.State.StylePassed,
                IsReadOnly = _journeyInstance.State.IsReadOnly
            };
        }

        private async Task<ApprenticeshipQASubmission> GetSubmission() =>
            await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = _journeyInstance.State.ProviderId
                });

        Guid IRestrictQAStatus<Query>.GetProviderId(Query request) => _journeyInstance.State.ProviderId;

        Guid IRestrictQAStatus<Command>.GetProviderId(Command request) => _journeyInstance.State.ProviderId;

        Guid IRestrictQAStatus<ConfirmationQuery>.GetProviderId(ConfirmationQuery request) => _journeyInstance.State.ProviderId;

        Guid IRestrictQAStatus<ConfirmationCommand>.GetProviderId(ConfirmationCommand request) => _journeyInstance.State.ProviderId;

        private class Data
        {
            public Provider Provider { get; set; }
            public ApprenticeshipQAStatus QAStatus { get; set; }
            public ApprenticeshipQASubmission LatestSubmission { get; set; }
            public OneOf<None, ApprenticeshipQAProviderAssessment> LatestAssessment { get; set; }
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.CompliancePassed)
                    .NotNull()
                    .WithMessageForAllRules("An outcome must be selected");

                RuleFor(c => c.ComplianceFailedReasons)
                    .NotNull()
                    .NotEqual(ApprenticeshipQAProviderComplianceFailedReasons.None)
                    .When(c => c.CompliancePassed == false)
                    .WithMessageForAllRules("A reason must be selected");

                RuleFor(c => c.ComplianceComments)
                    .NotEmpty()
                    .When(c => c.CompliancePassed == false &&
                        (c.ComplianceFailedReasons?.HasFlag(ApprenticeshipQAProviderComplianceFailedReasons.Other) ?? false))
                    .WithMessageForAllRules("Enter comments for the reason selected");

                RuleFor(c => c.StylePassed)
                    .NotNull()
                    .WithMessageForAllRules("An outcome must be selected");

                RuleFor(c => c.StyleFailedReasons)
                    .NotNull()
                    .NotEqual(ApprenticeshipQAProviderStyleFailedReasons.None)
                    .When(c => c.StylePassed == false)
                    .WithMessageForAllRules("A reason must be selected");

                RuleFor(c => c.StyleComments)
                    .NotEmpty()
                    .When(c => c.StylePassed == false &&
                        (c.StyleFailedReasons?.HasFlag(ApprenticeshipQAProviderStyleFailedReasons.Other) ?? false))
                    .WithMessageForAllRules("Enter comments for the reason selected");
            }
        }
    }
}
