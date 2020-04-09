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
            IReadOnlyDictionary<string, object> contextItems = null)
            where TState : IMptxState
        {
            return (MptxInstanceContext<TState>)CreateInstance(typeof(TState), contextItems);
        }

        public MptxInstanceContext<TState> CreateInstance<TState>(
            TState state,
            IReadOnlyDictionary<string, object> contextItems = null)
            where TState : IMptxState
        {
            return (MptxInstanceContext<TState>)CreateInstance(
                typeof(TState),
                parentInstanceId: null,
                parentStateType: null,
                state,
                contextItems);
        }

        public MptxInstanceContext CreateInstance(
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

            return CreateInstance(stateType, parentInstanceId: null, parentStateType: null, newState, contextItems);
        }

        public MptxInstanceContext<TState, TParentState> CreateInstance<TState, TParentState>(
            string parentInstanceId,
            IReadOnlyDictionary<string, object> contextItems = null)
            where TState : IMptxState<TParentState>
            where TParentState : IMptxState
        {
            return (MptxInstanceContext<TState, TParentState>)CreateInstance(
                typeof(TState),
                parentInstanceId,
                typeof(TParentState),
                contextItems);
        }

        public MptxInstanceContext<TState, TParentState> CreateInstance<TState, TParentState>(
            MptxInstanceContext<TParentState> parent,
            IReadOnlyDictionary<string, object> contextItems = null)
            where TState : IMptxState<TParentState>
            where TParentState : IMptxState
        {
            return (MptxInstanceContext<TState, TParentState>)CreateInstance(
                typeof(TState),
                parent.InstanceId,
                typeof(TParentState),
                contextItems);
        }

        public MptxInstanceContext<TState, TParentState> CreateInstance<TState, TParentState>(
            string parentInstanceId,
            TState state,
            IReadOnlyDictionary<string, object> contextItems = null)
            where TState : IMptxState<TParentState>
            where TParentState : IMptxState
        {
            return (MptxInstanceContext<TState, TParentState>)CreateInstance(
                typeof(TState),
                parentInstanceId,
                typeof(TParentState),
                state,
                contextItems);
        }

        public MptxInstanceContext<TState, TParentState> CreateInstance<TState, TParentState>(
            MptxInstanceContext<TParentState> parent,
            TState state,
            IReadOnlyDictionary<string, object> contextItems = null)
            where TState : IMptxState<TParentState>
            where TParentState : IMptxState
        {
            return (MptxInstanceContext<TState, TParentState>)CreateInstance(
                typeof(TState),
                parent.InstanceId,
                typeof(TParentState),
                state,
                contextItems);
        }

        public MptxInstanceContext CreateInstance(
            Type stateType,
            string parentInstanceId,
            Type parentStateType,
            IReadOnlyDictionary<string, object> contextItems = null)
        {
            if (!typeof(IMptxState).IsAssignableFrom(stateType))
            {
                throw new ArgumentException(
                    $"State type must implement {typeof(IMptxState).FullName}.",
                    nameof(stateType));
            }

            var newState = CreateNewState(stateType);

            return CreateInstance(stateType, parentInstanceId, parentStateType, newState, contextItems);
        }

        private MptxInstanceContext CreateInstance(
            Type stateType,
            string parentInstanceId,
            Type parentStateType,
            object state,
            IReadOnlyDictionary<string, object> contextItems = null)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var instance = _stateProvider.CreateInstance(stateType, parentInstanceId, state, contextItems);

            return _instanceContextFactory.CreateContext(instance, stateType, parentStateType);
        }

        public MptxInstanceContext GetInstance(string instanceId)
        {
            var instance = _stateProvider.GetInstance(instanceId);

            if (instance == null)
            {
                return null;
            }

            var parentStateType = instance.ParentInstanceId != null ?
                instance.ParentStateType :
                null;

            return _instanceContextFactory.CreateContext(instance, instance.StateType, parentStateType);
        }

        private object CreateNewState(Type stateType) =>
            ActivatorUtilities.CreateInstance(_serviceProvider, stateType);
    }
}
