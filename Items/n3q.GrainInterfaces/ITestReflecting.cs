using System.Threading.Tasks;
using Orleans;

namespace n3q.GrainInterfaces
{
    public interface ITestReflecting : IGrainWithStringKey
    {
        Task SetString(string value);
        Task SetLong(long value);
        Task SetDouble(double value);
        Task SetBool(bool value);

        Task<string> GetString();
        Task<long> GetLong();
        Task<double> GetDouble();
        Task<bool> GetBool();

        Task DeletePersistentStorage();
        Task Deactivate();
    }
}