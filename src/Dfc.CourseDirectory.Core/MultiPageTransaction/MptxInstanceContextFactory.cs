using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Core.MultiPageTransaction
{
    public class MptxInstanceContextFactory
    {
        private readonly IMptxStateProvider _stateProvider;
        private readonly IServiceProvider _serviceProvider;

        public MptxInstanceContextFactory(IMptxStateProvider stateProvider, IServiceProvider serviceProvider)
        {
            _stateProvider = stateProvider;
            _serviceProvider = serviceProvider;
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
                _serviceProvider,
                contextType,
                _stateProvider,
                instance);
        }
    }
}
