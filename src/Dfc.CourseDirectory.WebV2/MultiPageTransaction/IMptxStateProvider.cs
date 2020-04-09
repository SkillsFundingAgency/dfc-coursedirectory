using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public interface IMptxStateProvider
    {
        MptxInstance CreateInstance(
            Type stateType,
            string parentInstanceId,
            object state,
            IReadOnlyDictionary<string, object> items);
        void DeleteInstance(string instanceId);
        MptxInstance GetInstance(string instanceId);
        void UpdateInstanceState(string instanceId, Func<object, object> update);
    }
}
