using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using nine3q.GrainInterfaces;
using nine3q.Items;
using nine3q.Content;

namespace nine3q.Grains
{
    public class ContentGeneratorGrain : Grain, IContentGenerator
    {
        string Name { get; set; }

        public override Task OnActivateAsync()
        {
            Name = this.GetPrimaryKeyString();
            return Task.CompletedTask;
        }

        public void GetTemplates(string name, NamePropertiesCollection templates, BasicDefinition.TextSet text)
        {
            BasicData.GetTemplates(name, templates, text);
        }

        public void GetTemplate(string name, NamePropertiesCollection templates, BasicDefinition.TextSet text)
        {
            BasicData.GetTemplate(name, templates, text);
        }

        public Task<List<string>> GetGroups()
        {
            return Task.FromResult(BasicData.GetGroups());
        }

        public Task<List<string>> GetTemplates(string group)
        {
            return Task.FromResult(BasicData.GetTemplates(group));
        }

        public async Task<string> CreateTemplates(string name)
        {
            var translations = new BasicDefinition.TextSet();
            var templates = new NamePropertiesCollection();

            GetTemplates(name, templates, translations);
            ItemIdSet ids = await StoreTemplates(templates);
            await StoreTranslations(translations);

            return ids.ToString();
        }

        private async Task<ItemIdSet> StoreTemplates(NamePropertiesCollection templates)
        {
            GrainFactory.GetGrain<IInventory>(Name).SetStreamNamespace(InventoryService.StreamNamespaceTemplates).Wait();

            var ids = new ItemIdSet();
            var inv = GrainFactory.GetGrain<IInventory>(Name);
            foreach (var template in templates) {
                var templateName = template.Key;
                var templateProps = template.Value;
                var id = await inv.GetItemByName(templateName);
                if (id == ItemId.NoItem) {
                    id = await inv.CreateItem(templateProps);
                } else {
                    var oldProps = await inv.GetItemProperties(id, PidList.All, native: true);
                    oldProps.Remove(Pid.Id);
                    var newProps = templateProps;
                    var deletedProps = new PidList();
                    foreach (var pair in oldProps) {
                        if (!newProps.ContainsKey(pair.Key)) {
                            deletedProps.Add(pair.Key);
                        }
                    }
                    await inv.ModifyItemProperties(id, newProps, deletedProps);
                }
                ids.Add(id);
            }

            return ids;
        }

        public async Task StoreTranslations(BasicDefinition.TextSet translations)
        {
            foreach (var lang in BasicDefinition.Languages) {
                foreach (var pair in translations[lang]) {
                    var cacheKey = BasicDefinition.GetTranslationCacheKey(pair.Key, lang);
                    var cache = GrainFactory.GetGrain<ITranslation>(cacheKey);
                    await cache.Set(pair.Value);
                }
            }
        }
    }
}
