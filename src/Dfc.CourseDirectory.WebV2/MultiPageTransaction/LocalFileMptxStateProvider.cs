#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction.Json;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class LocalFileMptxStateProvider : IMptxStateProvider
    {
        private readonly string _dbFilePath;
        private readonly JsonSerializerSettings _serializerSettings;

        public LocalFileMptxStateProvider()
        {
            _dbFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CourseDirectory.dev",
                "state.json");

            EnsureDataFolderExists(_dbFilePath);

            _serializerSettings = Settings.CreateSerializerSettings();
        }

        public MptxInstance CreateInstance(string flowName, IReadOnlyDictionary<string, object> items, object state)
        {
            var instanceId = CreateInstanceId();

            items ??= new Dictionary<string, object>();

            var entry = new DbFileEntry()
            {
                FlowName = flowName,
                Items = items,
                State = state
            };

            UpdateDbFile(dbFile => dbFile.Entries.Add(instanceId, entry));

            var instance = new MptxInstance(flowName, instanceId, items, state);
            return instance;
        }

        public void DeleteInstance(string instanceId) =>
            UpdateDbFile(dbFile => dbFile.Entries.Remove(instanceId));

        public MptxInstance GetInstance(string instanceId)
        {
            DbFileEntry entry = null;
            UpdateDbFile(dbFile => dbFile.Entries.TryGetValue(instanceId, out entry));

            if (entry == null)
            {
                return null;
            }

            return new MptxInstance(entry.FlowName, instanceId, entry.Items, entry.State);
        }

        public void UpdateInstanceState(string instanceId, Func<object, object> update) =>
            UpdateDbFile(dbFile => update(dbFile.Entries[instanceId].State));

        private static string CreateInstanceId() => Guid.NewGuid().ToString();

        private static void EnsureDataFolderExists(string dbFilePath)
        {
            var folder = Path.GetDirectoryName(dbFilePath);
            
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        private void UpdateDbFile(Action<DbFile> updateDb)
        {
            DbFile dbFile = null;

            if (File.Exists(_dbFilePath))
            {
                try
                {
                    dbFile = JsonConvert.DeserializeObject<DbFile>(File.ReadAllText(_dbFilePath), _serializerSettings);
                }
                catch (JsonSerializationException)
                {
                }
            }

            if (dbFile == null)
            {
                dbFile = new DbFile() { Entries = new Dictionary<string, DbFileEntry>() };
            }

            updateDb(dbFile);

            File.WriteAllText(_dbFilePath, JsonConvert.SerializeObject(dbFile, _serializerSettings));
        }

        private class DbFile
        {
            public Dictionary<string, DbFileEntry> Entries { get; set; }
        }

        private class DbFileEntry
        {
            public string FlowName { get; set; }
            public IReadOnlyDictionary<string, object> Items { get; set; }
            public object State { get; set; }
        }
    }
}
#endif
