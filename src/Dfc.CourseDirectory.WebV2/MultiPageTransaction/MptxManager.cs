using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxManager
    {
        private readonly IMptxStateProvider _stateProvider;
        private readonly MptxInstanceContextFactory _instanceContextFactory;
        private readonly IServiceProvider _serviceProvider;

        public MptxManager(
            IMptxStateProvider stateProvider,
            MptxInstanceContextFactory mptxInstanceContextFactory,
            IServiceProvider serviceProvider)
        {
            _stateProvider = stateProvider;
            _instanceContextFactory = mptxInstanceContextFactory;
            _serviceProvider = serviceProvider;
        }

        public async Task<MptxInstanceContext> CreateInstance(
            string flowName,
            Type stateType,
            IReadOnlyDictionary<string, object> contextItems = null)
        {
            if (!typeof(IMptxState).IsAssignableFrom(stateType))
            {
                throw new ArgumentException(
                    $"State type must implement {typeof(IMptxState).FullName}.",
                    nameof(stateType));
            }

            var newState = CreateNewState(stateType);

            var instance = _stateProvider.CreateInstance(flowName, contextItems, newState);
            await InitializeState(instance, stateType);

            var instanceContext = _instanceContextFactory.CreateContext(instance);
            return instanceContext;
        }

        public MptxInstanceContext GetInstance(string instanceId)
        {
            var instance = _stateProvider.GetInstance(instanceId);
            return _instanceContextFactory.CreateContext(instance);
        }

        private object CreateNewState(Type stateType) =>
            ActivatorUtilities.CreateInstance(_serviceProvider, stateType);

        private async Task InitializeState(MptxInstance instance, Type stateType)
        {
            var initializerType = typeof(IInitializeMptxState<>).MakeGenericType(stateType);
            var initializer = _serviceProvider.GetService(initializerType);

            if (initializer != null)
            {
                var context = _instanceContextFactory.CreateContext(instance);

                await (Task)initializerType.GetMethod("Initialize").Invoke(
                    initializer,
                    new object[] { context });
            }
        }
    }
}
