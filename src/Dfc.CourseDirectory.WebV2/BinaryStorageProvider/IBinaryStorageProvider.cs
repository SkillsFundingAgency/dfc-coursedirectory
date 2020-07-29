using System.IO;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.BinaryStorageProvider
{
    public interface IBinaryStorageProvider
    {
        Task<bool> TryDownloadFile(string path, Stream destination);
        Task UploadFile(string path, Stream source);
    }
}
