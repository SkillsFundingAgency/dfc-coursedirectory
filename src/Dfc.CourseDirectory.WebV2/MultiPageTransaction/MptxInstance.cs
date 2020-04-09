using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxInstance
    {
        public MptxInstance(
            string instanceId,
            Type stateType,
            object state,
            string parentInstanceId,
            Type parentStateType,
            object parentState,
            IReadOnlyDictionary<string, object> items)
        {
            StateType = stateType ?? throw new ArgumentNullException(nameof(stateType));
            InstanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
            Items = items ?? throw new ArgumentNullException(nameof(items));
            State = state ?? throw new ArgumentNullException(nameof(state));
            ParentInstanceId = parentInstanceId;
            ParentState = parentState;
            ParentStateType = parentStateType;
        }

        public string InstanceId { get; }

        public IReadOnlyDictionary<string, object> Items { get; }

        public string ParentInstanceId { get; }

        public object ParentState { get; }

        public Type ParentStateType { get; }

        public object State { get; }

        public Type StateType { get; set; }
    }
}
