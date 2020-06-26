using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using n3q.StorageProviders;
using n3q.GrainInterfaces;

namespace UtilityGrains
{
    [Serializable]
    public class TranslationState
    {
        public string Text;
    }

    public class TranslationGrain : Grain, ITranslation
    {
        readonly IPersistentState<TranslationState> _state;

        public TranslationGrain(
            [PersistentState("Translation", AzureReflectingTableStorage.StorageProviderName)] IPersistentState<TranslationState> state
            )
        {
            _state = state;
        }

        public async Task Set(string data)
        {
            _state.State.Text = data;
            await _state.WriteStateAsync();
        }

        public async Task<string> Get()
        {
            await Task.CompletedTask;
            return _state.State.Text;
        }

        public async Task Unset()
        {
            _state.State.Text = null;
            await _state.ClearStateAsync();
        }

        #region For tests

        public async Task DeletePersistentStorage()
        {
            await _state.ClearStateAsync();
        }

        public async Task ReloadPersistentStorage()
        {
            _state.State = new TranslationState();
            await _state.ReadStateAsync();
        }

        #endregion
    }
}
