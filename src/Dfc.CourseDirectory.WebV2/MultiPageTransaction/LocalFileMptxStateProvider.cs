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

        public MptxInstance CreateInstance(Type stateType, IReadOnlyDictionary<string, object> items, object state)
        {
            var instanceId = CreateInstanceId();

            items ??= new Dictionary<string, object>();

            var entry = new DbFileEntry()
            {
                StateType = stateType,
                Items = items,
                State = state
            };

            UpdateDbFile(dbFile => dbFile.Entries.Add(instanceId, entry));

            var instance = new MptxInstance(stateType, instanceId, items, state);
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

            return new MptxInstance(entry.StateType, instanceId, entry.Items, entry.State);
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

            var serializer = JsonSerializer.Create(_serializerSettings);
            serializer.Error += Serializer_Error;

            try
            {
                if (File.Exists(_dbFilePath))
                {
                    using (var streamReader = File.OpenText(_dbFilePath))
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        dbFile = serializer.Deserialize<DbFile>(jsonReader);
                    }
                }

                // Maybe the outermost object failed to deserialize (or file didn't exist)
                dbFile ??= new DbFile();
                dbFile.Entries ??= new Dictionary<string, DbFileEntry>();

                // If serializing any entries failed they will be null - remove them
                foreach (var e in dbFile.Entries)
                {
                    if (e.Value == null)
                    {
                        dbFile.Entries.Remove(e.Key);
                    }
                }

                updateDb(dbFile);

                using (var streamWriter = File.CreateText(_dbFilePath))
                using (var jsonWriter = new JsonTextWriter(streamWriter))
                {
                    serializer.Serialize(jsonWriter, dbFile);
                }
            }
            finally
            {
                serializer.Error -= Serializer_Error;
            }

            void Serializer_Error(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e) =>
                e.ErrorContext.Handled = true;
        }

        private class DbFile
        {
            public Dictionary<string, DbFileEntry> Entries { get; set; }
        }

        private class DbFileEntry
        {
            public Type StateType { get; set; }
            public IReadOnlyDictionary<string, object> Items { get; set; }
            public object State { get; set; }
        }
    }
}
#endif
