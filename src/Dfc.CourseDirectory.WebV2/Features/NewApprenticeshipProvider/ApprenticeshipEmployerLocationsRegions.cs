using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider.ApprenticeshipEmployerLocationsRegions
{
    using CommandResponse = OneOf<ModelWithErrors<Command>, Success>;

    public class Query : IRequest<Command>
    {
        public Guid ProviderId { get; set; }
    }

    public class Command : IRequest<CommandResponse>
    {
        public Guid ProviderId { get; set; }
        public IReadOnlyCollection<string> RegionIds { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, Command>,
        IRequestHandler<Command, CommandResponse>
    {
        private readonly MptxInstanceContext<FlowModel> _flow;
        private readonly IRegionCache _regionCache;

        public Handler(MptxInstanceContext<FlowModel> flow, IRegionCache regionCache)
        {
            _flow = flow;
            _regionCache = regionCache;
        }

        public Task<Command> Handle(Query request, CancellationToken cancellationToken)
        {
            ValidateFlowState();

            var command = new Command()
            {
                ProviderId = request.ProviderId,
                RegionIds = _flow.State.ApprenticeshipLocationSubRegionIds ?? Array.Empty<string>()
            };
            return Task.FromResult(command);
        }

        public async Task<CommandResponse> Handle(Command request, CancellationToken cancellationToken)
        {
            ValidateFlowState();

            var allRegions = await _regionCache.GetAllRegions();

            var validator = new CommandValidator(allRegions);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                request.RegionIds ??= new List<string>();
                return new ModelWithErrors<Command>(request, validationResult);
            }

            _flow.Update(s => s.SetApprenticeshipLocationRegionIds(request.RegionIds));

            return new Success();
        }

        private void ValidateFlowState()
        {
            if ((_flow.State.ApprenticeshipLocationType != ApprenticeshipLocationType.EmployerBased &&
                _flow.State.ApprenticeshipLocationType != ApprenticeshipLocationType.ClassroomBasedAndEmployerBased) ||
                _flow.State.ApprenticeshipIsNational != false)
            {
                throw new InvalidStateException();
            }
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(IReadOnlyCollection<Region> allRegions)
            {
                RuleFor(c => c.RegionIds)
                    .Transform(v =>
                    {
                        if (v == null)
                        {
                            return v;
                        }

                        var regionIds = allRegions.Select(r => r.Id);
                        var subRegionIds = allRegions.SelectMany(r => r.SubRegions).Select(sr => sr.Id);
                        var allRegionIds = regionIds.Concat(subRegionIds).ToList();

                        // Remove any IDs that are not regions or sub-regions
                        return v.Intersect(allRegionIds).ToList();
                    })
                    .NotEmpty()
                    .WithMessage("Select at least one sub-region");
            }
        }
    }
}
