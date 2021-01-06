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
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation.Results;
using FormFlow;
using MediatR;
using OneOf;
using OneOf.Types;
using DeleteTLevelQuery = Dfc.CourseDirectory.Core.DataStore.Sql.Queries.DeleteTLevel;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.DeleteTLevel
{
    [JourneyState]
    public class JourneyModel
    {
        public string TLevelName { get; set; }
        public Guid ProviderId { get; set; }
        public string YourReference { get; set; }
    }

    public class Request : IRequest<ViewModel>
    {
        public Guid TLevelId { get; set; }
    }

    public class ViewModel : Command
    {
        public string TLevelName { get; set; }
        public string YourReference { get; set; }
        public DateTime StartDate { get; set; }
        public IReadOnlyCollection<string> LocationVenueNames { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public Guid TLevelId { get; set; }
        public bool Confirm { get; set; }
    }

    public class DeletedQuery : IRequest<DeletedViewModel>
    {
    }

    public class DeletedViewModel
    {
        public Guid ProviderId { get; set; }
        public string TLevelName { get; set; }
        public bool HasOtherTLevels { get; set; }
        public string YourReference { get; set; }
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

            var tLevel = await GetTLevel(request.TLevelId);
            return CreateViewModel(tLevel);
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            _journeyInstance.ThrowIfCompleted();

            var tLevel = await GetTLevel(request.TLevelId);

            if (!request.Confirm)
            {
                var vm = CreateViewModel(tLevel);
                var validationResult = new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.Confirm), "Confirm you want to delete the T Level")
                });
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            await _sqlQueryDispatcher.ExecuteQuery(new DeleteTLevelQuery()
            {
                TLevelId = request.TLevelId,
                DeletedBy = _currentUserProvider.GetCurrentUser(),
                DeletedOn = _clock.UtcNow
            });

            _providerOwnershipCache.OnTLevelDeleted(request.TLevelId);

            // The next page needs this info - stash it in the JourneyModel
            // since it will no longer able to query for it.
            _journeyInstance.UpdateState(new JourneyModel()
            {
                TLevelName = tLevel.TLevelDefinition.Name,
                ProviderId = tLevel.ProviderId,
                YourReference = tLevel.YourReference
            });

            _journeyInstance.Complete();

            return new Success();
        }

        public async Task<DeletedViewModel> Handle(DeletedQuery request, CancellationToken cancellationToken)
        {
            _journeyInstance.ThrowIfNotCompleted();

            var providerId = _journeyInstance.State.ProviderId;

            var liveTLevels = await _sqlQueryDispatcher.ExecuteQuery(new GetTLevelsForProvider()
            {
                ProviderId = providerId
            });

            return new DeletedViewModel()
            {
                TLevelName = _journeyInstance.State.TLevelName,
                HasOtherTLevels = liveTLevels.Count > 0,
                ProviderId = providerId,
                YourReference = _journeyInstance.State.YourReference
            };
        }

        private ViewModel CreateViewModel(TLevel tLevel) => new ViewModel()
        {
            TLevelId = tLevel.TLevelId,
            LocationVenueNames = tLevel.Locations.Select(l => l.VenueName).ToArray(),
            StartDate = tLevel.StartDate,
            TLevelName = tLevel.TLevelDefinition.Name,
            YourReference = tLevel.YourReference
        };

        private async Task<TLevel> GetTLevel(Guid tLevelId)
        {
            return await _sqlQueryDispatcher.ExecuteQuery(new GetTLevel() { TLevelId = tLevelId }) ??
                throw new ResourceDoesNotExistException(ResourceType.TLevel, tLevelId);
        }
    }
}
