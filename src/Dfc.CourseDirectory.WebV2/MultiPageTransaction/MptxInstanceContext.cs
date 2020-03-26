using System;
using System.Collections.Generic;
using Mapster;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public abstract class MptxInstanceContext
    {
        protected MptxInstanceContext(
            IMptxStateProvider stateProvider,
            MptxInstance instance)
        {
            StateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public string FlowName => Instance.FlowName;

        public MptxInstance Instance { get; }

        public string InstanceId => Instance.InstanceId;

        public IReadOnlyDictionary<string, object> Items => Instance.Items;

        public object State => Instance.State;

        protected IMptxStateProvider StateProvider { get; }
    }

    public class MptxInstanceContext<TState> : MptxInstanceContext
        where TState : IMptxState
    {
        public MptxInstanceContext(IMptxStateProvider stateProvider, MptxInstance instance)
            : base(stateProvider, instance)
        {
        }

        public bool IsCompleted { get; private set; }

        public new TState State => (TState)base.State;

        public void Assign<TOther>(TOther newState) => Update(oldState => newState.Adapt(oldState));

        public void Complete()
        {
            if (IsCompleted)
            {
                return;
            }

            StateProvider.DeleteInstance(InstanceId);
            IsCompleted = true;
        }

        public void Update(Action<TState> update)
        {
            ThrowIfCompleted();

            StateProvider.UpdateInstanceState(InstanceId, state =>
            {
                update((TState)state);
                return state;
            });

            // Refresh the cached state object - required so multiple state updates
            // in a single refresh are 'seen' everywhere
            update(State);
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
