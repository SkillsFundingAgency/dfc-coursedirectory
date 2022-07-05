using System.Threading.Tasks;

namespace Dfc.CosmosBulkUtils.Features.Touch
{
    public interface ITouchService
    {
        Task<int> Execute(TouchOptions options);
    }
}
