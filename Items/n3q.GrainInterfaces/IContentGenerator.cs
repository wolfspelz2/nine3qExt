using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using n3q.Items;

namespace n3q.GrainInterfaces
{
    public interface IContentGenerator : IGrainWithGuidKey
    {
        Task<List<string>> GetGroupNames();
        Task<List<string>> GetTemplateNames(string groupName);
        Task<List<string>> CreateTemplates(string templateOrGroupName);
    }
}
