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
        public Guid[] SelectedCourses { get; set; }
        public DateTime NewStartDate { get; set; }

    }

    public class ViewModel : IRequest<OneOf<ModelWithErrors<ViewModel>, Success>>
    {
        public int Total { get; set; }
        public IReadOnlyCollection<ViewModelRow> Rows { get; set; }
        public bool Checked { get; set; }
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
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        public UpdatedHandler(ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderContextProvider providerContextProvider)
        {
            _providerContextProvider = providerContextProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<OneOf<ModelWithErrors<ViewModel>, Success>> Handle(NewStartDateQuery request, CancellationToken cancellationToken)
        {

            var validator = new ExpiredCourseRunsValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsValid)
            {
                await _sqlQueryDispatcher.ExecuteQuery(new CourseStarteDateBulkUpdate()
                {
                    ProviderId = _providerContextProvider.GetProviderId(),
                    StartDate = request.NewStartDate,
                    SelectedCourseRunid = request.SelectedCourses

                });


                return new Success();
            }
            else
            {

                return new ModelWithErrors<ViewModel>(new ViewModel(), validationResult);
            }

        }


        public class ExpiredCourseRunsValidator : AbstractValidator<NewStartDateQuery>
        {
            public ExpiredCourseRunsValidator()
            {
               // RuleFor(t => t.NewStartDate).NNull().WithMessage("The Start Date must not be left Empty");
               //RuleFor(t => t.NewStartDate).GreaterThanOrEqualTo(DateTime.Today).WithMessage($"The Start Date should not be in the past");
               
                RuleFor(t => t.NewStartDate).LessThan(DateTime.Today).WithMessage($"date in the past error test");

            }
        }
    }
}
