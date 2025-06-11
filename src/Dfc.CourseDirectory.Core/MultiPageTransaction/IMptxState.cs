namespace Dfc.CourseDirectory.Core.MultiPageTransaction
{
    public interface IMptxState
    {
    }

    public interface IMptxState<out TParentState> : IMptxState
        where TParentState : IMptxState
    {
    }
}
