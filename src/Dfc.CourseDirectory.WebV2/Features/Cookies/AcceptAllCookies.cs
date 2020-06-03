using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Cookies;
using MediatR;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Cookies.AcceptAllCookies
{
    public class Command : IRequest<Success>
    {
    }

    public class Handler : IRequestHandler<Command, Success>
    {
        private readonly ICookieSettingsProvider _cookieSettingsProvider;

        public Handler(ICookieSettingsProvider cookieSettingsProvider)
        {
            _cookieSettingsProvider = cookieSettingsProvider;
        }

        public Task<Success> Handle(Command request, CancellationToken cancellationToken)
        {
            _cookieSettingsProvider.SetPreferencesForCurrentUser(new CookieSettings()
            {
                AllowAnalyticsCookies = true
            });

            return Task.FromResult(new Success());
        }
    }
}
