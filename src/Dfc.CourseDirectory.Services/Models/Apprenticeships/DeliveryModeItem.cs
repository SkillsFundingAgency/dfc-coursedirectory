namespace Dfc.CourseDirectory.Services.Models.Apprenticeships
{
    public class DeliveryModeItem
    {
        public int DeliveryModeId { get; set; }
        public string DeliveryModeName { get; set; }
        public string DeliveryModeDescription { get; set; }
        public string DASRef { get; set; }
        public bool MustHaveFullLocation { get; set; }
    }
}
