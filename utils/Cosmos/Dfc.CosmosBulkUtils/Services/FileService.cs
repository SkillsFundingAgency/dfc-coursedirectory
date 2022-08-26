using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dfc.CosmosBulkUtils.Config;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dfc.CosmosBulkUtils.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;

        public FileService(ILogger<FileService> logger)
        {
            _logger = logger;
        }

        public IList<Guid> LoadRecordIds(string filename)
        {
            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
                throw new ArgumentException("Filename not defined or not found", nameof(filename));

            var listOfLines = File.ReadAllLines(filename)
                .Where(x => !string.IsNullOrWhiteSpace(x));

            var results = new List<Guid>();
            foreach (var line in listOfLines)
            {
                if (Guid.TryParse(line, out var result))
                {
                    results.Add(result);
                    _logger.LogInformation("Parsed {guid}", result);
                }
                else
                {
                    _logger.LogWarning("Failed to parse line {line}", line);
                }
            }

            if (results.Count == 0) throw new ApplicationException("Files does not contain any valid guids, aborting");

            return results;
        }

        public PatchConfig LoadPatchConfig(string filename)
        {
            var result  = JsonConvert.DeserializeObject<PatchConfig>(File.ReadAllText(filename));
            return result;
        }
    }
}
