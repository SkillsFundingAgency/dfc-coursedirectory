namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class ProviderUploadResultSummary
    {
        public int Total { get; set; }
        public int NewProviders { get; set; }
        public int ChangeToStatus { get; set; }
        public int ChangeToType { get; set; }
        public int ChangeToStatusAndType { get; set; }
    }
}
