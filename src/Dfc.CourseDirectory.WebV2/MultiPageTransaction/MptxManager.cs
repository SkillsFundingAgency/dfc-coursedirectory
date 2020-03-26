﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        public async Task<MptxInstance> CreateInstance(
            string flowName,
            Type stateType,
            HttpRequest request,
            IEnumerable<string> capturesQueryParams)
        {
            if (!typeof(IMptxState).IsAssignableFrom(stateType))
            {
                throw new ArgumentException(
                    $"State type must implement {typeof(IMptxState).FullName}.",
                    nameof(stateType));
            }

            var newState = CreateNewState(stateType);

            var contextItems = GetContextItemsFromCaptures(request, capturesQueryParams);

            var instance = _stateProvider.CreateInstance(flowName, contextItems, newState);
            await InitializeState(instance, stateType);

            return instance;
        }

        public MptxInstance GetInstance(string instanceId) => _stateProvider.GetInstance(instanceId);

        private object CreateNewState(Type stateType) =>
            ActivatorUtilities.CreateInstance(_serviceProvider, stateType);

        private IReadOnlyDictionary<string, object> GetContextItemsFromCaptures(
            HttpRequest request,
            IEnumerable<string> capturesQueryParams)
        {
            var dict = new Dictionary<string, object>();

            foreach (var capture in capturesQueryParams)
            {
                var val = (string)request.Query[capture];
                dict.Add(capture, val);
            }

            return dict;
        }

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
