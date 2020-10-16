using System.Collections.Generic;
using FormFlow.State;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    /// <summary>
    /// An in-memory store for FormFlow instances used by tests.
    /// </summary>
    /// <remarks>
    /// This allows persisting instance state outside of an HTTP request.
    /// </remarks>
    public class TestUserInstanceStateStore : IUserInstanceStateStore
    {
        private readonly Dictionary<string, byte[]> _data;

        public TestUserInstanceStateStore()
        {
            _data = new Dictionary<string, byte[]>();
        }

        public void Clear() => _data.Clear();

        public void DeleteState(string key) => _data.Remove(key);

        public void SetState(string key, byte[] data) => _data[key] = data;

        public bool TryGetState(string key, out byte[] data) => _data.TryGetValue(key, out data);
    }
}
