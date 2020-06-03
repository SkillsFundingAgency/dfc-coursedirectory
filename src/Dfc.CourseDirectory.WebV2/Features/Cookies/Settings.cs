using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Cookies;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Cookies.Settings
{
    public class Query : IRequest<Command>
    {
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Command>, Success>>
    {
        public bool? AllowAnalyticsCookies { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, Command>,
        IRequestHandler<Command, OneOf<ModelWithErrors<Command>, Success>>
    {
        private readonly ICookieSettingsProvider _cookiePreferencesProvider;

        public Handler(ICookieSettingsProvider cookiePreferencesProvider)
        {
            _cookiePreferencesProvider = cookiePreferencesProvider;
        }

        public Task<Command> Handle(Query request, CancellationToken cancellationToken)
        {
            var currentUserPreferences = _cookiePreferencesProvider.GetPreferencesForCurrentUser();

            var command = new Command()
            {
                AllowAnalyticsCookies = currentUserPreferences?.AllowAnalyticsCookies
            };

            return Task.FromResult(command);
        }

        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            var preferences = new CookieSettings()
            {
                AllowAnalyticsCookies = request.AllowAnalyticsCookies.Value
            };
            _cookiePreferencesProvider.SetPreferencesForCurrentUser(preferences);

            return new Success();
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.AllowAnalyticsCookies)
                    .NotNull()
                    .WithMessage("Select if you want to use cookies that measure website use");
            }
        }
    }
}
