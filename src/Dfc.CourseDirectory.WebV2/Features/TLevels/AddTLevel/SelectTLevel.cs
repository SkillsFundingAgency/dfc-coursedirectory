using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation;
using FormFlow;
using Mapster;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.AddTLevel.SelectTLevel
{
    public class Query : IRequest<ViewModel>
    {
        public Guid ProviderId { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public Guid ProviderId { get; set; }
        public Guid? SelectedTLevelDefinitionId { get; set; }
    }

    public class ViewModel : Command
    {
        public IReadOnlyCollection<ViewModelTLevel> TLevels { get; set; }
    }

    public class ViewModelTLevel
    {
        public Guid TLevelDefinitionId { get; set; }
        public string Name { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        private readonly JourneyInstance<AddTLevelJourneyModel> _journeyInstance;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(
            JourneyInstance<AddTLevelJourneyModel> flowModel,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _journeyInstance = flowModel;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            var authorizedTLevelDefinitions = await GetAllTLevelDefinitions();

            return CreateViewModel(request.ProviderId, authorizedTLevelDefinitions);
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            var authorizedTLevelDefinitions = await GetAllTLevelDefinitions();

            var validator = new CommandValidator(authorizedTLevelDefinitions);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = CreateViewModel(request.ProviderId, authorizedTLevelDefinitions);
                request.Adapt(vm);

                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            if (request.SelectedTLevelDefinitionId != _journeyInstance.State.TLevelDefinitionId)
            {
                var tLevelDefinition = authorizedTLevelDefinitions
                    .Single(tld => tld.TLevelDefinitionId == request.SelectedTLevelDefinitionId);

                var exemplarContent = await _sqlQueryDispatcher.ExecuteQuery(
                    new GetTLevelDefinitionExemplarContent()
                    {
                        TLevelDefinitionId = request.SelectedTLevelDefinitionId.Value
                    });

                _journeyInstance.UpdateState(state => state.SetTLevel(
                    tLevelDefinition.TLevelDefinitionId,
                    tLevelDefinition.Name,
                    exemplarContent));
            }

            return new Success();
        }

        private ViewModel CreateViewModel(
            Guid providerId,
            IReadOnlyCollection<TLevelDefinition> authorizedTLevelDefinitions)
        {
            return new ViewModel()
            {
                ProviderId = providerId,
                SelectedTLevelDefinitionId = _journeyInstance.State.TLevelDefinitionId,
                TLevels = authorizedTLevelDefinitions.Select(tl => new ViewModelTLevel()
                {
                    Name = tl.Name,
                    TLevelDefinitionId = tl.TLevelDefinitionId
                }).ToList()
            };
        }

        private Task<IReadOnlyCollection<TLevelDefinition>> GetAllTLevelDefinitions() =>
            _sqlQueryDispatcher.ExecuteQuery(
                new GetAllTLevelDefinitions()
                {
                });

        private void ThrowIfFlowStateNotValid()
        {
            _journeyInstance.ThrowIfCompleted();
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator(IReadOnlyCollection<TLevelDefinition> tLevelDefinitions)
        {
            RuleFor(c => c.SelectedTLevelDefinitionId)
                .Must(value => value.HasValue && tLevelDefinitions.Any(tld => tld.TLevelDefinitionId == value))
                .WithMessageForAllRules("Select the T Level qualification to publish to the course directory");
        }
    }
}
