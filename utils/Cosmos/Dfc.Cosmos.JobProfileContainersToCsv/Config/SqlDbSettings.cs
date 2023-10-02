using System.Diagnostics.CodeAnalysis;

namespace Dfc.Cosmos.JobProfileContainersToCsv.Config
{
    public class SqlDbSettings
    {
        public static string SectionName => "SqlDb";
        [AllowNull]
        public string ConnectionString { get; set; }
    }
}
