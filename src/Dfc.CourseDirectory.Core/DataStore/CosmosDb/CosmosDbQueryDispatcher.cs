using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.StoredProcedures;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb
{
    public class CosmosDbQueryDispatcher : ICosmosDbQueryDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly StoredProcedureDeployHelper _storedProcedureDeployHelper;

        public CosmosDbQueryDispatcher(
            IServiceProvider serviceProvider,
            StoredProcedureDeployHelper storedProcedureDeployHelper)
        {
            _serviceProvider = serviceProvider;
            _storedProcedureDeployHelper = storedProcedureDeployHelper;
        }

        public virtual async Task<T> ExecuteQuery<T>(ICosmosDbQuery<T> query)
        {
            await _storedProcedureDeployHelper.EnsureStoredProceduresAreDeployed();

            var client = _serviceProvider.GetRequiredService<DocumentClient>();
            var configuration = _serviceProvider.GetRequiredService<Configuration>();

            var handlerType = typeof(ICosmosDbQueryHandler<,>).MakeGenericType(query.GetType(), typeof(T));
            var handler = _serviceProvider.GetRequiredService(handlerType);

            // TODO We could make this waaay more efficient
            var result = await (Task<T>)handlerType.GetMethod("Execute").Invoke(handler, new object[] { client, configuration, query });

            return result;
        }
    }
}
