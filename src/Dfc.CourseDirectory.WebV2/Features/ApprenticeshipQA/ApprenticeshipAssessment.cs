using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
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

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.ApprenticeshipAssessment
{
    using CommandResponse = OneOf<ModelWithErrors<ViewModel>, Success>;

    public struct NoValidSubmission
    {
    }

    [JourneyState]
    public class JourneyModel
    {
        public Guid ApprenticeshipId { get; set; }
        public bool IsReadOnly { get; set; }
        public Guid ProviderId { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public bool? CompliancePassed { get; set; }
        public ApprenticeshipQAApprenticeshipComplianceFailedReasons? ComplianceFailedReasons { get; set; }
        public string ComplianceComments { get; set; }
        public bool? StylePassed { get; set; }
        public ApprenticeshipQAApprenticeshipStyleFailedReasons? StyleFailedReasons { get; set; }
        public string StyleComments { get; set; }
        public bool? Passed { get; set; }

        public bool GotAssessmentOutcome { get; set; }

        public bool IsApprenticeshipAssessmentPassed() => CompliancePassed.Value && StylePassed.Value;

        public bool? IsSubmissionPassed(bool? providerAssessmentPassed) =>
            providerAssessmentPassed.HasValue ?
                IsApprenticeshipAssessmentPassed() && providerAssessmentPassed.Value :
                (bool?)null;

        public void SetAssessmentOutcome(
            bool compliancePassed,
            ApprenticeshipQAApprenticeshipComplianceFailedReasons complianceFailedReasons,
            string complianceComments,
            bool stylePassed,
            ApprenticeshipQAApprenticeshipStyleFailedReasons styleFailedReasons,
            string styleComments)
        {
            CompliancePassed = compliancePassed;
            ComplianceFailedReasons = !compliancePassed ? complianceFailedReasons : ApprenticeshipQAApprenticeshipComplianceFailedReasons.None;
            ComplianceComments = !compliancePassed ? complianceComments : null;
            StylePassed = stylePassed;
            StyleFailedReasons = !stylePassed ? styleFailedReasons : ApprenticeshipQAApprenticeshipStyleFailedReasons.None;
            StyleComments = !stylePassed ? styleComments : null;
            GotAssessmentOutcome = true;
        }
    }

    public class JourneyModelInitializer
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderOwnershipCache _providerOwnershipCache;

        public JourneyModelInitializer(
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderOwnershipCache providerOwnershipCache)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerOwnershipCache = providerOwnershipCache;
        }

        public async Task<JourneyModel> Initialize(Guid apprenticeshipId)
        {
            var maybeProviderId = await _providerOwnershipCache.GetProviderForApprenticeship(apprenticeshipId);

            if (!maybeProviderId.HasValue)
            {
                throw new ResourceDoesNotExistException(ResourceType.Apprenticeship, apprenticeshipId);
            }

            var providerId = maybeProviderId.Value;

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

            var submissionApprenticeship = latestSubmission.Apprenticeships
                .SingleOrDefault(a => a.ApprenticeshipId == apprenticeshipId);

            if (submissionApprenticeship == null)
            {
                throw new InvalidStateException(InvalidStateReason.NoValidApprenticeshipQASubmission);
            }

            var latestAssessment = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQAApprenticeshipAssessmentForSubmission()
                {
                    ApprenticeshipQASubmissionId = latestSubmission.ApprenticeshipQASubmissionId,
                    ApprenticeshipId = apprenticeshipId
                });

            return new JourneyModel()
            {
                ApprenticeshipId = apprenticeshipId,
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
        public Guid ApprenticeshipId { get; set; }
        public Guid ProviderId { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public string ApprenticeshipMarketingInformation { get; set; }
        public bool IsReadOnly { get; set; }
        public bool ApprenticeshipIsNational { get; set; }
        public IReadOnlyCollection<ViewModelApprenticeshipClassroomLocation> ApprenticeshipClassroomLocations { get; set; }
        public IReadOnlyCollection<ViewModelApprenticeshipEmployerLocationRegion> ApprenticeshipEmployerLocationRegions { get; set; }
    }

    public class ViewModelApprenticeshipClassroomLocation
    {
        public string VenueName { get; set; }
        public IReadOnlyCollection<ApprenticeshipDeliveryMode> DeliveryModes { get; set; }
        public int Radius { get; set; }
    }

    public class ViewModelApprenticeshipEmployerLocationRegion
    {
        public string Name { get; set; }
        public IReadOnlyCollection<string> SubRegionNames { get; set; }
    }

    public class Command : IRequest<CommandResponse>
    {
        public bool? CompliancePassed { get; set; }
        public ApprenticeshipQAApprenticeshipComplianceFailedReasons? ComplianceFailedReasons { get; set; }
        public string ComplianceComments { get; set; }
        public bool? StylePassed { get; set; }
        public ApprenticeshipQAApprenticeshipStyleFailedReasons? StyleFailedReasons { get; set; }
        public string StyleComments { get; set; }
    }

    public class ConfirmationQuery : IRequest<ConfirmationViewModel>
    {
    }

    public class ConfirmationViewModel : IRequest<ConfirmationCommand>
    {
        public Guid ApprenticeshipId { get; set; }
        public Guid ProviderId { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public bool CompliancePassed { get; set; }
        public ApprenticeshipQAApprenticeshipComplianceFailedReasons ComplianceFailedReasons { get; set; }
        public string ComplianceComments { get; set; }
        public bool StylePassed { get; set; }
        public ApprenticeshipQAApprenticeshipStyleFailedReasons StyleFailedReasons { get; set; }
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
                request.ComplianceFailedReasons ?? ApprenticeshipQAApprenticeshipComplianceFailedReasons.None,
                request.ComplianceComments,
                request.StylePassed.Value,
                request.StyleFailedReasons ?? ApprenticeshipQAApprenticeshipStyleFailedReasons.None,
                request.StyleComments));

            return new Success();
        }

        public async Task<ConfirmationViewModel> Handle(ConfirmationQuery request, CancellationToken cancellationToken)
        {
            if (!_journeyInstance.State.GotAssessmentOutcome)
            {
                throw new InvalidStateException();
            }

            var apprenticeshipId = _journeyInstance.State.ApprenticeshipId;

            var submission = await GetSubmission();
            var submissionApprenticeship = submission.Apprenticeships.Single(a => a.ApprenticeshipId == apprenticeshipId);

            var vm = new ConfirmationViewModel()
            {
                ApprenticeshipId = _journeyInstance.State.ApprenticeshipId,
                ProviderId = _journeyInstance.State.ProviderId,
                ApprenticeshipTitle = submissionApprenticeship.ApprenticeshipTitle,
                ComplianceComments = _journeyInstance.State.ComplianceComments,
                ComplianceFailedReasons = _journeyInstance.State.ComplianceFailedReasons.Value,
                CompliancePassed = _journeyInstance.State.CompliancePassed.Value,
                Passed = _journeyInstance.State.IsApprenticeshipAssessmentPassed(),
                StyleComments = _journeyInstance.State.StyleComments,
                StyleFailedReasons = _journeyInstance.State.StyleFailedReasons.Value,
                StylePassed = _journeyInstance.State.StylePassed.Value
            };

            return vm;
        }

        public async Task<Success> Handle(ConfirmationCommand request, CancellationToken cancellationToken)
        {
            if (!_journeyInstance.State.GotAssessmentOutcome)
            {
                throw new InvalidStateException();
            }

            var apprenticeshipId = _journeyInstance.State.ApprenticeshipId;
            var providerId = _journeyInstance.State.ProviderId;

            var currentUserId = _currentUserProvider.GetCurrentUser().UserId;

            var submission = await GetSubmission();

            var overallPassed = _journeyInstance.State.IsSubmissionPassed(submission.ProviderAssessmentPassed);

            await _sqlQueryDispatcher.ExecuteQuery(
                new SetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId,
                    ApprenticeshipQAStatus = ApprenticeshipQAStatus.InProgress
                });

            await _sqlQueryDispatcher.ExecuteQuery(
                new CreateApprenticeshipQAApprenticeshipAssessment()
                {
                    ApprenticeshipQASubmissionId = submission.ApprenticeshipQASubmissionId,
                    ApprenticeshipId = apprenticeshipId,
                    AssessedByUserId = currentUserId,
                    AssessedOn = _clock.UtcNow,
                    ComplianceComments = _journeyInstance.State.ComplianceComments,
                    ComplianceFailedReasons = _journeyInstance.State.ComplianceFailedReasons.Value,
                    CompliancePassed = _journeyInstance.State.CompliancePassed.Value,
                    Passed = _journeyInstance.State.IsApprenticeshipAssessmentPassed(),
                    StyleComments = _journeyInstance.State.StyleComments,
                    StyleFailedReasons = _journeyInstance.State.StyleFailedReasons.Value,
                    StylePassed = _journeyInstance.State.StylePassed.Value
                });

            await _sqlQueryDispatcher.ExecuteQuery(
                new UpdateApprenticeshipQASubmission()
                {
                    ApprenticeshipQASubmissionId = submission.ApprenticeshipQASubmissionId,
                    Passed = overallPassed,
                    LastAssessedByUserId = currentUserId,
                    LastAssessedOn = _clock.UtcNow,
                    ProviderAssessmentPassed = submission.ProviderAssessmentPassed,
                    ApprenticeshipAssessmentsPassed = _journeyInstance.State.IsApprenticeshipAssessmentPassed()
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
            var apprenticeshipId = _journeyInstance.State.ApprenticeshipId;

            var submission = await GetSubmission();
            var submissionApprenticeship = submission.Apprenticeships.Single(a => a.ApprenticeshipId == apprenticeshipId);

            return new ViewModel()
            {
                ApprenticeshipId = _journeyInstance.State.ApprenticeshipId,
                ApprenticeshipMarketingInformation = Html.SanitizeHtml(submissionApprenticeship.ApprenticeshipMarketingInformation),
                ApprenticeshipTitle = submissionApprenticeship.ApprenticeshipTitle,
                ProviderId = _journeyInstance.State.ProviderId,
                ComplianceComments = _journeyInstance.State.ComplianceComments,
                ComplianceFailedReasons = _journeyInstance.State.ComplianceFailedReasons,
                CompliancePassed = _journeyInstance.State.CompliancePassed,
                StyleComments = _journeyInstance.State.StyleComments,
                StyleFailedReasons = _journeyInstance.State.StyleFailedReasons,
                StylePassed = _journeyInstance.State.StylePassed,
                IsReadOnly = _journeyInstance.State.IsReadOnly,
                ApprenticeshipIsNational = submissionApprenticeship.Locations.Any(l => l.IsT1 ? l.AsT1.IsNational : false),
                ApprenticeshipClassroomLocations = submissionApprenticeship.Locations
                    .Where(l => l.IsClassroomBased)
                    .Select(l => l.AsT0)
                    .Select(l => new ViewModelApprenticeshipClassroomLocation()
                    {
                        DeliveryModes = l.DeliveryModes,
                        Radius = l.Radius,
                        VenueName = l.VenueName
                    })
                    .OrderBy(l => l.VenueName)
                    .ToList(),
                ApprenticeshipEmployerLocationRegions = GroupRegions()
            };

            IReadOnlyCollection<ViewModelApprenticeshipEmployerLocationRegion> GroupRegions()
            {
                var subRegionIds = submissionApprenticeship.Locations
                    .Where(l => l.IsEmployerBased)
                    .Select(l => l.AsT1)
                    .Where(l => l.HasRegions)
                    .Select(l => l.AsT1)
                    .SelectMany(l => l.SubRegionIds);

                return Region.All
                    .SelectMany(r => r.SubRegions.Select(sr => new { SubRegion = sr, Region = r }))
                    .Where(r => subRegionIds.Contains(r.SubRegion.Id))
                    .GroupBy(x => x.Region)
                    .Select(g => new ViewModelApprenticeshipEmployerLocationRegion()
                    {
                        Name = g.Key.Name,
                        SubRegionNames = g.Select(sr => sr.SubRegion.Name).OrderBy(sr => sr).ToList()
                    })
                    .OrderBy(g => g.Name)
                    .ToList();
            }
        }

        private async Task<ApprenticeshipQASubmission> GetSubmission() =>
            await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = _journeyInstance.State.ProviderId
                });

        Guid IRestrictQAStatus<Command>.GetProviderId(Command request) => _journeyInstance.State.ProviderId;

        Guid IRestrictQAStatus<Query>.GetProviderId(Query request) => _journeyInstance.State.ProviderId;

        Guid IRestrictQAStatus<ConfirmationQuery>.GetProviderId(ConfirmationQuery request) => _journeyInstance.State.ProviderId;

        Guid IRestrictQAStatus<ConfirmationCommand>.GetProviderId(ConfirmationCommand request) => _journeyInstance.State.ProviderId;

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.CompliancePassed)
                    .NotNull()
                    .WithMessageForAllRules("An outcome must be selected");

                RuleFor(c => c.ComplianceFailedReasons)
                    .NotNull()
                    .NotEqual(ApprenticeshipQAApprenticeshipComplianceFailedReasons.None)
                    .When(c => c.CompliancePassed == false)
                    .WithMessageForAllRules("A reason must be selected");

                RuleFor(c => c.ComplianceComments)
                    .NotEmpty()
                    .When(c => c.CompliancePassed == false &&
                        (c.ComplianceFailedReasons?.HasFlag(ApprenticeshipQAApprenticeshipComplianceFailedReasons.Other) ?? false))
                    .WithMessageForAllRules("Enter comments for the reason selected");

                RuleFor(c => c.StylePassed)
                    .NotNull()
                    .WithMessageForAllRules("An outcome must be selected");

                RuleFor(c => c.StyleFailedReasons)
                    .NotNull()
                    .NotEqual(ApprenticeshipQAApprenticeshipStyleFailedReasons.None)
                    .When(c => c.StylePassed == false)
                    .WithMessageForAllRules("A reason must be selected");

                RuleFor(c => c.StyleComments)
                    .NotEmpty()
                    .When(c => c.StylePassed == false &&
                        (c.StyleFailedReasons?.HasFlag(ApprenticeshipQAApprenticeshipStyleFailedReasons.Other) ?? false))
                    .WithMessageForAllRules("Enter comments for the reason selected");
            }
        }
    }
}
