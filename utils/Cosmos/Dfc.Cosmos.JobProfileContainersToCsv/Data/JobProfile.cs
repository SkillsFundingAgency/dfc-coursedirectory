using System.Diagnostics.CodeAnalysis;

namespace Dfc.Cosmos.JobProfileContainersToCsv.Data
{
    public class JobProfile
    {
        [AllowNull]
        public string DocumentId { get; set; }
        [AllowNull]
        public string Id { get; set; }
        [AllowNull]
        public string Title { get; set; }
        [AllowNull]
        public string Categories { get; set; }
    }
}
