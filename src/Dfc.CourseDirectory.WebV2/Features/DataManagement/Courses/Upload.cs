using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Courses.Upload
{
    public class Command : IRequest<OneOf<ModelWithErrors<Command>, Success>>
    {
        public IFormFile File { get; set; }
    }

    public class Handler : IRequestHandler<Command, OneOf<ModelWithErrors<Command>, Success>>
    {
        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var result = await validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                return new ModelWithErrors<Command>(request, result);
            }
            else
            {
                return new Success();
            }
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.File).NotNull().WithMessage("Select a CSV");
            RuleFor(x => x.File.ContentType).Equal("text/csv", StringComparer.OrdinalIgnoreCase).Unless(item => item.File == null).WithMessage("File must be a csv file");
        }
    }
}
