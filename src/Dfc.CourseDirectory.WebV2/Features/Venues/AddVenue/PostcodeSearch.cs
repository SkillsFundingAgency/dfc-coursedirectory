using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.AddressSearch;
using FluentValidation;
using FluentValidation.Results;
using FormFlow;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Venues.AddVenue.PostcodeSearch
{
    public class SearchCommand : IRequest<OneOf<ModelWithErrors<SearchCommand>, SearchViewModel>>
    {
        public string Postcode { get; set; }
    }

    public class SearchViewModel : SearchCommand
    {
        public string AddressId { get; set; }
        public IReadOnlyCollection<SearchResult> Results { get; set; }
    }

    public class SearchResult
    {
        public string AddressId { get; set; }
        public string StreetAddress { get; set; }
    }

    public class SelectCommand : IRequest<OneOf<ModelWithErrors<SearchViewModel>, Success>>
    {
        public string AddressId { get; set; }
    }

    public class Handler :
        IRequestHandler<SearchCommand, OneOf<ModelWithErrors<SearchCommand>, SearchViewModel>>,
        IRequestHandler<SelectCommand, OneOf<ModelWithErrors<SearchViewModel>, Success>>
    {
        private readonly IAddressSearchService _addressSearchService;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly JourneyInstanceProvider _journeyInstanceProvider;

        public Handler(
            IAddressSearchService addressSearchService,
            ISqlQueryDispatcher sqlQueryDispatcher,
            JourneyInstanceProvider journeyInstanceProvider)
        {
            _addressSearchService = addressSearchService;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _journeyInstanceProvider = journeyInstanceProvider;
        }

        public async Task<OneOf<ModelWithErrors<SearchCommand>, SearchViewModel>> Handle(
            SearchCommand request,
            CancellationToken cancellationToken)
        {
            var journeyInstance = _journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();

            journeyInstance.ThrowIfCompleted();

            var validator = new SearchCommandValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<SearchCommand>(request, validationResult);
            }

            var postcode = new Postcode(request.Postcode);

            // We need the postcode to exist in ONSPD so we can resolve a lat/lng.
            // Check it's present before we do an address search so we can output an error early.

            var postcodeInfo = await _sqlQueryDispatcher.ExecuteQuery(new GetPostcodeInfo() { Postcode = postcode });

            if (postcodeInfo == null)
            {
                return CreateInvalidPostcodeResponse();
            }

            var results = await _addressSearchService.SearchByPostcode(postcode);
            var vm = CreateViewModel(postcode, results);

            // Stash the results in journey state so we can re-use them in the SelectCommand's handler
            journeyInstance.UpdateState(state =>
            {
                state.PostcodeSearchResults = results;
                state.PostcodeSearchQuery = request.Postcode;
            });

            if (vm.Results.Count == 0)
            {
                return CreateInvalidPostcodeResponse();
            }

            return vm;

            ModelWithErrors<SearchCommand> CreateInvalidPostcodeResponse() =>
                new ModelWithErrors<SearchCommand>(
                    request,
                    new ValidationResult(new[]
                    {
                        new ValidationFailure(nameof(SearchCommand.Postcode), ErrorRegistry.All["VENUE_POSTCODE_FORMAT"].GetMessage())
                    }));
        }

        public async Task<OneOf<ModelWithErrors<SearchViewModel>, Success>> Handle(
            SelectCommand request,
            CancellationToken cancellationToken)
        {
            var journeyInstance = _journeyInstanceProvider.GetInstance<AddVenueJourneyModel>();

            journeyInstance.ThrowIfCompleted();

            // We should have cached search results at this point from the postcode search above
            if ((journeyInstance.State.PostcodeSearchResults?.Count ?? 0) == 0)
            {
                throw new InvalidStateException();
            }

            var validator = new SelectCommandValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                var vm = CreateViewModel(
                    journeyInstance.State.PostcodeSearchQuery,
                    journeyInstance.State.PostcodeSearchResults);

                return new ModelWithErrors<SearchViewModel>(vm, validationResult);
            }

            var addressDetail = await _addressSearchService.GetById(request.AddressId);

            if (addressDetail == null)
            {
                // Address specified isn't valid. (We should never get here.)
                throw new InvalidStateException();
            }

            // SearchCommand has already checked that there is an ONSPD entry for this postcode
            // so there is no valid case where `postcodeInfo` should blow up
            var postcodeInfo = await _sqlQueryDispatcher.ExecuteQuery(new GetPostcodeInfo() { Postcode = addressDetail.Postcode });

            journeyInstance.UpdateState(state =>
            {
                state.AddressLine1 = addressDetail.Line1;
                state.AddressLine2 = addressDetail.Line2;
                state.Town = addressDetail.PostTown;
                state.County = addressDetail.County;
                state.Postcode = addressDetail.Postcode;
                state.Latitude = postcodeInfo.Latitude;
                state.Longitude = postcodeInfo.Longitude;
                state.AddressIsOutsideOfEngland = !postcodeInfo.InEngland;
                state.ValidStages |= AddVenueCompletedStages.Address;
            });

            return new Success();
        }

        private static SearchViewModel CreateViewModel(string postcode, IReadOnlyCollection<PostcodeSearchResult> results) =>
            new SearchViewModel()
            {
                Postcode = new Postcode(postcode),
                Results = results
                    .Select(r => new SearchResult()
                    {
                        AddressId = r.Id,
                        StreetAddress = r.StreetAddress
                    })
                    .ToArray()
            };

        private class SearchCommandValidator : AbstractValidator<SearchCommand>
        {
            public SearchCommandValidator()
            {
                RuleFor(c => c.Postcode)
                    .NotEmpty().WithMessage(ErrorRegistry.All["VENUE_POSTCODE_REQUIRED"].GetMessage())
                    .Apply(Rules.Postcode).WithMessage(ErrorRegistry.All["VENUE_POSTCODE_FORMAT"].GetMessage());
            }
        }

        private class SelectCommandValidator : AbstractValidator<SelectCommand>
        {
            public SelectCommandValidator()
            {
                RuleFor(c => c.AddressId)
                    .NotEmpty().WithMessage("Select an address");
            }
        }
    }
}
