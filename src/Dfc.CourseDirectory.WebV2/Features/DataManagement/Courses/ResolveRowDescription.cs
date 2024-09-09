﻿using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.Helpers;
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
        public bool IsNonLars { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Command>, UploadStatus>>
    {
        public int RowNumber { get; set; }
        public bool IsNonLars { get; set; }
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
            var row = await GetRow(request.RowNumber, request.IsNonLars);

            var command = new Command()
            {
                RowNumber = request.RowNumber,
                IsNonLars = request.IsNonLars,
                WhoThisCourseIsFor = ASCIICodeHelper.ReplaceHexCodes(row.WhoThisCourseIsFor),
                EntryRequirements = ASCIICodeHelper.ReplaceHexCodes(row.EntryRequirements),
                WhatYouWillLearn = ASCIICodeHelper.ReplaceHexCodes(row.WhatYouWillLearn),
                HowYouWillLearn = ASCIICodeHelper.ReplaceHexCodes(row.HowYouWillLearn),
                WhatYouWillNeedToBring = ASCIICodeHelper.ReplaceHexCodes(row.WhatYouWillNeedToBring),
                HowYouWillBeAssessed = ASCIICodeHelper.ReplaceHexCodes(row.HowYouWillBeAssessed),
                WhereNext = ASCIICodeHelper.ReplaceHexCodes(row.WhereNext)
            };

            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(command);

            return new ModelWithErrors<Command>(command, validationResult);
        }

        public async Task<OneOf<ModelWithErrors<Command>, UploadStatus>> Handle(Command request, CancellationToken cancellationToken)
        {
            var row = await GetRow(request.RowNumber, request.IsNonLars);

            Command formattedRequest = new Command
            {
                WhoThisCourseIsFor = request.WhoThisCourseIsFor?.Replace("\r\n", "\n"),
                EntryRequirements = request.EntryRequirements?.Replace("\r\n", "\n"),
                WhatYouWillLearn = request.WhatYouWillLearn?.Replace("\r\n", "\n"),
                HowYouWillLearn = request.HowYouWillLearn?.Replace("\r\n", "\n"),
                WhatYouWillNeedToBring = request.WhatYouWillNeedToBring?.Replace("\r\n", "\n"),
                HowYouWillBeAssessed = request.HowYouWillBeAssessed?.Replace("\r\n", "\n"),
                WhereNext = request.WhereNext?.Replace("\r\n", "\n"),
                RowNumber = request.RowNumber,
                IsNonLars = request.IsNonLars
            };

            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(formattedRequest);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            return await _fileUploadProcessor.UpdateCourseUploadRowGroupForProvider(
                _providerContextProvider.GetProviderId(),
                row.CourseId,
                new CourseUploadRowGroupUpdate()
                {
                    WhoThisCourseIsFor = ASCIICodeHelper.ReplaceHexCodes(formattedRequest.WhoThisCourseIsFor),
                    EntryRequirements = ASCIICodeHelper.ReplaceHexCodes(formattedRequest.EntryRequirements),
                    WhatYouWillLearn = ASCIICodeHelper.ReplaceHexCodes(formattedRequest.WhatYouWillLearn),
                    HowYouWillLearn = ASCIICodeHelper.ReplaceHexCodes(formattedRequest.HowYouWillLearn),
                    WhatYouWillNeedToBring = ASCIICodeHelper.ReplaceHexCodes(formattedRequest.WhatYouWillNeedToBring),
                    HowYouWillBeAssessed = ASCIICodeHelper.ReplaceHexCodes(formattedRequest.HowYouWillBeAssessed),
                    WhereNext = ASCIICodeHelper.ReplaceHexCodes(formattedRequest.WhereNext)
                }, request.IsNonLars);
        }

        private async Task<CourseUploadRow> GetRow(int rowNumber, bool isNonLars)
        {
            var providerId = _providerContextProvider.GetProviderId();

            var row = await _fileUploadProcessor.GetCourseUploadRowDetailForProvider(providerId, rowNumber, isNonLars);
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
