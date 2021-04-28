using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.VenueValidation;
using FluentValidation;
using FluentValidation.Results;
using FormFlow;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.EditVenue.Address
{
    public class Query : IRequest<Command>
    {
        public Guid VenueId { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Command>, Success>>
    {
        public Guid VenueId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, Command>,
        IRequestHandler<Command, OneOf<ModelWithErrors<Command>, Success>>
    {
        private readonly JourneyInstance<EditVenueJourneyModel> _journeyInstance;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(
            JourneyInstance<EditVenueJourneyModel> journeyInstance,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _journeyInstance = journeyInstance;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public Task<Command> Handle(Query request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Command()
            {
                VenueId = request.VenueId,
                AddressLine1 = _journeyInstance.State.AddressLine1,
                AddressLine2 = _journeyInstance.State.AddressLine2,
                Town = _journeyInstance.State.Town,
                County = _journeyInstance.State.County,
                Postcode = _journeyInstance.State.Postcode
            });
        }

        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var postcodeInfo = await _sqlQueryDispatcher.ExecuteQuery(new GetPostcodeInfo() { Postcode = request.Postcode });

            var validator = new CommandValidator(postcodeInfo);
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            _journeyInstance.UpdateState(state =>
            {
                state.AddressLine1 = request.AddressLine1;
                state.AddressLine2 = request.AddressLine2;
                state.Town = request.Town;
                state.County = request.County;
                state.Postcode = request.Postcode;
                state.Latitude = postcodeInfo.Latitude;
                state.Longitude = postcodeInfo.Longitude;
                state.NewAddressIsOutsideOfEngland = !postcodeInfo.InEngland;
            });

            return new Success();
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(PostcodeInfo postcodeInfo)
            {
                RuleFor(c => c.AddressLine1).AddressLine1();
                RuleFor(c => c.AddressLine2).AddressLine2();
                RuleFor(c => c.Town).Town();
                RuleFor(c => c.County).County();
                RuleFor(c => c.Postcode).Postcode(postcodeInfo);
            }
        }
    }
}
