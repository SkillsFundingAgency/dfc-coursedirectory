using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public interface IInitializeMptxState<TState>
        where TState : IMptxState
    {
        Task Initialize(MptxInstanceContext<TState> context);
    }
}
