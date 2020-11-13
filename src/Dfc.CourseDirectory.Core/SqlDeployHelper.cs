using System;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using Microsoft.SqlServer.Dac;

namespace Dfc.CourseDirectory.Core
{
    public class SqlDeployHelper
    {
        public void Deploy(string connectionString, Action<string> writeMessage)
        {
            var runningInCi = Environment.GetEnvironmentVariable("TF_BUILD") != null;

            var dacpacLocation = Path.GetFullPath(Path.Combine(
                Environment.CurrentDirectory,
                "../../../../../src/Dfc.CourseDirectory.Database/bin",
#if DEBUG
                "Debug",
#else
                "Release",
#endif
                "Dfc.CourseDirectory.Database.dacpac"));

            var databaseName = GetDatabaseNameFromConnectionString();

            if (!runningInCi)
            {
                var dacpacHash = ComputeDacpacHash();
                var schemaHashFileName = GetSchemaHashFileName();

                // Check the hash of the last deployed database schema.
                // If it matches this one we can skip the deployment.
                if (TryGetLastDeployedDacpacHash(out var hash) && hash == dacpacHash)
                {
                    writeMessage("DACPAC is unchanged - skipping deployment");
                    return;
                }

                DeployCore();

                WriteDacpacHashToCacheFile();

                string ComputeDacpacHash()
                {
                    using (var fs = File.OpenRead(dacpacLocation))
                    using (var hashAlgo = SHA512.Create())
                    {
                        return Convert.ToBase64String(hashAlgo.ComputeHash(fs));
                    }
                }

                string GetSchemaHashFileName() =>
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "CourseDirectory",
                        $"{databaseName}.sqlschemahash");

                bool TryGetLastDeployedDacpacHash(out string hash)
                {
                    if (File.Exists(schemaHashFileName))
                    {
                        hash = File.ReadAllText(schemaHashFileName);
                        return true;
                    }
                    else
                    {
                        hash = default;
                        return false;
                    }
                }

                void WriteDacpacHashToCacheFile()
                {
                    var schemaHashFileDirectory = Path.GetDirectoryName(schemaHashFileName);
                    Directory.CreateDirectory(schemaHashFileDirectory);

                    File.WriteAllText(schemaHashFileName, dacpacHash);
                }
            }
            else
            {
                DeployCore();
            }

            void DeployCore()
            {
                var dacServices = new DacServices(connectionString);

                try
                {
                    dacServices.ProgressChanged += DacServices_ProgressChanged;

                    using var dacpac = DacPackage.Load(dacpacLocation);

                    dacServices.Deploy(
                        dacpac,
                        databaseName,
                        upgradeExisting: true,
                        options: new DacDeployOptions()
                        {
                            BlockOnPossibleDataLoss = false
                        });
                }
                finally
                {
                    dacServices.ProgressChanged -= DacServices_ProgressChanged;
                }
            }

            void DacServices_ProgressChanged(object sender, DacProgressEventArgs e) => writeMessage?.Invoke(e.Message);

            string GetDatabaseNameFromConnectionString() =>
                new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        }
    }
}
