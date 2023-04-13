using System.Threading.Tasks;

namespace Dfc.CosmosBulkUtils.Features.Delete
{
    public interface IDeleteService
    {
        Task<int> Execute(string filename);
    }
}
