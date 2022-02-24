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
using FluentValidation.Results;
using SqlQueries = Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using DeleteApprenticeshipQuery = Dfc.CourseDirectory.Core.DataStore.Sql.Queries.DeleteApprenticeship;

//using Dfc.CourseDirectory.Services.Models;

namespace Dfc.CourseDirectory.WebV2.Features.Apprenticeships.DeleteApprenticeship
{
    [JourneyState]
    public class JourneyModel
    {
        public string ApprenticeshipTitle { get; set; }
        public Guid ApprenticeshipId { get; set; }
        public Guid ProviderId { get; set; }
        public string NotionalNVQLevelv2 { get; set; }



    }
    public class Request : IRequest<ViewModel>
    {
        public Guid ApprenticeshipId { get; set; }
    }



    public class ViewModel : Command
    {
        public string ApprenticeshipTitle { get; set; }
        public int Level { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public int? StandardCode { get; set; }
        public int? Version { get; set; }

    }
    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public Guid ApprenticeshipId { get; set; }
        public Guid ProviderId { get; set; }
        public bool Confirm { get; set; }

    }


    public class DeletedQuery : IRequest<DeletedViewModel>
    {
    }

    public class DeletedViewModel
    {
        public string ApprenticeshipTitle { get; set; }
        public int Level { get; set; }
    }


    public class Handler :
       IRequestHandler<Request, ViewModel>,
       IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>,
       IRequestHandler<DeletedQuery, DeletedViewModel>
    {
        private readonly IProviderOwnershipCache _providerOwnershipCache;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly JourneyInstance<JourneyModel> _journeyInstance;
        private readonly IClock _clock;
        private readonly ICurrentUserProvider _currentUserProvider;
        
        public Handler(
            IProviderOwnershipCache providerOwnershipCache,
            ISqlQueryDispatcher sqlQueryDispatcher,
            JourneyInstance<JourneyModel> journeyInstance,
            IClock clock,
            ICurrentUserProvider currentUserProvider)
        {
            _providerOwnershipCache = providerOwnershipCache;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _journeyInstance = journeyInstance;
            _clock = clock;
            _currentUserProvider = currentUserProvider;
            
        }

        public async Task<ViewModel> Handle(Request request, CancellationToken cancellationToken)
        {
           _journeyInstance.ThrowIfCompleted();

            var apprenticeship = await GetApprenticeship(request.ApprenticeshipId);
            return CreateViewModel(apprenticeship);
        }


        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            _journeyInstance.ThrowIfCompleted();

            var apprenticeship = await GetApprenticeship(request.ApprenticeshipId);

            if (!request.Confirm)
            {
                var vm = CreateViewModel(apprenticeship);
                var validationResult = new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.Confirm), "Confirm you want to delete the Apprenticeship")
                });
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            await _sqlQueryDispatcher.ExecuteQuery(new DeleteApprenticeshipQuery()
            {
                ApprenticeshipId = request.ApprenticeshipId,
                DeletedBy = _currentUserProvider.GetCurrentUser(),
                DeletedOn = _clock.UtcNow
            });

            _providerOwnershipCache.OnApprenticeshipDeleted(request.ApprenticeshipId);

            // The next page needs this info - stash it in the JourneyModel
            // since it will no longer able to query for it.
            _journeyInstance.UpdateState(new JourneyModel()
            {
                ApprenticeshipId = apprenticeship.ApprenticeshipId,
                ProviderId = apprenticeship.ProviderId,
                ApprenticeshipTitle = apprenticeship.Standard.StandardName,
                NotionalNVQLevelv2 = apprenticeship.Standard.NotionalNVQLevelv2
            });

            _journeyInstance.Complete();

            return new Success();
        }

        public async Task<DeletedViewModel> Handle(DeletedQuery request, CancellationToken cancellationToken)
        {
            _journeyInstance.ThrowIfNotCompleted();

            var providerId = _journeyInstance.State.ProviderId;

            var liveApprenticeships = await _sqlQueryDispatcher.ExecuteQuery(new GetApprenticeshipsForProvider()
            {
                ProviderId = providerId
            });

            return new DeletedViewModel()
            {
                ApprenticeshipTitle = _journeyInstance.State.ApprenticeshipTitle,

            };
        }

        private ViewModel CreateViewModel(Apprenticeship apprenticeship) => new ViewModel()
        {
            ApprenticeshipId = apprenticeship.ApprenticeshipId,
            ApprenticeshipTitle = apprenticeship.Standard.StandardName,
            NotionalNVQLevelv2 = apprenticeship.Standard.NotionalNVQLevelv2
        };
        private async Task<Apprenticeship> GetApprenticeship(Guid apprenticeshipId)
        {
            return await _sqlQueryDispatcher.ExecuteQuery(new GetApprenticeship() { ApprenticeshipId = apprenticeshipId }) ??
                throw new ResourceDoesNotExistException(ResourceType.Apprenticeship, apprenticeshipId);
        }


    }

}




     
