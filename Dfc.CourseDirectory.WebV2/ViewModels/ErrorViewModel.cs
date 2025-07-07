namespace Dfc.CourseDirectory.WebV2.ViewModels
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string ErrorMessage { get; set; }
        public string ErrorPath { get; set; }
    }
}
