using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public interface IInitializeMptxState<TState>
    {
        Task Initialize(TState state);
    }
}
