using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Orleans;
using Orleans.Storage;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.Serialization;
using System.Threading;
using Orleans.Runtime.Configuration;
using Microsoft.Azure.Cosmos.Table;
using System.Threading.Tasks.Sources;
using Microsoft.OData.UriParser;
using System.Collections.Generic;
using n3q.Items;
using n3q.Tools;
using n3q.GrainInterfaces;

namespace n3q.StorageProviders
{
    public static class ItemAzureTableStorage
    {
        public const string StorageProviderName = "ItemAzureTableStorage";
    }

    public class ItemAzureTableStorageOptions
    {
        public string ConnectionString = "DataConnectionString";
        public string TableName = "Grains";

        public int InitStage { get; set; } = DEFAULT_INIT_STAGE;
        public const int DEFAULT_INIT_STAGE = ServiceLifecycleStage.ApplicationServices;
    }

    public class ItemAzureTableStorageProvider : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        readonly string _name;
        readonly ItemAzureTableStorageOptions _options;
        readonly ILogger _logger;
        readonly ILoggerFactory _loggerFactory;
        readonly IGrainFactory _grainFactory;
        readonly long _id;
        static long _counter;
        string _connectionString;
        string _tableName;
        CloudTable _table;

        public ItemAzureTableStorageProvider(string name,
            ItemAzureTableStorageOptions options,
            IGrainFactory grainFactory,
            ITypeResolver typeResolver,
            ILoggerFactory loggerFactory
            )
        {
            _name = name;
            _options = options;
            _grainFactory = grainFactory;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger($"{typeof(ItemAzureTableStorageProvider).FullName}.{name}");
            _id = Interlocked.Increment(ref _counter);
        }

        #region Interface

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            try {
                var pk = KeySafeName(GetPrimaryKey(grainType, grainReference, grainState));
                var rk = KeySafeName(grainType);
                _logger.LogInformation($"Writing item={grainReference.GetPrimaryKeyString()} GrainType={grainType} Pk={pk} Rk={rk} Table={_tableName}");
                var itemState = grainState.State as ItemState;
                var entityProperties = ItemProperties2EntityProperties(itemState.Properties);
                var entity = new DynamicTableEntity(pk, rk, grainState.ETag, entityProperties);
                await GetTable().ExecuteAsync(TableOperation.InsertOrReplace(entity));
            } catch (Exception ex) {
                _logger.Error(0, $"Error writing item={grainReference.GetPrimaryKeyString()}: {ex.Message}");
                throw;
            }
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            try {
                var pk = KeySafeName(GetPrimaryKey(grainType, grainReference, grainState));
                var rk = KeySafeName(grainType);
                _logger.LogInformation($"Reading item={grainReference.GetPrimaryKeyString()} GrainType={grainType} Pk={pk} Rk={rk} Table={_tableName}");
                var res = await GetTable().ExecuteAsync(TableOperation.Retrieve<DynamicTableEntity>(pk, rk));
                if (res.Result == null) { return; }
                var entity = res.Result as DynamicTableEntity;
                if (entity == null) { return; }
                var entityProperties = entity.Properties;
                var itemProperties = EntityProperties2ItemProperties(entityProperties);
                var itemState = grainState.State as ItemState;
                itemState.Properties = itemProperties;
                grainState.ETag = entity.ETag;
            } catch (Exception ex) {
                _logger.Error(0, $"Error reading: {ex.Message}");
                throw;
            }
        }

        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            try {
                //if (fileInfo.Exists) {
                //    fileInfo.Delete();
                //}
            } catch (Exception ex) {
                _logger.Error(0, $"Error deleting: {ex.Message}");
                throw;
            }
            return Task.CompletedTask;
        }

        #endregion

        #region Internal

        IDictionary<string, EntityProperty> ItemProperties2EntityProperties(Dictionary<Pid, string> itemProps)
        {
            var entityProps = new Dictionary<string, EntityProperty>();

            foreach (var pair in itemProps) {
                var prop = Property.GetDefinition(pair.Key).Storage switch
                {
                    Property.Storage.Int => new EntityProperty(pair.Value.ToLong()),
                    Property.Storage.Float => new EntityProperty(pair.Value.ToDouble()),
                    Property.Storage.Bool => new EntityProperty(pair.Value.IsTrue()),
                    _ => new EntityProperty(pair.Value),
                };
                entityProps.Add(pair.Key.ToString(), prop);
            }

            return entityProps;
        }

        Dictionary<Pid, string> EntityProperties2ItemProperties(IDictionary<string, EntityProperty> entityProps)
        {
            var itemProps = new Dictionary<Pid, string>();

            foreach (var pair in entityProps) {
                var pid = pair.Key.ToEnum(Pid.Unknown);
                if (pid != Pid.Unknown) {
                    itemProps.Add(pid, pair.Value.ToString());
                }
            }

            return itemProps;
        }

        protected static string GetPrimaryKey(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            grainReference.GetPrimaryKey(out string primaryKey);
            return primaryKey;
        }

        static char[] KeyInvalidChars = { '/', '\\', '#', '?', '|', '[', ']', '{', '}', '<', '>', '$', '^', '&', '%', '+', '\'' };
        protected string KeySafeName(string name)
        {
            var safeName = new StringBuilder();
            for (var i = 0; i < name.Length; i++) {
                if (KeyInvalidChars.Contains(name[i])) {
                    safeName.Append(Convert.ToInt32(name[i]).ToString("X2"));
                } else {
                    safeName.Append(name[i]);
                }
            }
            return safeName.ToString();
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(OptionFormattingUtilities.Name<ItemAzureTableStorageProvider>(_name), _options.InitStage, Init);
        }

        private Task Init(CancellationToken ct)
        {
            _connectionString = _options.ConnectionString;
            _tableName = _options.ConnectionString;

            _logger.LogInformation($"Init: name={this._name} table={_tableName} connectionString={ConfigUtilities.RedactConnectionStringInfo(_connectionString)}");

            GetTable().CreateIfNotExists();

            return Task.CompletedTask;
        }

        protected CloudTable GetTable()
        {
            if (_table == null) {
                var storageAccount = CloudStorageAccount.Parse(_connectionString);
                var tableClient = storageAccount.CreateCloudTableClient();
                _table = tableClient.GetTableReference(_tableName);
            }

            return _table;
        }

        #endregion
    }

    #region Provider registration

    public class ItemAzureTableStorageOptionsValidator : IConfigurationValidator
    {
        public ItemAzureTableStorageOptionsValidator(ItemAzureTableStorageOptions options, string name) { }
        public void ValidateConfiguration() { }
    }

    public static class ItemAzureTableStorageFactory
    {
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<ItemAzureTableStorageOptions>>();
            return ActivatorUtilities.CreateInstance<ItemAzureTableStorageProvider>(services, name, optionsMonitor.Get(name));
        }
    }

    public static class ItemAzureTableStorageSiloBuilderExtensions
    {
        public static ISiloHostBuilder AddItemAzureTableStorage(this ISiloHostBuilder builder, string name, Action<ItemAzureTableStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddItemAzureTableStorage(name, configureOptions));
        }

        public static ISiloBuilder AddItemAzureTableStorage(this ISiloBuilder builder, string name, Action<ItemAzureTableStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddItemAzureTableStorage(name, configureOptions));
        }

        public static IServiceCollection AddItemAzureTableStorage(this IServiceCollection services, string name, Action<ItemAzureTableStorageOptions> configureOptions)
        {
            return services.AddItemAzureTableStorage(name, ob => ob.Configure(configureOptions));
        }

        public static IServiceCollection AddItemAzureTableStorage(this IServiceCollection services, string name,
            Action<OptionsBuilder<ItemAzureTableStorageOptions>> configureOptions = null)
        {
            configureOptions?.Invoke(services.AddOptions<ItemAzureTableStorageOptions>(name));
            services.AddTransient<IConfigurationValidator>(sp => new ItemAzureTableStorageOptionsValidator(sp.GetRequiredService<IOptionsMonitor<ItemAzureTableStorageOptions>>().Get(name), name));
            services.ConfigureNamedOptionForLogging<ItemAzureTableStorageOptions>(name);
            services.TryAddSingleton<IGrainStorage>(sp => sp.GetServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
            return services.AddSingletonNamedService<IGrainStorage>(name, ItemAzureTableStorageFactory.Create)
                           .AddSingletonNamedService<ILifecycleParticipant<ISiloLifecycle>>(name, (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s.GetRequiredServiceByName<IGrainStorage>(n));
        }
    }

    #endregion
}
