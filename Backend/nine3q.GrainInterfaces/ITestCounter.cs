using System.Threading.Tasks;
using Orleans;

namespace nine3q.GrainInterfaces
{
    public interface ITestCounter : IGrainWithStringKey
    {
        Task<long> Get();
    }
}