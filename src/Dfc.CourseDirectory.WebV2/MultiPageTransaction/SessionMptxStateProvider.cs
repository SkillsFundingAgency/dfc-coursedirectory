using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class SessionMptxStateProvider : IMptxStateProvider
    {
        private static readonly Encoding _encoding = Encoding.UTF8;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly ILogger<SessionMptxStateProvider> _logger;

        public SessionMptxStateProvider(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _serializerSettings = Settings.CreateSerializerSettings();
            _logger = loggerFactory.CreateLogger<SessionMptxStateProvider>();
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

            if (_httpContextAccessor.HttpContext.Session.TryGetValue(key, out var serialized) &&
                TryDeserialize(serialized, out var entry))
            {
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

            if (_httpContextAccessor.HttpContext.Session.TryGetValue(key, out var serialized) &&
                TryDeserialize(serialized, out var entry))
            {
                entry.State = update(entry.State);

                var reserialized = Serialize(entry);
                _httpContextAccessor.HttpContext.Session.Set(key, reserialized);
            }
            else
            {
                throw new ArgumentException("No valid instance with the specified ID exists.", nameof(instanceId));
            }
        }

        private static string GetSessionKey(string instanceId) => $"mtpx:{instanceId}";

        private bool TryDeserialize(byte[] bytes, out SessionEntry entry)
        {
            try
            {
                entry = JsonConvert.DeserializeObject<SessionEntry>(_encoding.GetString(bytes), _serializerSettings);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize MPTX state.");

                entry = default;
                return false;
            }
        }

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
