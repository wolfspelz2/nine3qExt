using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Orleans;
using Orleans.Concurrency;
using n3q.GrainInterfaces;
using n3q.Content;
using n3q.Items;
using n3q.Aspects;
using System;

namespace n3q.Grains
{
    [StatelessWorker]
    public class ContentGeneratorGrain : Grain, IContentGenerator
    {
        #region Interface

        public Task<List<string>> GetGroups()
        {
            return Task.FromResult(DevSpec.GetGroups());
        }

        public Task<List<string>> GetTemplates(string group)
        {
            return Task.FromResult(DevData.GetTemplates(group));
        }

        public async Task<List<string>> CreateTemplates(string name)
        {
            var translations = new DevSpec.TextCollection();
            var templates = new DevSpec.TemplateCollection();

            GetTemplates(name, templates, translations);
            var templateIds = await StoreTemplates(templates);
            await RegisterTemplates(templateIds, DevSpec.TemplateContainer);
            await StoreTranslations(translations);

            return templateIds;
        }

        #endregion

        public void GetTemplates(string name, DevSpec.TemplateCollection templates, DevSpec.TextCollection text)
        {
            DevData.GetTemplates(name, templates, text);
        }

        public void GetTemplate(string name, DevSpec.TemplateCollection templates, DevSpec.TextCollection text)
        {
            DevData.GetTemplate(name, templates, text);
        }

        private async Task<List<string>> StoreTemplates(DevSpec.TemplateCollection templates)
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

        public async Task StoreTranslations(DevSpec.TextCollection translations)
        {
            foreach (var lang in DevSpec.Languages) {
                foreach (var pair in translations[lang]) {
                    var cacheKey = DevSpec.GetTranslationCacheKey(pair.Key, lang);
                    var cache = GrainFactory.GetGrain<ITranslation>(cacheKey);
                    await cache.Set(pair.Value);
                }
            }
        }

        private async Task RegisterTemplates(List<string> templateIds, string containerId)
        {
            var container = new ItemStub(GrainFactory, containerId);
            await container.WithTransaction(async self => {
                await self.Set(Pid.ContainerAspect, true);
            });
            await container.WithTransaction(async self => {
                foreach (var id in templateIds) {
                    var child = await self.Item(id);
                    await self.AsContainer().AddChild(child);
                }
            });
        }

    }
}
