using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.Web.Tests.Controllers
{
    public class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new Dictionary<string, byte[]>();

        public void Set(string key, byte[] value)
        {
            _store[key] = value;
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            if (!_store.ContainsKey(key))
            {
                value = null;
                return false;
            }

            value = _store[key];
            return true;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public Task CommitAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task LoadAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            _store.Remove(key);
        }

        public string Id => throw new NotImplementedException();
        public bool IsAvailable => throw new NotImplementedException();
        public IEnumerable<string> Keys => throw new NotImplementedException();
    }
}
