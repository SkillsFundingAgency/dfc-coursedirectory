﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
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
    using QueryResponse = OneOf<Error<ErrorReason>, ViewModel>;
    using CommandResponse = OneOf<Error<ErrorReason>, ModelWithErrors<ViewModel>, ConfirmationViewModel>;

    public enum ErrorReason
    {
        ProviderDoesNotExist,
        NoValidSubmission,
    }

    public class Query : IRequest<QueryResponse>
    {
        public Guid ProviderId { get; set; }
    }

    public class ViewModel : Command
    {
        public string ProviderName { get; set; }
        public string MarketingInformation { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class Command : IRequest<CommandResponse>
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
        IRequestHandler<Query, QueryResponse>,
        IRestrictQAStatus<Query, QueryResponse>,
        IRequestHandler<Command, CommandResponse>,
        IRestrictQAStatus<Command, CommandResponse>
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

        IEnumerable<ApprenticeshipQAStatus> IRestrictQAStatus<Command, CommandResponse>.PermittedStatuses { get; } = new[]
        {
            ApprenticeshipQAStatus.Submitted,
            ApprenticeshipQAStatus.InProgress
        };

        IEnumerable<ApprenticeshipQAStatus> IRestrictQAStatus<Query, QueryResponse>.PermittedStatuses { get; } = new[]
        {
            ApprenticeshipQAStatus.Submitted,
            ApprenticeshipQAStatus.InProgress,
            ApprenticeshipQAStatus.Failed,
            ApprenticeshipQAStatus.Passed,
            ApprenticeshipQAStatus.UnableToComplete
        };

        public async Task<QueryResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var errorOrData = await CheckStatus(request.ProviderId);

            if (errorOrData.Value is Error<ErrorReason>)
            {
                return errorOrData.AsT0;
            }
            else
            {
                return CreateViewModel(errorOrData.AsT1);
            }
        }

        public async Task<CommandResponse> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var errorOrData = await CheckStatus(request.ProviderId);

            if (errorOrData.Value is Error<ErrorReason>)
            {
                return errorOrData.AsT0;
            }

            var data = errorOrData.AsT1;

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

        private async Task<OneOf<Error<ErrorReason>, Data>> CheckStatus(Guid providerId)
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

            if (qaStatus == ApprenticeshipQAStatus.NotStarted)
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

            return new Data()
            {
                Provider = provider,
                QAStatus = qaStatus,
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

        Task<Guid> IRestrictQAStatus<Command, CommandResponse>.GetProviderId(Command request) =>
            Task.FromResult(request.ProviderId);

        Task<Guid> IRestrictQAStatus<Query, QueryResponse>.GetProviderId(Query request) =>
            Task.FromResult(request.ProviderId);

        CommandResponse IRestrictQAStatus<Command, CommandResponse>.CreateErrorResponse() =>
            new Error<ErrorReason>(ErrorReason.NoValidSubmission);

        QueryResponse IRestrictQAStatus<Query, QueryResponse>.CreateErrorResponse() =>
            new Error<ErrorReason>(ErrorReason.NoValidSubmission);

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
                    .When(c => c.CompliancePassed == false)
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
                    .When(c => c.StylePassed == false)
                    .WithMessageForAllRules("Enter comments for the reason selected");
            }
        }
    }
}
