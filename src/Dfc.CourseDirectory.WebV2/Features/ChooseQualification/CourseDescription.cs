﻿using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.CourseValidation;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using FluentValidation;
using Mapster;
using MediatR;
using OneOf;
using OneOf.Types;
using System.Linq;

namespace Dfc.CourseDirectory.WebV2.Features.ChooseQualification.CourseDescription
{
    using CommandResponse = OneOf<ModelWithErrors<Command>, Success>;

    public class ViewModel : Command
    {
        public string LarsCode { get; set; }
    }

    public class Query : IRequest<ViewModel>
    {
    }

    public class Command : IRequest<CommandResponse>
    {
        public string WhoThisCourseIsFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYouWillLearn { get; set; }
        public string HowYouWillLearn { get; set; }
        public string WhatYouWillNeedToBring { get; set; }
        public string HowYouWillBeAssessed { get; set; }
        public string WhereNext { get; set; }
        public CourseType? CourseType { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, CommandResponse>
    {
        private readonly MptxInstanceContext<FlowModel> _flow;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(MptxInstanceContext<FlowModel> flow, ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _flow = flow;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            if (_flow.State.LarsCode == null)
            {
                throw new InvalidStateException();
            }

            var vm = new ViewModel()
            {
                WhoThisCourseIsFor = _flow.State.WhoThisCourseIsFor,
                EntryRequirements = _flow.State.EntryRequirements,
                WhatYouWillLearn = _flow.State.WhatYouWillLearn,
                HowYouWillLearn = _flow.State.HowYouWillLearn,
                WhatYouWillNeedToBring = _flow.State.WhatYouWillNeedToBring,
                HowYouWillBeAssessed = _flow.State.HowYouWillBeAssessed,
                WhereNext = _flow.State.WhereNext
            };
            return Task.FromResult(vm);
        }

        public async Task<CommandResponse> Handle(Command request, CancellationToken cancellationToken)
        {
            if (_flow.State.LarsCode == null)
            {
                throw new InvalidStateException();
            }

            Command formattedRequest = new Command
            {
                WhoThisCourseIsFor = request.WhoThisCourseIsFor?.Replace("\r\n", "\n"),
                EntryRequirements = request.EntryRequirements?.Replace("\r\n", "\n"),
                WhatYouWillLearn = request.WhatYouWillLearn?.Replace("\r\n", "\n"),
                HowYouWillLearn = request.HowYouWillLearn?.Replace("\r\n", "\n"),
                WhatYouWillNeedToBring = request.WhatYouWillNeedToBring?.Replace("\r\n", "\n"),
                HowYouWillBeAssessed = request.HowYouWillBeAssessed?.Replace("\r\n", "\n"),
                WhereNext = request.WhereNext?.Replace("\r\n", "\n")
            };

            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(formattedRequest);

            if (!validationResult.IsValid)
            {
                var vm = request.Adapt<ViewModel>();
                return new ModelWithErrors<Command>(vm, validationResult);
            }
            
            request.CourseType = await GetCourseType();

            _flow.Update(s => s.SetCourseDescription(
                request.WhoThisCourseIsFor,
                request.EntryRequirements,
                request.WhatYouWillLearn,
                request.HowYouWillLearn,
                request.WhatYouWillNeedToBring,
                request.HowYouWillBeAssessed,
                request.WhereNext,
                request.CourseType));

            return new Success();
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.WhoThisCourseIsFor).WhoThisCourseIsFor();
                RuleFor(c => c.EntryRequirements).EntryRequirements();
                RuleFor(c => c.WhatYouWillLearn).WhatYouWillLearn();
                RuleFor(c => c.HowYouWillLearn).HowYouWillLearn();
                RuleFor(c => c.WhatYouWillNeedToBring).WhatYouWillNeedToBring();
                RuleFor(c => c.HowYouWillBeAssessed).HowYouWillBeAssessed();
                RuleFor(c => c.WhereNext).WhereNext();
            }
        }

        private async Task<CourseType?> GetCourseType()
        {
            var larsCourseTypes = await _sqlQueryDispatcher.ExecuteQuery(new GetLarsCourseType() { LearnAimRef = _flow.State.LarsCode });

            foreach (var larsCourseType in larsCourseTypes)
            {
                if (larsCourseType.CategoryRef == "40" && !larsCourseType.LearnAimRefTitle.Contains("ESOL"))
                {
                    larsCourseType.CourseType = null;
                    continue;
                }

                if (larsCourseType.CategoryRef == "3" && !larsCourseType.LearnAimRefTitle.StartsWith("T Level"))
                {
                    larsCourseType.CourseType = null;
                }
            }

            var distinctLarsCourseTypes = larsCourseTypes.Select(lc => lc.CourseType).Distinct();

            return distinctLarsCourseTypes.FirstOrDefault();
        }
    }
}
