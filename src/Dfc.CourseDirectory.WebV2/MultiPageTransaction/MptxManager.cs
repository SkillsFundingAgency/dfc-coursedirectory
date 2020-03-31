using System;
using System.Collections.Generic;
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

        public MptxInstanceContext<TState> CreateInstance<TState>(
            string flowName,
            IReadOnlyDictionary<string, object> contextItems = null)
            where TState : IMptxState
        {
            return (MptxInstanceContext<TState>)CreateInstance(flowName, typeof(TState), contextItems);
        }

        public MptxInstanceContext<TState> CreateInstance<TState>(
            string flowName,
            TState state,
            IReadOnlyDictionary<string, object> contextItems = null)
            where TState : IMptxState
        {
            return (MptxInstanceContext<TState>)CreateInstance(flowName, typeof(TState), state, contextItems);
        }

        public MptxInstanceContext CreateInstance(
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

            return CreateInstance(flowName, stateType, newState, contextItems);
        }

        private MptxInstanceContext CreateInstance(
            string flowName,
            Type stateType,
            object state,
            IReadOnlyDictionary<string, object> contextItems = null)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var instance = _stateProvider.CreateInstance(flowName, contextItems, state);

            var instanceContext = _instanceContextFactory.CreateContext(instance);
            return instanceContext;
        }

        public MptxInstanceContext GetInstance(string instanceId)
        {
            var instance = _stateProvider.GetInstance(instanceId);

            if (instance == null)
            {
                return null;
            }

            return _instanceContextFactory.CreateContext(instance);
        }

        private object CreateNewState(Type stateType) =>
            ActivatorUtilities.CreateInstance(_serviceProvider, stateType);
    }
}
