namespace Dfc.CourseDirectory.Core.DataStore.Sql.Models
{
    public class PostcodeInfo
    {
        public string Postcode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool InEngland { get; set; }
    }
}
