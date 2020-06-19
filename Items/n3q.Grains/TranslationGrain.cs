using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using n3q.StorageProviders;
using n3q.GrainInterfaces;

namespace UtilityGrains
{
    public class TranslationGrain : Grain, ITranslation
    {
        readonly IPersistentState<KeyValueStorageData> _state;
        const string TEXT = "Text";

        public TranslationGrain(
            [PersistentState("Translation", AzureKeyValueTableStorage.StorageProviderName)] IPersistentState<KeyValueStorageData> state
            )
        {
            _state = state;
        }

        public async Task Set(string data)
        {
            _state.State[TEXT] = data;
            await _state.WriteStateAsync();
        }

        public async Task<string> Get()
        {
            await Task.CompletedTask;
            if (_state.State.TryGetValue(TEXT, out var translation)) {
                return translation.ToString();
            }
            return null;
        }

        public async Task Unset()
        {
            _state.State[TEXT] = "";
            await _state.ClearStateAsync();
        }

        #region For tests

        public async Task DeletePersistentStorage()
        {
            await _state.ClearStateAsync();
        }

        public async Task ReloadPersistentStorage()
        {
            _state.State = new KeyValueStorageData();
            await _state.ReadStateAsync();
        }

        #endregion
    }
}
