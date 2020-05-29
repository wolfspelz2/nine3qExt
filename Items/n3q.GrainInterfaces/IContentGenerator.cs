using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using n3q.Items;

namespace n3q.GrainInterfaces
{
    public interface IContentGenerator : IGrainWithGuidKey
    {
        Task<List<string>> GetGroups();
        Task<List<string>> GetTemplates(string name);
        Task<List<string>> CreateTemplates(string name);
    }
}
