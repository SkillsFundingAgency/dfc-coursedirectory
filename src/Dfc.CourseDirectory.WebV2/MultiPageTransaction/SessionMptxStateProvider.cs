using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction.Json;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class SessionMptxStateProvider : IMptxStateProvider
    {
        private static readonly Encoding _encoding = Encoding.UTF8;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerSettings _serializerSettings;

        public SessionMptxStateProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _serializerSettings = Settings.CreateSerializerSettings();
        }

        public MptxInstance CreateInstance(
            string flowName,
            IReadOnlyDictionary<string, object> items,
            object state)
        {
            var instanceId = CreateInstanceId();

            items ??= new Dictionary<string, object>();

            var entry = new SessionEntry()
            {
                FlowName = flowName,
                Items = items,
                State = state
            };
            var serialized = Serialize(entry);

            var key = GetSessionKey(instanceId);
            _httpContextAccessor.HttpContext.Session.Set(key, serialized);

            var instance = new MptxInstance(flowName, instanceId, items, state);
            return instance;
        }

        public void DeleteInstance(string instanceId)
        {
            var key = GetSessionKey(instanceId);
            _httpContextAccessor.HttpContext.Session.Remove(key);
        }

        public MptxInstance GetInstance(string instanceId)
        {
            var key = GetSessionKey(instanceId);

            if (_httpContextAccessor.HttpContext.Session.TryGetValue(key, out var serialized))
            {
                var entry = Deserialize(serialized);
                var instance = new MptxInstance(entry.FlowName, instanceId, entry.Items, entry.State);
                return instance;
            }
            else
            {
                return null;
            }
        }

        public void UpdateInstanceState(string instanceId, Func<object, object> update)
        {
            var key = GetSessionKey(instanceId);

            if (_httpContextAccessor.HttpContext.Session.TryGetValue(key, out var serialized))
            {
                var entry = Deserialize(serialized);
                entry.State = update(entry.State);

                var reserialized = Serialize(entry);
                _httpContextAccessor.HttpContext.Session.Set(key, reserialized);
            }
            else
            {
                throw new ArgumentException("No instance with the specified ID exists.", nameof(instanceId));
            }
        }

        private static string GetSessionKey(string instanceId) => $"mtpx:{instanceId}";

        private SessionEntry Deserialize(byte[] bytes) =>
            JsonConvert.DeserializeObject<SessionEntry>(_encoding.GetString(bytes), _serializerSettings);

        private byte[] Serialize(SessionEntry entry) =>
            _encoding.GetBytes(JsonConvert.SerializeObject(entry, _serializerSettings));

        private string CreateInstanceId()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var idBytes = new byte[6];

            using var randomGenerator = RandomNumberGenerator.Create();
            while (true)
            {
                randomGenerator.GetBytes(idBytes);
                var instanceId = Convert.ToBase64String(idBytes);

                var key = GetSessionKey(instanceId);
                if (!session.Keys.Contains(key))
                {
                    return instanceId;
                }
            }
        }

        private class SessionEntry
        {
            public string FlowName { get; set; }
            public IReadOnlyDictionary<string, object> Items { get; set; }
            public object State { get; set; }
        }
    }
}
