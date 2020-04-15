namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public interface IMptxState
    {
    }

    public interface IMptxState<out TParentState> : IMptxState
        where TParentState : IMptxState
    {
    }
}
