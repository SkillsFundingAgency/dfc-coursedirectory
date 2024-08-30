using System.Collections.Concurrent;

public class FunctionInstanceServicesCatalog
{
    private readonly ConcurrentDictionary<Guid, IServiceProvider> _instanceServices;

    public FunctionInstanceServicesCatalog()
    {
        _instanceServices = new ConcurrentDictionary<Guid, IServiceProvider>();
    }

    public IServiceProvider GetFunctionServices(Guid instanceId)
    {
        _instanceServices.TryGetValue(instanceId, out var serviceProvider);
        return serviceProvider;
    }

    public void AddFunctionInstance(Guid instanceId, IServiceProvider serviceProvider)
    {
        _instanceServices.TryAdd(instanceId, serviceProvider);
    }

    public void RemoveFunctionInstance(Guid instanceId)
    {
        _instanceServices.TryRemove(instanceId, out _);
    }
}
