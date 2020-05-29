using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Orleans;
using Orleans.Concurrency;
using n3q.GrainInterfaces;
using n3q.Content;
using n3q.Items;
using n3q.Aspects;

namespace n3q.Grains
{
    [StatelessWorker]
    public class ContentGeneratorGrain : Grain, IContentGenerator
    {
        public void GetTemplates(string name, Dev.NamePropertiesCollection templates, Dev.TextSet text)
        {
            DevData.GetTemplates(name, templates, text);
        }

        public void GetTemplate(string name, Dev.NamePropertiesCollection templates, Dev.TextSet text)
        {
            DevData.GetTemplate(name, templates, text);
        }

        public Task<List<string>> GetGroups()
        {
            return Task.FromResult(Dev.GetGroups());
        }

        public Task<List<string>> GetTemplates(string group)
        {
            return Task.FromResult(DevData.GetTemplates(group));
        }

        public async Task<List<string>> CreateTemplates(string name)
        {
            var translations = new Dev.TextSet();
            var templates = new Dev.NamePropertiesCollection();

            GetTemplates(name, templates, translations);
            var ids = await StoreTemplates(templates);
            await StoreTranslations(translations);

            return ids;
        }

        private async Task<List<string>> StoreTemplates(Dev.NamePropertiesCollection templates)
        {
            var templateIds = new List<string>();

            foreach (var template in templates) {
                var templateId = template.Key;
                var templateProps = template.Value;
                var item = new ItemStub(GrainFactory, templateId);
                await item.WithTransaction(async self => {
                    var oldProps = await self.GetProperties(PidSet.All, native: true);
                    var deletePids = new PidSet(oldProps.Keys.Select(pid => pid));
                    await self.ModifyProperties(PropertySet.Empty, deletePids);
                    await self.ModifyProperties(templateProps, PidSet.Empty);
                });
                templateIds.Add(templateId);
            }

            return templateIds;
        }

        public async Task StoreTranslations(Dev.TextSet translations)
        {
            foreach (var lang in Dev.Languages) {
                foreach (var pair in translations[lang]) {
                    var cacheKey = Dev.GetTranslationCacheKey(pair.Key, lang);
                    var cache = GrainFactory.GetGrain<ITranslation>(cacheKey);
                    await cache.Set(pair.Value);
                }
            }
        }
    }
}
