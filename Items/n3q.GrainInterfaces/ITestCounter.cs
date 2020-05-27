using System.Threading.Tasks;
using Orleans;

namespace n3q.GrainInterfaces
{
    public interface ITestCounter : IGrainWithStringKey
    {
        Task<long> Get();
    }
}