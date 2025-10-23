using System;

namespace Dfc.CourseDirectory.Core.BinaryStorageProvider
{
    public class BlobFileInfo
    {
        public string Name { get; set; }
        public long? Size { get; set; }
        public DateTimeOffset? DateUploaded { get; set; }
    }
}
