using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxInstance
    {
        public MptxInstance(
            string flowName,
            string instanceId,
            IReadOnlyDictionary<string, object> items,
            object state)
        {
            FlowName = flowName ?? throw new ArgumentNullException(nameof(flowName));
            InstanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
            Items = items ?? throw new ArgumentNullException(nameof(items));
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public string FlowName { get; }

        public string InstanceId { get; }

        public IReadOnlyDictionary<string, object> Items { get; }

        public object State { get; }

        public Type StateType => State.GetType();
    }
}
