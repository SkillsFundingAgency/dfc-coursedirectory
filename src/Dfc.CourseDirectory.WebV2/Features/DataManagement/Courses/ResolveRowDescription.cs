using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.CourseValidation;
using FluentValidation;
using MediatR;
using OneOf;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.ResolveRowDescription
{
    public class Query : IRequest<ModelWithErrors<Command>>
    {
        public int RowNumber { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Command>, UploadStatus>>
    {
        public int RowNumber { get; set; }
        public string WhoThisCourseIsFor { get; set; }
        public string EntryRequirements { get; set; }
        public string WhatYouWillLearn { get; set; }
        public string HowYouWillLearn { get; set; }
        public string WhatYouWillNeedToBring { get; set; }
        public string HowYouWillBeAssessed { get; set; }
        public string WhereNext { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ModelWithErrors<Command>>,
        IRequestHandler<Command, OneOf<ModelWithErrors<Command>, UploadStatus>>
    {
        private readonly IFileUploadProcessor _fileUploadProcessor;
        private readonly IProviderContextProvider _providerContextProvider;

        public Handler(IFileUploadProcessor fileUploadProcessor, IProviderContextProvider providerContextProvider)
        {
            _fileUploadProcessor = fileUploadProcessor;
            _providerContextProvider = providerContextProvider;
        }

        public async Task<ModelWithErrors<Command>> Handle(Query request, CancellationToken cancellationToken)
        {
            var row = await GetRow(request.RowNumber);

            var command = new Command()
            {
                RowNumber = request.RowNumber,
                WhoThisCourseIsFor = row.WhoThisCourseIsFor,
                EntryRequirements = row.EntryRequirements,
                WhatYouWillLearn = row.WhatYouWillLearn,
                HowYouWillLearn = row.HowYouWillLearn,
                WhatYouWillNeedToBring = row.WhatYouWillNeedToBring,
                HowYouWillBeAssessed = row.HowYouWillBeAssessed,
                WhereNext = row.WhereNext
            };

            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(command);

            return new ModelWithErrors<Command>(command, validationResult);
        }

        public async Task<OneOf<ModelWithErrors<Command>, UploadStatus>> Handle(Command request, CancellationToken cancellationToken)
        {
            var row = await GetRow(request.RowNumber);

            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            return await _fileUploadProcessor.UpdateCourseUploadRowGroupForProvider(
                _providerContextProvider.GetProviderId(),
                row.CourseId,
                new CourseUploadRowGroupUpdate()
                {
                    WhoThisCourseIsFor = request.WhoThisCourseIsFor,
                    EntryRequirements = request.EntryRequirements,
                    WhatYouWillLearn = request.WhatYouWillLearn,
                    HowYouWillLearn = request.HowYouWillLearn,
                    WhatYouWillNeedToBring = request.WhatYouWillNeedToBring,
                    HowYouWillBeAssessed = request.HowYouWillBeAssessed,
                    WhereNext = request.WhereNext
                });
        }

        private async Task<CourseUploadRow> GetRow(int rowNumber)
        {
            var providerId = _providerContextProvider.GetProviderId();

            var row = await _fileUploadProcessor.GetCourseUploadRowDetailForProvider(providerId, rowNumber);
            if (row == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.CourseUploadRow, rowNumber);
            }

            if (row.IsValid)
            {
                throw new InvalidStateException();
            }

            return row;
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
    }
}
