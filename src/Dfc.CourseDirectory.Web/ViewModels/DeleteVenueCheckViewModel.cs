namespace Dfc.CourseDirectory.Web.ViewModels
{
    public class DeleteVenueCheckViewModel
    {
        public bool LiveCoursesExist { get; set; }
        public bool PendingCoursesExist { get; set; }
        public bool LiveApprenticeshipsExist { get; set; }
        public bool LiveTLevelsExist { get; set; }
    }
}
