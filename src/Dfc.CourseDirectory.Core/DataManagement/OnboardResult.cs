namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class OnboardResult
    {
        private OnboardResult()
        {
        }

        public OnboardResultStatus Status { get; private set; }
        public int OnboardCount { get; private set; }

        public static OnboardResult Success(int onboardCount) => new OnboardResult()
        {
            Status = OnboardResultStatus.Success,
            OnboardCount = onboardCount
        };

        public static OnboardResult UploadHasErrors() => new OnboardResult()
        {
            Status = OnboardResultStatus.UploadHasErrors
        };
    }
    public enum OnboardResultStatus
    {
        Success,
        UploadHasErrors
    }
}
