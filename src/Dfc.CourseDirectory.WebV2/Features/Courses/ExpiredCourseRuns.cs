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
using MediatR;
using Dfc.CourseDirectory.Core.Validation;

using Dfc.CourseDirectory.Core.Validation.CourseValidation;

using OneOf;
using OneOf.Types;
using FluentValidation;

namespace Dfc.CourseDirectory.WebV2.Features.Courses.ExpiredCourseRuns
{
    public class Query : IRequest<ViewModel>
    {
    }

    public class SelectedQuery : IRequest<ViewModel>
    {
        public Guid[] CheckedRows { get; set; }
    }

    public class NewStartDateQuery : ViewModel
    {
        public Guid[]  SelectedCourses { get; set; }
        public DateInput NewStartDate { get; set; }

    }

    public class ViewModel : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public int Total { get; set; }
        public IReadOnlyCollection<ViewModelRow> Rows { get; set; }
        public bool Checked { get; set; }

        public DateTime? NewStartDate { get; set; }

        public Guid[] SelectedCourses { get; set; }

        public Guid[] CheckedRows { get; set; }

    }

    public class ViewModelRow  : ViewModel
    {
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public string ProviderCourseRef { get; set; }
        public CourseDeliveryMode DeliveryMode { get; set; }
        public IReadOnlyCollection<string> SubRegionNames { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsChecked { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
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
            public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var results = await _sqlQueryDispatcher.ExecuteQuery(new GetExpiredCourseRunsForProvider()
            {
                ProviderId = _providerContextProvider.GetProviderId(),
                Today = _clock.UtcNow.Date
            });

            var allRegions = await _regionCache.GetAllRegions();
            var allSubRegions = allRegions.SelectMany(r => r.SubRegions).ToDictionary(sr => sr.Id, sr => sr);

            return new ViewModel()
            {
                Rows = results
                    .Select(r => new ViewModelRow()
                    {
                        CourseId = r.CourseId,
                        CourseName = r.CourseName,
                        ProviderCourseRef = r.ProviderCourseId,
                        DeliveryMode = r.DeliveryMode,
                        SubRegionNames = r.SubRegionIds.Select(id => allSubRegions[id].Name).ToArray(),
                        StartDate = r.StartDate,
                        IsChecked = false,
                    })
                    .ToArray(),
                Total = results.Count,
            };
        }
    }
    public class SelectedHandler : IRequestHandler<SelectedQuery, ViewModel>
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IClock _clock;
        private readonly IRegionCache _regionCache;

        public SelectedHandler(
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
        public async Task<ViewModel> Handle(SelectedQuery request, CancellationToken cancellationToken)
        {
            var results = await _sqlQueryDispatcher.ExecuteQuery(new GetExpiredSelectedCourseRunsForProvider()
            {
                ProviderId = _providerContextProvider.GetProviderId(),
                Today = _clock.UtcNow.Date,
                SelectedCourseRuns = request.CheckedRows
            }) ;

            var allRegions = await _regionCache.GetAllRegions();
            var allSubRegions = allRegions.SelectMany(r => r.SubRegions).ToDictionary(sr => sr.Id, sr => sr);

            return new ViewModel()
            {
                Rows = results
                    .Select(r => new ViewModelRow()
                    {
                        CourseId = r.CourseId,
                        CourseName = r.CourseName,
                        ProviderCourseRef = r.ProviderCourseId,
                        DeliveryMode = r.DeliveryMode,
                        SubRegionNames = r.SubRegionIds.Select(id => allSubRegions[id].Name).ToArray(),
                        StartDate = r.StartDate,
                        IsChecked = false,
                       
                    })
                    .ToArray(),
                Total = results.Count,

            };
        }
    }

    public class UpdatedHandler : IRequestHandler<NewStartDateQuery, OneOf<ModelWithErrors<ViewModel>, Success>>


    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly IClock _clock;
        private readonly IRegionCache _regionCache;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        public UpdatedHandler(ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderContextProvider providerContextProvider, IClock clock, IRegionCache regionCache)
        {
            _providerContextProvider = providerContextProvider;
            _clock = clock;
            _regionCache = regionCache;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(NewStartDateQuery request, CancellationToken cancellationToken)
        {

            var validator = new ExpiredCourseRunsValidator(_clock);
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsValid)
            {
                await _sqlQueryDispatcher.ExecuteQuery(new CourseStarteDateBulkUpdate()
                {
                    ProviderId = _providerContextProvider.GetProviderId(),
                    StartDate = request.NewStartDate.ToDateTime(),
                    SelectedCourseRunid = request.SelectedCourses

                });

                return new Success();
            }
            else
            {
                var selectedHandler = new SelectedHandler(_providerContextProvider, _sqlQueryDispatcher, _clock, _regionCache);
                var vm = await selectedHandler.Handle(new SelectedQuery()
                {
                    CheckedRows = request.SelectedCourses
                }, cancellationToken);

                vm.NewStartDate = request.NewStartDate.ToDateTime();
                
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

        }


        public class ExpiredCourseRunsValidator : AbstractValidator<NewStartDateQuery>
        {
            public ExpiredCourseRunsValidator(IClock clock)
            {
                RuleFor(c => c.NewStartDate).StartDate(now: clock.UtcNow, getFlexibleStartDate: c => false);
                RuleFor(c => c.SelectedCourses).NotEmpty().WithMessage("No selected row");

            }
        }
    }
}
