namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class PublishResult
    {
        private PublishResult()
        {
        }

        public PublishResultStatus Status { get; private set; }
        public int PublishedCount { get; private set; }

        public static PublishResult Success(int publishedCount) => new PublishResult()
        {
            Status = PublishResultStatus.Success,
            PublishedCount = publishedCount
        };

        public static PublishResult UploadHasErrors() => new PublishResult()
        {
            Status = PublishResultStatus.UploadHasErrors
        };
    }

    public enum PublishResultStatus
    {
        Success,
        UploadHasErrors
    }
}
