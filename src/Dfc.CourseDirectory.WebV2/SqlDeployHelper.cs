using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.SqlServer.Dac;

namespace Dfc.CourseDirectory.WebV2
{
    public class SqlDeployHelper
    {
        public void Deploy(string connectionString, Action<string> writeMessage)
        {
            var dacServices = new DacServices(connectionString);

            dacServices.ProgressChanged += DacServices_ProgressChanged;

            try
            {
                var dacpacLocation = Path.GetFullPath(Path.Combine(
                    Environment.CurrentDirectory,
                    "../../../../../src/Dfc.CourseDirectory.Database/bin",
#if DEBUG
                    "Debug",
#else
                    "Release",
#endif
                    "Dfc.CourseDirectory.Database.dacpac"));

                using var dacpac = DacPackage.Load(dacpacLocation);

                var databaseName = GetDatabaseNameFromConnectionString();

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

            void DacServices_ProgressChanged(object sender, DacProgressEventArgs e) => writeMessage?.Invoke(e.Message);

            string GetDatabaseNameFromConnectionString() =>
                new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        }
    }
}
