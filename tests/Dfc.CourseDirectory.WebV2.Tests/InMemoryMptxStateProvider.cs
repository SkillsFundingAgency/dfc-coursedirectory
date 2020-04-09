using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class InMemoryMptxStateProvider : IMptxStateProvider
    {
        private readonly Dictionary<string, Entry> _instances;

        public InMemoryMptxStateProvider()
        {
            _instances = new Dictionary<string, Entry>();
        }

        public IReadOnlyDictionary<string, MptxInstance> Instances =>
            _instances.ToDictionary(
                kvp => kvp.Key,
                kvp => new MptxInstance(
                    kvp.Key,
                    kvp.Value.StateType,
                    kvp.Value.State,
                    kvp.Value.ParentInstanceId,
                    kvp.Value.ParentInstanceId != null ? _instances[kvp.Value.ParentInstanceId].StateType : null,
                    kvp.Value.ParentInstanceId != null ? _instances[kvp.Value.ParentInstanceId].State : null,
                    kvp.Value.Items));

        public void Clear() => _instances.Clear();

        public MptxInstance CreateInstance(
            Type stateType,
            string parentInstanceId,
            object state,
            IReadOnlyDictionary<string, object> items)
        {
            object parentState = null;
            Type parentStateType = null;

            if (parentInstanceId != null)
            {
                if (!_instances.TryGetValue(parentInstanceId, out var parentEntry))
                {
                    throw new InvalidParentException(parentInstanceId);
                }

                parentState = parentEntry.State;
                parentStateType = parentEntry.StateType;
            }

            var instanceId = Guid.NewGuid().ToString();

            items ??= new Dictionary<string, object>();

            var entry = new Entry()
            {
                StateType = stateType,
                Items = items,
                State = state,
                ParentInstanceId = parentInstanceId
            };
            _instances.Add(instanceId, entry);

            return new MptxInstance(
                instanceId,
                stateType,
                state,
                parentInstanceId,
                parentStateType,
                parentState,
                items);
        }

        public void DeleteInstance(string instanceId) => _instances.Remove(instanceId);

        public MptxInstance GetInstance(string instanceId)
        {
            if (_instances.TryGetValue(instanceId, out var entry))
            {
                object parentState = null;
                Type parentStateType = null;

                if (entry.ParentInstanceId != null)
                {
                    var parentEntry = _instances[entry.ParentInstanceId];

                    parentState = parentEntry.State;
                    parentStateType = parentEntry.StateType;
                }

                return new MptxInstance(
                    instanceId,
                    entry.StateType,
                    entry.State,
                    entry.ParentInstanceId,
                    parentStateType,
                    parentState,
                    entry.Items);
            }
            else
            {
                return null;
            }
        }

        public void UpdateInstanceState(string instanceId, Func<object, object> update)
        {
            var instance = _instances[instanceId];
            update(instance.State);
        }

        private class Entry
        {
            public Type StateType { get; set; }
            public IReadOnlyDictionary<string, object> Items { get; set; }
            public object State { get; set; }
            public string ParentInstanceId { get; set; }
        }
    }
}
