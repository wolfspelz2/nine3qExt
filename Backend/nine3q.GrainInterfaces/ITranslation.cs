using System.Threading.Tasks;
using Orleans;

namespace nine3q.GrainInterfaces
{
    public interface ITranslation : IGrainWithStringKey
    {
        Task Set(string s);
        Task<string> Get();
        Task Unset();

        Task DeletePersistentStorage();
        Task ReloadPersistentStorage();
    }
}
