using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxInstance
    {
        public MptxInstance(
            Type stateType,
            string instanceId,
            IReadOnlyDictionary<string, object> items,
            object state)
        {
            StateType = stateType ?? throw new ArgumentNullException(nameof(stateType));
            InstanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
            Items = items ?? throw new ArgumentNullException(nameof(items));
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public string InstanceId { get; }

        public IReadOnlyDictionary<string, object> Items { get; }

        public object State { get; }

        public Type StateType { get; set; }
    }
}
