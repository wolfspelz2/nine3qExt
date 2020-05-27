using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace nine3q.GrainInterfaces
{
    public interface IContentGenerator : IGrainWithStringKey
    {
        Task<List<string>> GetGroups();
        Task<List<string>> GetTemplates(string name);
        Task<string> CreateTemplates(string name);
    }
}
