namespace Dfc.CourseDirectory.Core.MultiPageTransaction
{
    public interface IMptxController<TState>
        where TState : IMptxState
    {
        MptxInstanceContext<TState> Flow { get; set; }
    }
}
