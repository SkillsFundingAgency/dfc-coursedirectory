using System;
using System.Collections.Generic;
using Mapster;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxInstanceContext<TState>
        where TState : IMptxState, new()
    {
        private readonly IMptxStateProvider _stateProvider;
        private readonly MptxInstance _instance;

        public MptxInstanceContext(
            IMptxStateProvider stateProvider,
            MptxInstance instance)
        {
            _stateProvider = stateProvider;
            _instance = instance;
        }

        public string FlowName => _instance.FlowName;

        public string InstanceId => _instance.InstanceId;

        public bool IsCompleted { get; private set; }

        public IReadOnlyDictionary<string, object> Items => _instance.Items;

        public TState State => (TState)_instance.State ?? new TState();

        public void Assign<TOther>(TOther newState) => Update(oldState => newState.Adapt(oldState));

        public void Complete()
        {
            if (IsCompleted)
            {
                return;
            }

            _stateProvider.DeleteInstance(InstanceId);
            IsCompleted = true;
        }

        public void Update(Action<TState> update)
        {
            ThrowIfCompleted();

            _stateProvider.UpdateInstanceState(InstanceId, s =>
            {
                var state = (TState)s ?? new TState();
                update(state);
                return state;
            });
        }

        private void ThrowIfCompleted()
        {
            if (IsCompleted)
            {
                throw new InvalidOperationException("Instance has been completed.");
            }
        }
    }
}
