using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.Behaviors.Errors;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Security;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation;
using Mapster;
using MediatR;
using OneOf;
using OneOf.Types;
using Dfc.CourseDirectory.Core;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.ApprenticeshipAssessment
{
    using CommandResponse = OneOf<ModelWithErrors<ViewModel>, Success>;

    public struct NoValidSubmission
    {
    }

    public class FlowModel : IMptxState
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

    public class FlowModelInitializer
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderOwnershipCache _providerOwnershipCache;

        public FlowModelInitializer(
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderOwnershipCache providerOwnershipCache)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerOwnershipCache = providerOwnershipCache;
        }

        public async Task<FlowModel> Initialize(Guid apprenticeshipId)
        {
            var maybeProviderId = await _providerOwnershipCache.GetProviderForApprenticeship(apprenticeshipId);

            if (!maybeProviderId.HasValue)
            {
                throw new ErrorException<ApprenticeshipDoesNotExist>(new ApprenticeshipDoesNotExist());
            }

            var providerId = maybeProviderId.Value;

            var qaStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId
                });

            var maybeLatestSubmission = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = providerId
                });

            if (maybeLatestSubmission.Value is None)
            {
                throw new ErrorException<NoValidSubmission>(new NoValidSubmission());
            }

            var latestSubmission = maybeLatestSubmission.AsT1;

            var submissionApprenticeship = latestSubmission.Apprenticeships
                .SingleOrDefault(a => a.ApprenticeshipId == apprenticeshipId);

            if (submissionApprenticeship == null)
            {
                throw new ErrorException<NoValidSubmission>(new NoValidSubmission());
            }

            var latestAssessment = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQAApprenticeshipAssessmentForSubmission()
                {
                    ApprenticeshipQASubmissionId = latestSubmission.ApprenticeshipQASubmissionId,
                    ApprenticeshipId = apprenticeshipId
                });

            return new FlowModel()
            {
                ApprenticeshipId = apprenticeshipId,
                ProviderId = providerId,
                ComplianceComments = latestAssessment.Match(_ => string.Empty, v => v.ComplianceComments),
                ComplianceFailedReasons = latestAssessment.Match(_ => ApprenticeshipQAApprenticeshipComplianceFailedReasons.None, v => v.ComplianceFailedReasons),
                CompliancePassed = latestAssessment.Match(_ => null, v => v.CompliancePassed),
                StyleComments = latestAssessment.Match(_ => string.Empty, v => v.StyleComments),
                StyleFailedReasons = latestAssessment.Match(_ => ApprenticeshipQAApprenticeshipStyleFailedReasons.None, v => v.StyleFailedReasons),
                StylePassed = latestAssessment.Match(_ => null, v => v.StylePassed),
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
        public ApprenticeshipDeliveryModes DeliveryModes { get; set; }
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
        private readonly MptxInstanceContext<FlowModel> _flow;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock,
            MptxInstanceContext<FlowModel> flow)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
            _flow = flow;
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

            _flow.Update(s => s.SetAssessmentOutcome(
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
            if (!_flow.State.GotAssessmentOutcome)
            {
                throw new ErrorException<InvalidFlowState>(new InvalidFlowState());
            }

            var apprenticeshipId = _flow.State.ApprenticeshipId;

            var submission = await GetSubmission();
            var submissionApprenticeship = submission.Apprenticeships.Single(a => a.ApprenticeshipId == apprenticeshipId);

            var vm = new ConfirmationViewModel()
            {
                ProviderId = _flow.State.ProviderId,
                ApprenticeshipTitle = submissionApprenticeship.ApprenticeshipTitle,
                ComplianceComments = _flow.State.ComplianceComments,
                ComplianceFailedReasons = _flow.State.ComplianceFailedReasons.Value,
                CompliancePassed = _flow.State.CompliancePassed.Value,
                Passed = _flow.State.IsApprenticeshipAssessmentPassed(),
                StyleComments = _flow.State.StyleComments,
                StyleFailedReasons = _flow.State.StyleFailedReasons.Value,
                StylePassed = _flow.State.StylePassed.Value
            };

            return vm;
        }

        public async Task<Success> Handle(ConfirmationCommand request, CancellationToken cancellationToken)
        {
            if (!_flow.State.GotAssessmentOutcome)
            {
                throw new ErrorException<InvalidFlowState>(new InvalidFlowState());
            }

            var apprenticeshipId = _flow.State.ApprenticeshipId;
            var providerId = _flow.State.ProviderId;

            var currentUserId = _currentUserProvider.GetCurrentUser().UserId;

            var submission = await GetSubmission();

            var overallPassed = _flow.State.IsSubmissionPassed(submission.ProviderAssessmentPassed);

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
                    ComplianceComments = _flow.State.ComplianceComments,
                    ComplianceFailedReasons = _flow.State.ComplianceFailedReasons.Value,
                    CompliancePassed = _flow.State.CompliancePassed.Value,
                    Passed = _flow.State.IsApprenticeshipAssessmentPassed(),
                    StyleComments = _flow.State.StyleComments,
                    StyleFailedReasons = _flow.State.StyleFailedReasons.Value,
                    StylePassed = _flow.State.StylePassed.Value
                });

            await _sqlQueryDispatcher.ExecuteQuery(
                new UpdateApprenticeshipQASubmission()
                {
                    ApprenticeshipQASubmissionId = submission.ApprenticeshipQASubmissionId,
                    Passed = overallPassed,
                    LastAssessedByUserId = currentUserId,
                    LastAssessedOn = _clock.UtcNow,
                    ProviderAssessmentPassed = submission.ProviderAssessmentPassed,
                    ApprenticeshipAssessmentsPassed = _flow.State.IsApprenticeshipAssessmentPassed()
                });

            await _sqlQueryDispatcher.ExecuteQuery(
                new SetProviderApprenticeshipQAStatus()
                {
                    ProviderId = _flow.State.ProviderId,
                    ApprenticeshipQAStatus = ApprenticeshipQAStatus.InProgress
                });

            return new Success();
        }

        private async Task<ViewModel> CreateViewModel()
        {
            var apprenticeshipId = _flow.State.ApprenticeshipId;

            var submission = await GetSubmission();
            var submissionApprenticeship = submission.Apprenticeships.Single(a => a.ApprenticeshipId == apprenticeshipId);

            return new ViewModel()
            {
                ApprenticeshipMarketingInformation = Html.SanitizeHtml(submissionApprenticeship.ApprenticeshipMarketingInformation),
                ApprenticeshipTitle = submissionApprenticeship.ApprenticeshipTitle,
                ProviderId = _flow.State.ProviderId,
                ComplianceComments = _flow.State.ComplianceComments,
                ComplianceFailedReasons = _flow.State.ComplianceFailedReasons,
                CompliancePassed = _flow.State.CompliancePassed,
                StyleComments = _flow.State.StyleComments,
                StyleFailedReasons = _flow.State.StyleFailedReasons,
                StylePassed = _flow.State.StylePassed,
                IsReadOnly = _flow.State.IsReadOnly,
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
            (await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = _flow.State.ProviderId
                })).AsT1;

        Guid IRestrictQAStatus<Command>.GetProviderId(Command request) => _flow.State.ProviderId;

        Guid IRestrictQAStatus<Query>.GetProviderId(Query request) => _flow.State.ProviderId;

        Guid IRestrictQAStatus<ConfirmationQuery>.GetProviderId(ConfirmationQuery request) => _flow.State.ProviderId;

        Guid IRestrictQAStatus<ConfirmationCommand>.GetProviderId(ConfirmationCommand request) => _flow.State.ProviderId;

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
