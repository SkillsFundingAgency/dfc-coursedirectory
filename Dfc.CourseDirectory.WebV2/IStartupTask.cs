using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2
{
    public interface IStartupTask
    {
        Task Execute();
    }
}
