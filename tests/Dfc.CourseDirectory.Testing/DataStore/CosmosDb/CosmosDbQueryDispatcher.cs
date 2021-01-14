using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb
{
    public class CosmosDbQueryDispatcher : ICosmosDbQueryDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly object _syncLock = new object();

        public CosmosDbQueryDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public virtual Task<T> ExecuteQuery<T>(ICosmosDbQuery<T> query)
        {
            lock (_syncLock)
            {
                var inMemoryStore = _serviceProvider.GetRequiredService<InMemoryDocumentStore>();

                var handlerType = typeof(ICosmosDbQueryHandler<,>).MakeGenericType(query.GetType(), typeof(T));
                var handler = _serviceProvider.GetRequiredService(handlerType);

                // TODO We could make this waaay more efficient
                var result = (T)handlerType.GetMethod("Execute").Invoke(handler, new object[] { inMemoryStore, query });

                return Task.FromResult(result);
            }
        }
    }
}
