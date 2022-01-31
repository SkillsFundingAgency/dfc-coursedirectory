using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.CourseValidation;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using FluentValidation;
using Mapster;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.ChooseQualification.CourseRun
{
    public class Query : IRequest<ViewModel>
    {
        public CourseDeliveryMode DeliveryMode { get; set; }
        public Guid ProviderId { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public CourseDeliveryMode DeliveryMode { get; set; }
        public string CourseName { get; set; }
        public string ProviderCourseRef { get; set; }
        public DateInput StartDate { get; set; }
        public bool? FlexibleStartDate { get; set; }
        public bool? NationalDelivery { get; set; }
        public IEnumerable<string> SubRegionIds { get; set; }
        public string CourseWebPage { get; set; }
        public string Cost { get; set; }
        public string CostDescription { get; set; }
        public int? Duration { get; set; }
        public CourseDurationUnit? DurationUnit { get; set; }
        public CourseStudyMode? StudyMode { get; set; }
        public CourseAttendancePattern? AttendancePattern { get; set; }
        public Guid? VenueId { get; set; }
    }

    public class ViewModel : Command
    {
        public IReadOnlyCollection<ViewModelProviderVenuesItem> ProviderVenues { get; set; }
    }

    public class ViewModelProviderVenuesItem
    {
        public Guid VenueId { get; set; }
        public string VenueName { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IClock _clock;
        private readonly IRegionCache _regionCache;
        private readonly MptxInstanceContext<FlowModel> _flow;

        public Handler(
            IProviderContextProvider providerContextProvider,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IClock clock,
            IRegionCache regionCache,
            MptxInstanceContext<FlowModel> flow)
        {
            _providerContextProvider = providerContextProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _clock = clock;
            _regionCache = regionCache;
            _flow = flow;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            if (_flow.State.LarsCode == null || string.IsNullOrEmpty(_flow.State.WhoThisCourseIsFor) || !_flow.State.DeliveryMode.HasValue)
            {
                throw new InvalidStateException();
            }

            var command = new Command() { CourseName = _flow.State.CourseName };
            var vm = await CreateViewModel(request.DeliveryMode, command);
            NormalizeViewModel();
            return await Task.FromResult(vm);

            void NormalizeViewModel()
            {
                if (request.DeliveryMode != CourseDeliveryMode.ClassroomBased)
                {
                    vm.VenueId = null;
                    vm.StudyMode = null;
                    vm.AttendancePattern = null;
                }

                if (request.DeliveryMode != CourseDeliveryMode.WorkBased)
                {
                    vm.NationalDelivery = null;
                    vm.SubRegionIds = null;
                }
            }
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            NormalizeCommand();
            var allRegions = await _regionCache.GetAllRegions();
            var validator = new CommandValidator(_clock, allRegions);
            var validationResult = await validator.ValidateAsync(request);
            if (validationResult.IsValid)
            {
                _flow.Update(s => s.SetCourseRun(request.CourseName,
                    request.ProviderCourseRef,
                    request.StartDate,
                    request.FlexibleStartDate,
                    request.NationalDelivery,
                    request.SubRegionIds,
                    request.CourseWebPage,
                    request.Cost,
                    request.CostDescription,
                    request.Duration,
                    request.DurationUnit,
                    request.StudyMode,
                    request.AttendancePattern,
                    request.VenueId));
                return new Success();
            }
            else
            {
                var vm = await CreateViewModel(request.DeliveryMode, request);
                request.Adapt(vm);
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }


            void NormalizeCommand()
            {
                // Some fields only apply under certain conditions; ensure we don't save fields that are not applicable

                if (request.DeliveryMode != CourseDeliveryMode.ClassroomBased)
                {
                    request.VenueId = null;
                    request.StudyMode = null;
                    request.AttendancePattern = null;
                }

                if (request.DeliveryMode != CourseDeliveryMode.WorkBased)
                {
                    request.NationalDelivery = null;
                    request.SubRegionIds = null;
                }

                if (request.FlexibleStartDate == true)
                {
                    request.StartDate = null;
                }

                if (request.NationalDelivery == true)
                {
                    request.SubRegionIds = null;
                }
            }
        }

        private async Task<ViewModel> CreateViewModel(CourseDeliveryMode deliveryMode, Command row)
        {
            var providerVenues = deliveryMode == CourseDeliveryMode.ClassroomBased ?
                (await _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = _providerContextProvider.GetProviderId() }))
                    .Select(v => new ViewModelProviderVenuesItem()
                    {
                        VenueId = v.VenueId,
                        VenueName = v.VenueName
                    })
                    .OrderBy(v => v.VenueName)
                    .ToArray() :
                null;

            return new ViewModel()
            {
                DeliveryMode = deliveryMode,
                CourseName = row.CourseName,
                ProviderCourseRef = row.ProviderCourseRef,
                StartDate = row.StartDate,
                FlexibleStartDate = row.FlexibleStartDate,
                NationalDelivery = row.NationalDelivery,
                SubRegionIds = row.SubRegionIds,
                CourseWebPage = row.CourseWebPage,
                Cost = row.Cost,
                CostDescription = row.CostDescription,
                Duration = row.Duration,
                DurationUnit = row.DurationUnit,
                StudyMode = row.StudyMode,
                AttendancePattern = row.AttendancePattern,
                VenueId = row.VenueId,
                ProviderVenues = providerVenues
            };
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator(IClock clock, IReadOnlyCollection<Region> allRegions)
            {
                {
                    RuleFor(c => c.CourseName).CourseName();
                    RuleFor(c => c.ProviderCourseRef).ProviderCourseRef();
                    RuleFor(c => c.StartDate).StartDate(now: clock.UtcNow, getFlexibleStartDate: c => c.FlexibleStartDate);
                    RuleFor(c => c.FlexibleStartDate).FlexibleStartDate();
                    RuleFor(c => c.NationalDelivery).NationalDelivery(getDeliveryMode: c => c.DeliveryMode);
                    RuleFor(c => c.CourseWebPage).CourseWebPage();
                    RuleFor(c => c.Cost)
                        .Transform(v => decimal.TryParse(v, out var parsed) ? parsed : (decimal?)null)
                        .Cost(costWasSpecified: c => !string.IsNullOrWhiteSpace(c.Cost), getCostDescription: c => c.CostDescription);
                    RuleFor(c => c.CostDescription).CostDescription();
                    RuleFor(c => c.Duration).Duration();
                    RuleFor(c => c.DurationUnit).DurationUnit();
                    RuleFor(c => c.VenueId).VenueId(getDeliveryMode: c => c.DeliveryMode);

                    RuleFor(c => c.StudyMode).StudyMode(
                        studyModeWasSpecified: c => c.StudyMode.HasValue,
                        getDeliveryMode: c => c.DeliveryMode);

                    RuleFor(c => c.AttendancePattern).AttendancePattern(
                        attendancePatternWasSpecified: c => c.AttendancePattern.HasValue,
                        getDeliveryMode: c => c.DeliveryMode);

                    RuleFor(c => c.SubRegionIds)
                        .Transform(ids =>
                        {
                            return allRegions
                                .SelectMany(r => r.SubRegions)
                                .Join(ids ?? Array.Empty<string>(), sr => sr.Id, id => id, (sr, id) => sr)
                                .ToArray();
                        })
                        .SubRegions(
                            subRegionsWereSpecified: c => c.SubRegionIds?.Count() > 0,
                            getDeliveryMode: c => c.DeliveryMode,
                            getNationalDelivery: c => c.NationalDelivery);

                }
            }
        }
    }
}
