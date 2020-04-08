using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.Behaviors.Errors;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider.ApprenticeshipSummary
{
    public class Query : IRequest<ViewModel>, IProviderScopedRequest
    {
        public Guid ProviderId { get; set; }
    }

    public class ViewModel
    {
        public Guid ProviderId { get; set; }
        public string StandardOrFrameworkTitle { get; set; }
        public string MarketingInformation { get; set; }
        public string Website { get; set; }
        public string ContactTelephone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactWebsite { get; set; }
        public ApprenticeshipLocationType ApprenticeshipLocationType { get; set; }
        public bool? ApprenticeshipIsNational { get; set; }
    }

    public class CompleteCommand : IRequest<Success>, IProviderScopedRequest
    {
        public Guid ProviderId { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequireUserCanSubmitQASubmission<Query>,
        IRequestHandler<CompleteCommand, Success>,
        IRequireUserCanSubmitQASubmission<CompleteCommand>
    {
        private readonly MptxInstanceContext<FlowModel> _flow;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;
        private readonly IProviderInfoCache _providerInfoCache;

        public Handler(
            MptxInstanceContext<FlowModel> flow,
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock,
            IProviderInfoCache providerInfoCache)
        {
            _flow = flow;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
            _providerInfoCache = providerInfoCache;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            ValidateFlowState();

            var vm = new ViewModel()
            {
                ProviderId = request.ProviderId,
                StandardOrFrameworkTitle = _flow.State.ApprenticeshipStandardOrFramework.StandardOrFrameworkTitle,
                MarketingInformation = _flow.State.ApprenticeshipMarketingInformation,
                Website = _flow.State.ApprenticeshipWebsite,
                ContactTelephone = _flow.State.ApprenticeshipContactTelephone,
                ContactEmail = _flow.State.ApprenticeshipContactEmail,
                ContactWebsite = _flow.State.ApprenticeshipContactWebsite,
                ApprenticeshipLocationType = _flow.State.ApprenticeshipLocationType.Value,
                ApprenticeshipIsNational = _flow.State.ApprenticeshipIsNational
            };
            return Task.FromResult(vm);
        }

        public async Task<Success> Handle(CompleteCommand request, CancellationToken cancellationToken)
        {
            ValidateFlowState();

            var currentUser = _currentUserProvider.GetCurrentUser();
            var now = _clock.UtcNow;

            var apprenticeshipId = Guid.NewGuid();
            var providerId = request.ProviderId;
            var providerUkprn = (await _providerInfoCache.GetProviderInfo(providerId)).Ukprn;

            // Create apprenticeship
            await _cosmosDbQueryDispatcher.ExecuteQuery(
                new CreateApprenticeship()
                {
                    ApprenticeshipLocations = new[]
                    {
                        CreateApprenticeshipLocation.CreateNational()
                    },
                    ApprenticeshipTitle = _flow.State.ApprenticeshipStandardOrFramework.StandardOrFrameworkTitle,
                    ApprenticeshipType = _flow.State.ApprenticeshipStandardOrFramework.IsStandard ?
                        ApprenticeshipType.StandardCode :
                        ApprenticeshipType.FrameworkCode,
                    ContactEmail = _flow.State.ApprenticeshipContactEmail,
                    ContactTelephone = _flow.State.ApprenticeshipContactTelephone,
                    ContactWebsite = _flow.State.ApprenticeshipContactWebsite,
                    CreatedByUser = currentUser,
                    CreatedDate = _clock.UtcNow,
                    Id = apprenticeshipId,
                    MarketingInformation = _flow.State.ApprenticeshipMarketingInformation,
                    ProviderId = request.ProviderId,
                    ProviderUkprn = providerUkprn,
                    StandardOrFramework = _flow.State.ApprenticeshipStandardOrFramework,
                    Url = _flow.State.ApprenticeshipWebsite
                });

            // Create QA submission
            await _sqlQueryDispatcher.ExecuteQuery(
                new CreateApprenticeshipQASubmission()
                {
                    ProviderId = request.ProviderId,
                    Apprenticeships = new List<CreateApprenticeshipQASubmissionApprenticeship>()
                    {
                        new CreateApprenticeshipQASubmissionApprenticeship()
                        {
                            ApprenticeshipId = apprenticeshipId,
                            ApprenticeshipMarketingInformation = _flow.State.ApprenticeshipMarketingInformation,
                            ApprenticeshipTitle = _flow.State.ApprenticeshipStandardOrFramework.StandardOrFrameworkTitle
                        }
                    },
                    ProviderMarketingInformation = _flow.State.ProviderMarketingInformation,
                    SubmittedByUserId = currentUser.UserId,
                    SubmittedOn = now
                });

            await _sqlQueryDispatcher.ExecuteQuery(
                new SetProviderApprenticeshipQAStatus()
                {
                    ProviderId = request.ProviderId,
                    ApprenticeshipQAStatus = ApprenticeshipQAStatus.Submitted
                });

            // Ensure user cannot go 'back' to any part of this flow
            _flow.Complete();

            return new Success();
        }

        private void ValidateFlowState()
        {
            if (!_flow.State.IsValid)
            {
                throw new ErrorException<InvalidFlowState>(new InvalidFlowState());
            }
        }
    }
}
