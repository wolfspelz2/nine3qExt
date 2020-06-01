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
using n3q.Tools;

namespace n3q.Grains
{
    [StatelessWorker]
    public class ContentGeneratorGrain : Grain, IContentGenerator
    {
        #region Interface

        public Task<List<string>> GetGroupNames()
        {
            var groupNames = new List<string> { DevSpec.AllGroupsSpecialSelector };
            groupNames.AddRange(DevSpec.GetGroups());
            return Task.FromResult(groupNames);
        }

        public Task<List<string>> GetTemplateNames(string groupSelector)
        {
            var names = new List<string> { groupSelector };
            if (groupSelector == DevSpec.AllGroupsSpecialSelector) {
                names = DevSpec.AllGroups;
            }

            var result = new List<string>();
            foreach (var name in names) {
                result.AddRange(DevData.GetTemplateNames(name));
            }

            return Task.FromResult(result);
        }

        public async Task<List<string>> CreateTemplates(string templateOrGroupSelector)
        {
            var names = new List<string> { templateOrGroupSelector };
            if (templateOrGroupSelector == DevSpec.AllGroupsSpecialSelector) {
                names = EnumUtil.GetEnumValues<DevSpec.Group>().Select(group => group.ToString()).ToList();
            }

            var result = new List<string>();
            foreach (var name in names) {
                var translations = new DevSpec.TextCollection();
                var templates = new DevSpec.TemplateCollection();
                DevData.GetTemplates(name, templates, translations);
                var templateIds = await StoreTemplates(templates);
                result.AddRange(templateIds);
                await RegisterTemplates(templateIds, DevSpec.TemplateContainer);
                await StoreTranslations(translations);
            }

            return result;
        }

        #endregion

        #region Internal

        private async Task<List<string>> StoreTemplates(DevSpec.TemplateCollection templates)
        {
            var templateIds = new List<string>();

            foreach (var template in templates) {
                var templateId = template.Key;
                var templateProps = template.Value;
                var item = new ItemStub(new OrleansGrainFactoryClient(GrainFactory, templateId));
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
            var container = new ItemStub(new OrleansGrainFactoryClient(GrainFactory, containerId));
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

        #endregion
    }
}
