using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.TLevelValidation;
using FluentValidation;
using FormFlow;
using Mapster;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.TLevels.AddTLevel.Description
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public string WhoFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYoullLearn { get; set; }
        public string HowYoullLearn { get; set; }
        public string HowYoullBeAssessed { get; set; }
        public string WhatYouCanDoNext { get; set; }
    }

    public class ViewModel : Command
    {
        public string TLevelName { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        private readonly JourneyInstance<AddTLevelJourneyModel> _journeyInstance;

        public Handler(JourneyInstance<AddTLevelJourneyModel> journeyInstance)
        {
            _journeyInstance = journeyInstance;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            return Task.FromResult(CreateViewModel());
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            ThrowIfFlowStateNotValid();

            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = CreateViewModel();
                request.Adapt(vm);

                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            _journeyInstance.UpdateState(state => state.SetDescription(
                request.WhoFor,
                request.EntryRequirements,
                request.WhatYoullLearn,
                request.HowYoullLearn,
                request.HowYoullBeAssessed,
                request.WhatYouCanDoNext,
                isComplete: true));

            return new Success();
        }

        private ViewModel CreateViewModel() => new ViewModel()
        {
            EntryRequirements = _journeyInstance.State.EntryRequirements,
            HowYoullBeAssessed = _journeyInstance.State.HowYoullBeAssessed,
            HowYoullLearn = _journeyInstance.State.HowYoullLearn,
            TLevelName = _journeyInstance.State.TLevelName,
            WhatYouCanDoNext = _journeyInstance.State.WhatYouCanDoNext,
            WhatYoullLearn = _journeyInstance.State.WhatYoullLearn,
            WhoFor = _journeyInstance.State.WhoFor
        };

        private void ThrowIfFlowStateNotValid()
        {
            _journeyInstance.ThrowIfCompleted();

            if (!_journeyInstance.State.CompletedStages.HasFlag(AddTLevelJourneyCompletedStages.SelectTLevel))
            {
                throw new InvalidStateException();
            }
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(c => c.WhoFor).WhoFor();
            RuleFor(c => c.EntryRequirements).EntryRequirements();
            RuleFor(c => c.WhatYoullLearn).WhatYoullLearn();
            RuleFor(c => c.HowYoullLearn).HowYoullLearn();
            RuleFor(c => c.HowYoullBeAssessed).HowYoullBeAssessed();
            RuleFor(c => c.WhatYouCanDoNext).WhatYouCanDoNext();
        }
    }
}
