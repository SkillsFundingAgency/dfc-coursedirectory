using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class MptxInstanceContextFactory
    {
        private readonly IMptxStateProvider _stateProvider;

        public MptxInstanceContextFactory(IMptxStateProvider stateProvider)
        {
            _stateProvider = stateProvider;
        }

        public MptxInstanceContext CreateContext(MptxInstance instance)
        {
            var contextType = typeof(MptxInstanceContext<>).MakeGenericType(instance.StateType);

            return (MptxInstanceContext)ActivatorUtilities.CreateInstance(
                provider: null,
                contextType,
                _stateProvider,
                instance);
        }
    }
}
