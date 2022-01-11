using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.CourseValidation;
using FluentValidation;
using Mapster;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.ChooseQualification.CourseRun
{
    public class Query : IRequest<ModelWithErrors<ViewModel>>
    {
    }

    public class ViewModel : Command
    {
    }

    public class Command : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public CourseDeliveryMode DeliveryMode { get; set; }
        public int RowNumber { get; set; }
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



    public class Handler :
        IRequestHandler<Query, ModelWithErrors<ViewModel>>,
        IRequestHandler<Command, OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IClock _clock;
        private readonly IRegionCache _regionCache;

        public Handler(
            IProviderContextProvider providerContextProvider,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IClock clock,
            IRegionCache regionCache)
        {
            _providerContextProvider = providerContextProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _clock = clock;
            _regionCache = regionCache;
        }

        public async Task<ModelWithErrors<ViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var vm = new ViewModel();
            var allRegions = await _regionCache.GetAllRegions();

            var validator = new CommandValidator(_clock, allRegions);
            var validationResult = await validator.ValidateAsync(vm);

            return new ModelWithErrors<ViewModel>(vm, validationResult);


        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var allRegions = await _regionCache.GetAllRegions();
            var validator = new CommandValidator(_clock, allRegions);
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.IsValid)
                return new Success();

            else
            {
                var vm = new ViewModel();
                request.Adapt(vm);
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }
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
