using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    /// <summary>
    /// A dictionary-backed implementation of <see cref="ISession" /> for testing.
    /// </summary>
    /// <remarks>
    /// This allows us to query/amend the session data outside of an HTTP request.
    /// </remarks>
    public class SingletonSession : ISession
    {
        private readonly Dictionary<string, byte[]> _data;

        public SingletonSession()
        {
            _data = new Dictionary<string, byte[]>();
        }

        public string Id => "1";

        public bool IsAvailable => true;

        public IEnumerable<string> Keys => _data.Keys;

        public void Clear() => _data.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => Remove(key);

        public void Set(string key, byte[] value) => _data[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _data.TryGetValue(key, out value);
    }

    public class SingletonSessionStore : ISessionStore
    {
        public SingletonSession Instance { get; } = new SingletonSession();

        public ISession Create(
            string sessionKey,
            TimeSpan idleTimeout,
            TimeSpan ioTimeout,
            Func<bool> tryEstablishSession,
            bool isNewSessionKey)
        {
            return Instance;
        }
    }
}
