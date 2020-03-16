using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class SessionMptxStateProvider : IMptxStateProvider
    {
        private static Encoding _encoding = Encoding.UTF8;

        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.None,
            TypeNameHandling = TypeNameHandling.All
        };

        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionMptxStateProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public MptxInstance CreateInstance(string flowName, IReadOnlyDictionary<string, object> items)
        {
            var instanceId = CreateInstanceId();

            var entry = new SessionEntry()
            {
                FlowName = flowName,
                Items = items,
                State = null
            };
            var serialized = Serialize(entry);

            var key = GetSessionKey(instanceId);
            _httpContextAccessor.HttpContext.Session.Set(key, serialized);

            var instance = new MptxInstance(flowName, instanceId, items, null);
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

        private static string CreateInstanceId() => Guid.NewGuid().ToString("N");

        private static SessionEntry Deserialize(byte[] bytes) =>
            JsonConvert.DeserializeObject<SessionEntry>(_encoding.GetString(bytes), _serializerSettings);

        private static string GetSessionKey(string instanceId) => $"mtpx:{instanceId}";

        private static byte[] Serialize(SessionEntry entry) =>
            _encoding.GetBytes(JsonConvert.SerializeObject(entry, _serializerSettings));

        private class SessionEntry
        {
            public string FlowName { get; set; }
            public IReadOnlyDictionary<string, object> Items { get; set; }
            public object State { get; set; }
        }
    }
}
