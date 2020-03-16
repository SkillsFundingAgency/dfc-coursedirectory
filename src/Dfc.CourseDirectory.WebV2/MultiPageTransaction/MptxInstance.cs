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
            FlowName = flowName;
            InstanceId = instanceId;
            Items = items;
            State = state;
        }

        public string FlowName { get; }

        public string InstanceId { get; }

        public IReadOnlyDictionary<string, object> Items { get; }

        public object State { get; }
    }
}
