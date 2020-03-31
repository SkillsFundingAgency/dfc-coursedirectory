namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public interface IMptxController<TState>
        where TState : IMptxState
    {
        MptxInstanceContext<TState> Flow { get; set; }
    }
}
