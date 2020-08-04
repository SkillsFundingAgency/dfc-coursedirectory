using System.IO;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Core.BinaryStorageProvider
{
    public interface IBinaryStorageProvider
    {
        Task<bool> TryDownloadFile(string path, Stream destination);
        Task UploadFile(string path, Stream source);
    }
}
