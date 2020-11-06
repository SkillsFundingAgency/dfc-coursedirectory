using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using FluentValidation;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Providers.EditProviderType
{
    public class Query : IRequest<Command>
    {
        public Guid ProviderId { get; set; }
    }

    public class Command : IRequest<OneOf<ModelWithErrors<Command>, Success>>
    {
        public Guid ProviderId { get; set; }
        public ProviderType? ProviderType { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, Command>,
        IRequestHandler<Command, OneOf<ModelWithErrors<Command>, Success>>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IProviderInfoCache _providerInfoCache;

        public Handler(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher, IProviderInfoCache providerInfoCache)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _providerInfoCache = providerInfoCache;
        }

        public async Task<Command> Handle(Query request, CancellationToken cancellationToken)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderById()
            {
                ProviderId = request.ProviderId
            });

            return new Command()
            {
                ProviderId = provider.Id,
                ProviderType = provider.ProviderType
            };
        }

        public async Task<OneOf<ModelWithErrors<Command>, Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validator = new CommandValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                return new ModelWithErrors<Command>(request, validationResult);
            }

            await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateProviderType()
            {
                ProviderId = request.ProviderId,
                ProviderType = request.ProviderType.Value
            });

            // Remove this provider from the cache - subsequent requests will re-fetch updated record
            await _providerInfoCache.Remove(request.ProviderId);

            return new Success();
        }

        private class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.ProviderType)
                    .NotNull()
                        .WithMessage("Select the provider type");
            }
        }
    }
}
