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
                kvp => new MptxInstance(kvp.Value.FlowName, kvp.Key, kvp.Value.Items, kvp.Value.State));

        public void Clear() => _instances.Clear();

        public MptxInstance CreateInstance(
            string flowName,
            IReadOnlyDictionary<string, object> items,
            object state)
        {
            var instanceId = Guid.NewGuid().ToString();

            var entry = new Entry()
            {
                FlowName = flowName,
                Items = items,
                State = state
            };
            _instances.Add(instanceId, entry);

            var instance = new MptxInstance(flowName, instanceId, items, state);

            return instance;
        }

        public void DeleteInstance(string instanceId) => _instances.Remove(instanceId);

        public MptxInstance GetInstance(string instanceId)
        {
            if (_instances.TryGetValue(instanceId, out var entry))
            {
                return new MptxInstance(entry.FlowName, instanceId, entry.Items, entry.State);
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
            public string FlowName { get; set; }
            public IReadOnlyDictionary<string, object> Items { get; set; }
            public object State { get; set; }
        }
    }
}
