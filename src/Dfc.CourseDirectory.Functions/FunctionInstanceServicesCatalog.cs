using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Executors;

namespace Dfc.CourseDirectory.Functions
{
    public class FunctionInstanceServicesCatalog : IJobActivatorEx
    {
        private readonly ConditionalWeakTable<IFunctionInstanceEx, IServiceProvider> _instanceServices;
        private readonly IJobActivatorEx _innerActivator;

        public FunctionInstanceServicesCatalog(IJobActivator innerActivator)
        {
            _instanceServices = new ConditionalWeakTable<IFunctionInstanceEx, IServiceProvider>();
            _innerActivator = (IJobActivatorEx)innerActivator;
        }

        public IServiceProvider GetFunctionServices(Guid instanceId) =>
            _instanceServices.SingleOrDefault(k => k.Key.Id == instanceId).Value;

        T IJobActivatorEx.CreateInstance<T>(IFunctionInstanceEx functionInstance)
        {
            _instanceServices.Add(functionInstance, functionInstance.InstanceServices);
            return _innerActivator.CreateInstance<T>(functionInstance);
        }

        T IJobActivator.CreateInstance<T>() => _innerActivator.CreateInstance<T>();
    }
}
