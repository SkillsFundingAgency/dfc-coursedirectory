using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public interface IMptxStateProvider
    {
        MptxInstance CreateInstance(string flowName, IReadOnlyDictionary<string, object> items, object state);
        void DeleteInstance(string instanceId);
        MptxInstance GetInstance(string instanceId);
        void UpdateInstanceState(string instanceId, Func<object, object> update);
    }
}
