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
            Type stateType,
            string parentInstanceId,
            object state,
            IReadOnlyDictionary<string, object> items)
        {
            object parentState = null;
            Type parentStateType = null;

            if (parentInstanceId != null)
            {
                var parentEntry = GetEntry(GetSessionKey(parentInstanceId));

                if (parentEntry == null)
                {
                    throw new InvalidParentException(parentInstanceId);
                }

                parentState = parentEntry.State;
                parentStateType = parentEntry.StateType;
            }

            var instanceId = CreateInstanceId();

            items ??= new Dictionary<string, object>();

            var entry = new SessionEntry()
            {
                StateType = stateType,
                Items = items,
                State = state,
                ParentInstanceId = parentInstanceId
            };
            var serialized = Serialize(entry);

            var key = GetSessionKey(instanceId);
            _httpContextAccessor.HttpContext.Session.Set(key, serialized);

            return new MptxInstance(
                instanceId,
                stateType,
                state,
                parentInstanceId,
                parentStateType,
                parentState,
                items);
        }

        public void DeleteInstance(string instanceId)
        {
            var key = GetSessionKey(instanceId);
            _httpContextAccessor.HttpContext.Session.Remove(key);
        }

        public MptxInstance GetInstance(string instanceId)
        {
            var key = GetSessionKey(instanceId);
            var entry = GetEntry(key);

            if (entry != null)
            {
                object parentState = null;
                Type parentStateType = null;

                if (entry.ParentInstanceId != null)
                {
                    var parentEntry = GetEntry(GetSessionKey(entry.ParentInstanceId));

                    parentState = parentEntry.State;
                    parentStateType = parentEntry.StateType;
                }

                return new MptxInstance(
                    instanceId,
                    entry.StateType,
                    entry.State,
                    entry.ParentInstanceId,
                    parentStateType,
                    parentState,
                    entry.Items);
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

        private SessionEntry GetEntry(string key)
        {
            if(_httpContextAccessor.HttpContext.Session.TryGetValue(key, out var serialized) &&
                TryDeserialize(serialized, out var entry))
            {
                return entry;
            }
            else
            {
                return null;
            }
        }

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

        private class SessionEntry
        {
            public Type StateType { get; set; }
            public IReadOnlyDictionary<string, object> Items { get; set; }
            public object State { get; set; }
            public string ParentInstanceId { get; set; }
        }
    }
}
