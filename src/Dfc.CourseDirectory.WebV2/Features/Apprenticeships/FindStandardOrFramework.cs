using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation;
using Mapster;
using MediatR;
using OneOf;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Apprenticeships.FindStandardOrFramework
{
    using QueryResponse = OneOf<ModelWithErrors<ViewModel>, ViewModel>;

    public class FlowModel : IMptxState<IFlowModelCallback>
    {
        public Guid ProviderId { get; set; }
    }

    public interface IFlowModelCallback : IMptxState
    {
        void ReceiveStandardOrFramework(StandardOrFramework standardOrFramework);
    }

    public class Query : IRequest<ViewModel>
    {
    }

    public class SearchQuery : IRequest<QueryResponse>
    {
        public string Search { get; set; }
    }

    public class ViewModel
    {
        public string Search { get; set; }
        public bool SearchWasDone { get; set; }
        public IReadOnlyCollection<ViewModelResult> Results { get; set; }
    }

    public class SelectCommand : IRequest<Success>
    {
        public StandardOrFramework StandardOrFramework { get; set; }
    }

    public class ViewModelResult
    {
        public string ApprenticeshipTitle { get; set; }
        public bool IsFramework { get; set; }
        public bool? OtherBodyApprovalRequired { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public int? FrameworkCode { get; set; }
        public int? FrameworkProgType { get; set; }
        public int? FrameworkPathwayCode { get; set; }
        public int? StandardCode { get; set; }
        public int? StandardVersion { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRestrictProviderType<Query>,
        IRequestHandler<SearchQuery, QueryResponse>,
        IRestrictProviderType<SearchQuery>,
        IRequestHandler<SelectCommand, Success>,
        IRestrictProviderType<SelectCommand>
    {
        private readonly IStandardsAndFrameworksCache _standardsAndFrameworksCache;
        private readonly MptxInstanceContext<FlowModel, IFlowModelCallback> _flow;

        public Handler(
            IStandardsAndFrameworksCache standardsAndFrameworksCache,
            MptxInstanceContext<FlowModel, IFlowModelCallback> flow)
        {
            _standardsAndFrameworksCache = standardsAndFrameworksCache;
            _flow = flow;
        }

        ProviderType IRestrictProviderType<SearchQuery>.ProviderType => ProviderType.Apprenticeships;

        ProviderType IRestrictProviderType<Query>.ProviderType => ProviderType.Apprenticeships;

        ProviderType IRestrictProviderType<SelectCommand>.ProviderType => ProviderType.Apprenticeships;

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var vm = new ViewModel()
            {
                SearchWasDone = false
            };
            return Task.FromResult(vm);
        }

        public async Task<QueryResponse> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var validator = new QueryValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var vm = request.Adapt<ViewModel>();
                return new ModelWithErrors<ViewModel>(vm, validationResult);
            }

            var searchResults = await _standardsAndFrameworksCache.SearchStandardsAndFrameworks(request.Search);

            return new ViewModel()
            {
                Search = request.Search,
                SearchWasDone = true,
                Results = searchResults
                    .Select(r => new ViewModelResult()
                    {
                        ApprenticeshipTitle = r.Match(s => s.StandardName, f => f.NasTitle),
                        IsFramework = r.Value is Framework,
                        NotionalNVQLevelv2 = r.Match(s => s.NotionalNVQLevelv2, f => null),
                        OtherBodyApprovalRequired = r.Match(s => s.OtherBodyApprovalRequired, f => (bool?)null),
                        FrameworkCode = r.Match(s => (int?)null, f => f.FrameworkCode),
                        FrameworkProgType = r.Match(s => (int?)null, f => f.ProgType),
                        FrameworkPathwayCode = r.Match(s => (int?)null, f => f.PathwayCode),
                        StandardCode = r.Match(s => s.StandardCode, f => (int?)null),
                        StandardVersion = r.Match(s => s.Version, f => (int?)null)
                    })
                    .ToList()
            };
        }

        public Task<Success> Handle(SelectCommand request, CancellationToken cancellationToken)
        {
            _flow.UpdateParent(s => s.ReceiveStandardOrFramework(request.StandardOrFramework));

            return Task.FromResult(new Success());
        }

        Guid IRestrictProviderType<Query>.GetProviderId(Query request) => _flow.State.ProviderId;

        Guid IRestrictProviderType<SearchQuery>.GetProviderId(SearchQuery request) => _flow.State.ProviderId;

        Guid IRestrictProviderType<SelectCommand>.GetProviderId(SelectCommand request) => _flow.State.ProviderId;

        private class QueryValidator : AbstractValidator<SearchQuery>
        {
            public QueryValidator()
            {
                RuleFor(q => q.Search)
                    .NotEmpty()
                    .MinimumLength(3)
                    .WithMessageForAllRules("Name or keyword for the apprenticeship this training is for must be 3 characters or more");
            }
        }
    }
}
