namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class Validity
    {
        public string LearnAimRef { get; set; }
        public string ValidityCategory { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string LastNewStartDate { get; set; }
    }
}
