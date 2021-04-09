using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Validation;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.DataManagement.Venues
{
    public class Command : IRequest<OneOf<ModelWithErrors<Command>, Success>>
    {
        public Guid ProviderId { get; set; }
        public IFormFile File { get; set; }
    }

    public class Handler : IRequestHandler<Command, OneOf<ModelWithErrors<Command>, Success>>
    {
        private readonly CommandValidator _validator;

        public Handler()
        {
            _validator = new CommandValidator();
        }

        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var result = await _validator.ValidateAsync(request);

            if (!result.IsValid)
                return new ModelWithErrors<Command>(request, result);
            else
                return new Success();
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.File).NotNull().WithMessage("File cannot be null");
            RuleFor(x => x.File.ContentType).Equal("text/csv", StringComparer.OrdinalIgnoreCase).Unless(item => item.File == null).WithMessage("File must be a csv file");
        }
    }
}
