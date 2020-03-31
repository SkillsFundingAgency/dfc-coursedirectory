using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.Behaviors.Errors;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
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
    using CommandResponse = OneOf<ModelWithErrors<ViewModel>, ConfirmationViewModel>;

    public struct NoValidSubmission
    {
    }

    public class Query : IRequest<ViewModel>, IProviderScopedRequest
    {
        public Guid ProviderId { get; set; }
    }

    public class ViewModel : Command
    {
        public string ProviderName { get; set; }
        public string MarketingInformation { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class Command : IRequest<CommandResponse>, IProviderScopedRequest
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
        IRequestHandler<Query, ViewModel>,
        IRestrictQAStatus<Query>,
        IRequestHandler<Command, CommandResponse>,
        IRestrictQAStatus<Command>
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

        IEnumerable<ApprenticeshipQAStatus> IRestrictQAStatus<Command>.PermittedStatuses { get; } = new[]
        {
            ApprenticeshipQAStatus.Submitted,
            ApprenticeshipQAStatus.InProgress
        };

        IEnumerable<ApprenticeshipQAStatus> IRestrictQAStatus<Query>.PermittedStatuses { get; } = new[]
        {
            ApprenticeshipQAStatus.Submitted,
            ApprenticeshipQAStatus.InProgress,
            ApprenticeshipQAStatus.Failed,
            ApprenticeshipQAStatus.Passed,
            ApprenticeshipQAStatus.UnableToComplete
        };

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var data = await CheckStatus(request.ProviderId);
            return CreateViewModel(data);
        }

        public async Task<CommandResponse> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var data = await CheckStatus(request.ProviderId);

            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = CreateViewModel(data);
                request.Adapt(vm);

                return new ModelWithErrors<ViewModel>(vm, validationResult);
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

            var overallPassed = data.LatestSubmission.ApprenticeshipAssessmentsPassed.HasValue ?
                data.LatestSubmission.ApprenticeshipAssessmentsPassed.Value && passed :
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
                    ApprenticeshipQASubmissionId = data.LatestSubmission.ApprenticeshipQASubmissionId,
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
                    ApprenticeshipQASubmissionId = data.LatestSubmission.ApprenticeshipQASubmissionId,
                    Passed = overallPassed,
                    LastAssessedByUserId = currentUserId,
                    LastAssessedOn = _clock.UtcNow,
                    ProviderAssessmentPassed = passed,
                    ApprenticeshipAssessmentsPassed = data.LatestSubmission.ApprenticeshipAssessmentsPassed
                });

            var confirmVm = CreateViewModel(data).Adapt<ConfirmationViewModel>();
            request.Adapt(confirmVm);
            confirmVm.Passed = IsQAPassed(request.CompliancePassed.Value, request.StylePassed.Value);

            return confirmVm;
        }

        private async Task<Data> CheckStatus(Guid providerId)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = providerId
                });

            if (provider == null)
            {
                throw new ErrorException<ProviderDoesNotExist>(new ProviderDoesNotExist());
            }

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

            var assessment = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQAProviderAssessmentForSubmission()
                {
                    ApprenticeshipQASubmissionId = latestSubmission.ApprenticeshipQASubmissionId
                });

            return new Data()
            {
                Provider = provider,
                QAStatus = qaStatus.ValueOrDefault(),
                LatestSubmission = latestSubmission,
                LatestAssessment = assessment
            };
        }

        private ViewModel CreateViewModel(Data data) => new ViewModel()
        {
            ProviderId = data.Provider.Id,
            MarketingInformation = data.LatestSubmission.ProviderMarketingInformation,
            ProviderName = data.Provider.ProviderName,
            ComplianceComments = data.LatestAssessment.Match(_ => string.Empty, v => v.ComplianceComments),
            ComplianceFailedReasons = data.LatestAssessment.Match(_ => ApprenticeshipQAProviderComplianceFailedReasons.None, v => v.ComplianceFailedReasons),
            CompliancePassed = data.LatestAssessment.Match(_ => null, v => v.CompliancePassed),
            StyleComments = data.LatestAssessment.Match(_ => string.Empty, v => v.StyleComments),
            StyleFailedReasons = data.LatestAssessment.Match(_ => ApprenticeshipQAProviderStyleFailedReasons.None, v => v.StyleFailedReasons),
            StylePassed = data.LatestAssessment.Match(_ => null, v => v.StylePassed),
            IsReadOnly = !(data.QAStatus == ApprenticeshipQAStatus.Submitted || data.QAStatus == ApprenticeshipQAStatus.InProgress)
        };

        private static bool IsQAPassed(bool compliancePassed, bool stylePassed) =>
            compliancePassed && stylePassed;

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
