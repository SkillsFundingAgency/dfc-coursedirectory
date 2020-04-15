using System;
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

        public MptxInstanceContext CreateContext(MptxInstance instance, Type stateType, Type parentStateType)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (stateType == null)
            {
                throw new ArgumentNullException(nameof(stateType));
            }

            var contextType = parentStateType != null ?
                typeof(MptxInstanceContext<,>).MakeGenericType(stateType, parentStateType) :
                typeof(MptxInstanceContext<>).MakeGenericType(stateType);

            return (MptxInstanceContext)ActivatorUtilities.CreateInstance(
                provider: null,
                contextType,
                _stateProvider,
                instance);
        }
    }
}
