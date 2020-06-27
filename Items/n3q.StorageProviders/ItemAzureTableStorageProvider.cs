using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orleans;
using Orleans.Storage;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Hosting;
using Orleans.Configuration;
using System.Threading;
using Orleans.Runtime.Configuration;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using n3q.Tools;

namespace n3q.StorageProviders
{
    #region Options

    public static class ItemAzureTableStorage
    {
        public const string StorageProviderName = "ItemAzureTableStorage";
        public const string PidTemplate = "Template";
        public const int PartitionKeyFromPrimaryKeyLength = 2;
    }

    public class ItemAzureTableStorageOptions
    {
        public string ConnectionString = "DataConnectionString";
        public string TableName = "Items";

        // if you change these, then partition keys will change
        //public int PartitionCount = 100;
        //public string PartitionMask = "D2";

        public int InitStage { get; set; } = DEFAULT_INIT_STAGE;
        public const int DEFAULT_INIT_STAGE = ServiceLifecycleStage.ApplicationServices;
    }

    #endregion

    public class ItemAzureTableStorageProvider : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        readonly string _name;
        readonly ItemAzureTableStorageOptions _options;
        readonly ILogger _logger;
        readonly ILoggerFactory _loggerFactory;
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
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger($"{typeof(ItemAzureTableStorageProvider).FullName}.{name}");
        }

        #region Interface

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            try {
                if (GetPartitionKeyAndRowKey(grainType, grainReference, grainState, out var pk, out var rk)) {
                    pk = KeySafeName(pk);
                    rk = KeySafeName(rk);
                    _logger.LogInformation($"Writing item={grainReference.GetPrimaryKeyString()} GrainType={grainType} Pk={pk} Rk={rk} Table={_tableName}");
                    var kvGrainState = grainState as GrainState<KeyValueStorageData>;
                    var entityProperties = StateDictionary2EntityProperties(kvGrainState.State);
                    var entity = new DynamicTableEntity(pk, rk, grainState.ETag, entityProperties);
                    await GetTable().ExecuteAsync(TableOperation.InsertOrReplace(entity));
                }
            } catch (Exception ex) {
                _logger.Error(0, $"Error writing item={grainReference.GetPrimaryKeyString()}: {ex.Message}");
                throw;
            }
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            try {
                if (GetPartitionKeyAndRowKey(grainType, grainReference, grainState, out var pk, out var rk)) {
                    pk = KeySafeName(pk);
                    rk = KeySafeName(rk);
                    _logger.LogInformation($"Reading item={grainReference.GetPrimaryKeyString()} GrainType={grainType} Pk={pk} Rk={rk} Table={_tableName}");
                    var res = await GetTable().ExecuteAsync(TableOperation.Retrieve<DynamicTableEntity>(pk, rk));
                    if (res.Result is DynamicTableEntity entity) {
                        var kvGrainState = grainState as GrainState<KeyValueStorageData>;
                        kvGrainState.State = EntityProperties2StateDictionary(entity.Properties);
                        grainState.ETag = entity.ETag;
                    }
                }
            } catch (Exception ex) {
                _logger.Error(0, $"Error reading: {ex.Message}");
                throw;
            }
        }

        public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            try {
                if (GetPartitionKeyAndRowKey(grainType, grainReference, grainState, out var pk, out var rk)) {
                    pk = KeySafeName(pk);
                    rk = KeySafeName(rk);
                    _logger.LogInformation($"Reading item={grainReference.GetPrimaryKeyString()} GrainType={grainType} Pk={pk} Rk={rk} Table={_tableName}");
                    var res = await GetTable().ExecuteAsync(TableOperation.Retrieve<DynamicTableEntity>(pk, rk));
                    if (res.Result is DynamicTableEntity entity) {
                        await GetTable().ExecuteAsync(TableOperation.Delete(entity));
                    }
                }
            } catch (Exception ex) {
                _logger.Error(0, $"Error deleting: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Internal

        IDictionary<string, EntityProperty> StateDictionary2EntityProperties(KeyValueStorageData kvData)
        {
            var entityProps = new Dictionary<string, EntityProperty>();

            EntityProperty prop = null;
            foreach (var pair in kvData) {
                if (false) {
                } else if (pair.Value is long) {
                    prop = new EntityProperty((long)pair.Value);
                } else if (pair.Value is double) {
                    prop = new EntityProperty((double)pair.Value);
                } else if (pair.Value is bool) {
                    prop = new EntityProperty((bool)pair.Value);
                } else {
                    prop = new EntityProperty(pair.Value.ToString());
                }

                entityProps.Add(pair.Key.ToString(), prop);
            }

            return entityProps;
        }

        KeyValueStorageData EntityProperties2StateDictionary(IDictionary<string, EntityProperty> entityProps)
        {
            var kvData = new KeyValueStorageData();

            foreach (var pair in entityProps) {
                object value = pair.Value;
                kvData.Add(pair.Key, value.ToString());
            }

            return kvData;
        }

        bool GetPartitionKeyAndRowKey(string grainType, GrainReference grainReference, IGrainState grainState, out string partitionKey, out string rowKey)
        {
            partitionKey = "";
            rowKey = "";

            //if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey)) {
            //    if (grainState is GrainState<KeyValueStorageData> kvGrainState) {
            //        var kv = kvGrainState.State;
            //        var template = kv.Get(ItemAzureTableStorage.PidTemplate, "");
            //        if (!string.IsNullOrEmpty(template)) {
            //            partitionKey = template;
            //            rowKey = grainReference.GetPrimaryKeyString();
            //        }
            //    }
            //}

            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey)) {
                var primaryKey = grainReference.GetPrimaryKeyString();
                partitionKey = primaryKey.Substring(0, ItemAzureTableStorage.PartitionKeyFromPrimaryKeyLength); ;
                rowKey = primaryKey;
            }

            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey)) {
                var typeListParts = grainType.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (typeListParts.Length > 0) {
                    var fullType = typeListParts[typeListParts.Length - 1];
                    var typeParts = fullType.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (typeParts.Length > 0) {
                        partitionKey = typeParts[typeParts.Length - 1];
                        rowKey = grainReference.GetPrimaryKeyString();
                    }
                }
            }

            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey)) {
                partitionKey = grainType;
                rowKey = grainReference.GetPrimaryKeyString();
            }

            return true;
        }

        static readonly char[] KeyInvalidChars = { '/', '\\', '#', '?', '|', '[', ']', '{', '}', '<', '>', '$', '^', '&', '%', '+', '\'' };
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
            _tableName = _options.TableName;

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
