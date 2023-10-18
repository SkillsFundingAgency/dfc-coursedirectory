using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.ProviderValidation;
using Dfc.CourseDirectory.WebV2.Behaviors;
using Dfc.CourseDirectory.WebV2.Security;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Providers.EditProviderInfo
{
    public class Query : IRequest<Command>
    {
        public Guid ProviderId { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Command>, Success>>
    {
        public Guid ProviderId { get; set; }
        public string MarketingInformation { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, Command>,
        IRequestHandler<Command, OneOf<ModelWithErrors<Command>, Success>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IClock _clock;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICurrentUserProvider currentUserProvider,
            IClock clock)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _currentUserProvider = currentUserProvider;
            _clock = clock;
        }

        public async Task<Command> Handle(Query request, CancellationToken cancellationToken)
        {
            var provider = await GetProvider(request.ProviderId);

            return new Command()
            {
                ProviderId = request.ProviderId,
                MarketingInformation = provider.MarketingInformation
            };
        }

        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            var currentUser = _currentUserProvider.GetCurrentUser();

            var updateCommand = new UpdateProviderInfo()
            {
                ProviderId = request.ProviderId,
                MarketingInformation = request.MarketingInformation,
                UpdatedBy = currentUser,
                UpdatedOn = _clock.UtcNow
            };
            await _sqlQueryDispatcher.ExecuteQuery(updateCommand);

            return new Success();
        }

        private async Task<Provider> GetProvider(Guid providerId)
        {
            var result = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderById() { ProviderId = providerId });

            if (result == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Provider, providerId);
            }

            return result;
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.MarketingInformation).MarketingInformation();
            }
        }
    }
}
