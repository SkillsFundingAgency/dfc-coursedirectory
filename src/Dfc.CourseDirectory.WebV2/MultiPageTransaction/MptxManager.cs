﻿using System;
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
            return (MptxInstanceContext<TState>)CreateInstance(typeof(TState), state, contextItems);
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

            return CreateInstance(stateType, newState, contextItems);
        }

        private MptxInstanceContext CreateInstance(
            Type stateType,
            object state,
            IReadOnlyDictionary<string, object> contextItems = null)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var instance = _stateProvider.CreateInstance(stateType, contextItems, state);

            var instanceContext = _instanceContextFactory.CreateContext(instance, stateType);
            return instanceContext;
        }

        public MptxInstanceContext GetInstance(string instanceId)
        {
            var instance = _stateProvider.GetInstance(instanceId);

            if (instance == null)
            {
                return null;
            }

            return _instanceContextFactory.CreateContext(instance, instance.StateType);
        }

        private object CreateNewState(Type stateType) =>
            ActivatorUtilities.CreateInstance(_serviceProvider, stateType);
    }
}
