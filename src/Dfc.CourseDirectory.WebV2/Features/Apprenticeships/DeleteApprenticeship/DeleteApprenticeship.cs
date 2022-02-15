using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues.Upload;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation;
using FormFlow;
using MediatR;
using OneOf;
using OneOf.Types;
using SqlQueries = Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
//using Dfc.CourseDirectory.Services.Models;

namespace Dfc.CourseDirectory.WebV2
{
    class JourneyModel
    {
        public string ApprenticeshipTitle { get; set; }

    }
    public class Query : IRequest<ViewModel>
    {
        public Guid VenueId { get; set; }
        public Guid ProviderId { get; set; }
        public Guid ApprenticeshipId { get; internal set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public Guid ApprenticeshipId { get; set; }
        public Guid ProviderId { get; set; }
        public bool Confirm { get; set; }

    }
    public class ViewModel : Command
    {
        // public string ProviderVenueRef { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public int Level { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public int? StandardCode { get; set; }
        public int? Version { get; set; }
        // public IReadOnlyCollection<string> AddressParts { get; set; }

    }

    public class DeletedQuery : IRequest<DeletedViewModel>
    {
    }

    public class DeletedViewModel
    {
        public string ApprenticeshipTitle { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>,
        IRequestHandler<DeletedQuery, DeletedViewModel>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderOwnershipCache _providerOwnershipCache;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;
        private readonly IClock _clock;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderOwnershipCache providerOwnershipCache,
            ICurrentUserProvider currentUserProvider,
            JourneyInstanceProvider journeyInstanceProvider,
            IClock clock)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerOwnershipCache = providerOwnershipCache;
            _currentUserProvider = currentUserProvider;
            _journeyInstanceProvider = journeyInstanceProvider;
            _clock = clock;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken) => CreateViewModel(request.ApprenticeshipId);

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var vm = await CreateViewModel(request.ApprenticeshipId);


            var result = await _sqlQueryDispatcher.ExecuteQuery(new SqlQueries.DeleteApprenticeship()
            {
                ApprenticeshipId = request.ApprenticeshipId,
                DeletedOn = _clock.UtcNow,
                DeletedBy = _currentUserProvider.GetCurrentUser()
            });

            if (result.Value is NotFound)
            {
                throw new ResourceDoesNotExistException(ResourceType.Apprenticeship, request.ApprenticeshipId);
            }

            _providerOwnershipCache.OnVenueDeleted(request.ApprenticeshipId);

            _journeyInstanceProvider.GetOrCreateInstance(() => new JourneyModel { ApprenticeshipTitle = vm.ApprenticeshipTitle })
                .Complete();

            return new Success();
        }

        public Task<DeletedViewModel> Handle(DeletedQuery request, CancellationToken cancellationToken)
        {
            var journeyInstance = _journeyInstanceProvider.GetInstance<JourneyModel>();
            journeyInstance.ThrowIfNotCompleted();

            return Task.FromResult(new DeletedViewModel
            {
                ApprenticeshipTitle = journeyInstance.State.ApprenticeshipTitle
            });
        }

        private async Task<ViewModel> CreateViewModel(Guid ApprenticeshipId)
        {
            var offeringInfo = await _sqlQueryDispatcher.ExecuteQuery(new GetApprenticeshipsForProvider() { ProviderId = ApprenticeshipId });

            if (offeringInfo == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Apprenticeship, ApprenticeshipId);
            }
             
            var providerId = offeringInfo.Apprenticeship.ProviderId; 

            return new ViewModel();

        }


    }

}



     
