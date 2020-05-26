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
        public string Data;
    }

    public class TranslationGrain : Grain, ITranslation
    {
        private readonly IPersistentState<TranslationState> _state;

        public TranslationGrain(
            [PersistentState("Translation", JsonFileStorage.StorageProviderName)] IPersistentState<TranslationState> state
            )
        {
            _state = state;
        }

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
            await _state.ReadStateAsync();
        }

        public async Task Set(string data)
        {
            _state.State.Data = data;
                await _state.WriteStateAsync();
        }

        public async Task<string> Get()
        {
            await Task.CompletedTask;
            return _state.State.Data;
        }

        public async Task Unset()
        {
            _state.State.Data = "";
            await _state.ClearStateAsync();
        }

        #region For tests

        public async Task DeletePersistentStorage()
        {
            await _state.ClearStateAsync();
        }

        public async Task ReloadPersistentStorage()
        {
            await _state.ReadStateAsync();
        }

        #endregion
    }
}
