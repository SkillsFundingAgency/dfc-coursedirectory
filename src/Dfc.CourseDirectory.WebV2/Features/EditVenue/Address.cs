using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.VenueValidation;
using FluentValidation;
using FluentValidation.Results;
using FormFlow;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.EditVenue.Address
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
        private readonly FormFlowInstance<EditVenueFlowModel> _formFlowInstance;
        private readonly ISearchClient<Onspd> _onspdSearchClient;

        public Handler(
            FormFlowInstance<EditVenueFlowModel> formFlowInstance,
            ISearchClient<Onspd> onspdSearchClient)
        {
            _formFlowInstance = formFlowInstance;
            _onspdSearchClient = onspdSearchClient;
        }

        public Task<Command> Handle(Query request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Command()
            {
                VenueId = request.VenueId,
                AddressLine1 = _formFlowInstance.State.AddressLine1,
                AddressLine2 = _formFlowInstance.State.AddressLine2,
                Town = _formFlowInstance.State.Town,
                County = _formFlowInstance.State.County,
                Postcode = _formFlowInstance.State.Postcode
            });
        }

        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            var onspdSearchResults = await _onspdSearchClient.Search(new OnspdSearchQuery() { Postcode = request.Postcode });

            if (onspdSearchResults.Results.Count == 0)
            {
                validationResult = new ValidationResult(new[]
                {
                    new ValidationFailure(nameof(request.Postcode), "Enter a valid postcode")
                });

                return new ModelWithErrors<Command>(request, validationResult);
            }

            var onspdPostcodeRecord = onspdSearchResults.Results.Single();

            _formFlowInstance.UpdateState(state =>
            {
                state.AddressLine1 = request.AddressLine1;
                state.AddressLine2 = request.AddressLine2;
                state.Town = request.Town;
                state.County = request.County;
                state.Postcode = request.Postcode;
                state.Latitude = onspdPostcodeRecord.lat;
                state.Longitude = onspdPostcodeRecord.@long;
                state.NewAddressIsOutsideOfEngland = onspdPostcodeRecord.Country != "England";
            });

            return new Success();
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.AddressLine1).AddressLine1();
                RuleFor(c => c.AddressLine2).AddressLine2();
                RuleFor(c => c.Town).Town();
                RuleFor(c => c.County).County();
                RuleFor(c => c.Postcode).Postcode();
            }
        }
    }
}
