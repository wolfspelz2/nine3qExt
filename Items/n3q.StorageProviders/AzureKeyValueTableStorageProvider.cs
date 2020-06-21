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
    public static class AzureKeyValueTableStorage
    {
        public const string StorageProviderName = "AzureKeyValueTableStorage";
    }

    public class AzureKeyValueTableStorageOptions
    {
        public string ConnectionString = "DataConnectionString";
        public string TableName = "Items";

        // if you change these, then partitions keys will change
        public int PartitionCount = 100; 
        public string PartitionMask = "D2";
        
        public int InitStage { get; set; } = DEFAULT_INIT_STAGE;
        public const int DEFAULT_INIT_STAGE = ServiceLifecycleStage.ApplicationServices;
    }

    public class AzureKeyValueTableStorageProvider : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        readonly string _name;
        readonly AzureKeyValueTableStorageOptions _options;
        readonly ILogger _logger;
        readonly ILoggerFactory _loggerFactory;
        readonly IGrainFactory _grainFactory;
        readonly long _id;
        static long _counter;
        string _connectionString;
        string _tableName;
        CloudTable _table;

        public AzureKeyValueTableStorageProvider(string name,
            AzureKeyValueTableStorageOptions options,
            IGrainFactory grainFactory,
            ITypeResolver typeResolver,
            ILoggerFactory loggerFactory
            )
        {
            _name = name;
            _options = options;
            _grainFactory = grainFactory;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger($"{typeof(AzureKeyValueTableStorageProvider).FullName}.{name}");
            _id = Interlocked.Increment(ref _counter);
        }

        #region Interface

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            try {
                var pk = KeySafeName(GetPartitionKey(grainType, grainReference, grainState));
                var rk = KeySafeName(GetRowKey(grainType, grainReference, grainState));
                _logger.LogInformation($"Writing item={grainReference.GetPrimaryKeyString()} GrainType={grainType} Pk={pk} Rk={rk} Table={_tableName}");
                var kvGrainState = grainState as GrainState<KeyValueStorageData>;
                var entityProperties = StateDictionary2EntityProperties(kvGrainState.State);
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
                var pk = KeySafeName(GetPartitionKey(grainType, grainReference, grainState));
                var rk = KeySafeName(GetRowKey(grainType, grainReference, grainState));
                _logger.LogInformation($"Reading item={grainReference.GetPrimaryKeyString()} GrainType={grainType} Pk={pk} Rk={rk} Table={_tableName}");
                var res = await GetTable().ExecuteAsync(TableOperation.Retrieve<DynamicTableEntity>(pk, rk));
                if (res.Result == null) { return; }
                var entity = res.Result as DynamicTableEntity;
                if (entity == null) { return; }
                var kvGrainState = grainState as GrainState<KeyValueStorageData>;
                kvGrainState.State = EntityProperties2StateDictionary(entity.Properties);
                grainState.ETag = entity.ETag;
            } catch (Exception ex) {
                _logger.Error(0, $"Error reading: {ex.Message}");
                throw;
            }
        }

        public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            try {
                var pk = KeySafeName(GetPartitionKey(grainType, grainReference, grainState));
                var rk = KeySafeName(GetRowKey(grainType, grainReference, grainState));
                _logger.LogInformation($"Reading item={grainReference.GetPrimaryKeyString()} GrainType={grainType} Pk={pk} Rk={rk} Table={_tableName}");
                var res = await GetTable().ExecuteAsync(TableOperation.Retrieve<DynamicTableEntity>(pk, rk));
                if (res.Result == null) { return; }
                var entity = res.Result as DynamicTableEntity;
                if (entity == null) { return; }
                await GetTable().ExecuteAsync(TableOperation.Delete(entity));
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

        string GetPartitionKey(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var type = grainState.Type.Name;
            var typeListParts = grainType.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (typeListParts.Length > 0) {
                var fullType = typeListParts[typeListParts.Length - 1];
                var typeParts = fullType.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (typeParts.Length > 0) {
                    type = typeParts[typeParts.Length - 1];
                }
            }

            var primaryKey = grainReference.GetPrimaryKeyString();
            var hash = primaryKey.SimpleHash() % _options.PartitionCount;
            var partitionId = hash.ToString(_options.PartitionMask);

            return type + "-" + partitionId;
        }

        string GetRowKey(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var primaryKey = grainReference.GetPrimaryKeyString();
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
            lifecycle.Subscribe(OptionFormattingUtilities.Name<AzureKeyValueTableStorageProvider>(_name), _options.InitStage, Init);
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

    public class AzureKeyValueTableStorageOptionsValidator : IConfigurationValidator
    {
        public AzureKeyValueTableStorageOptionsValidator(AzureKeyValueTableStorageOptions options, string name) { }
        public void ValidateConfiguration() { }
    }

    public static class AzureKeyValueTableStorageFactory
    {
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<AzureKeyValueTableStorageOptions>>();
            return ActivatorUtilities.CreateInstance<AzureKeyValueTableStorageProvider>(services, name, optionsMonitor.Get(name));
        }
    }

    public static class AzureKeyValueTableStorageSiloBuilderExtensions
    {
        public static ISiloHostBuilder AddAzureKeyValueTableStorage(this ISiloHostBuilder builder, string name, Action<AzureKeyValueTableStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddAzureKeyValueTableStorage(name, configureOptions));
        }

        public static ISiloBuilder AddAzureKeyValueTableStorage(this ISiloBuilder builder, string name, Action<AzureKeyValueTableStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddAzureKeyValueTableStorage(name, configureOptions));
        }

        public static IServiceCollection AddAzureKeyValueTableStorage(this IServiceCollection services, string name, Action<AzureKeyValueTableStorageOptions> configureOptions)
        {
            return services.AddAzureKeyValueTableStorage(name, ob => ob.Configure(configureOptions));
        }

        public static IServiceCollection AddAzureKeyValueTableStorage(this IServiceCollection services, string name,
            Action<OptionsBuilder<AzureKeyValueTableStorageOptions>> configureOptions = null)
        {
            configureOptions?.Invoke(services.AddOptions<AzureKeyValueTableStorageOptions>(name));
            services.AddTransient<IConfigurationValidator>(sp => new AzureKeyValueTableStorageOptionsValidator(sp.GetRequiredService<IOptionsMonitor<AzureKeyValueTableStorageOptions>>().Get(name), name));
            services.ConfigureNamedOptionForLogging<AzureKeyValueTableStorageOptions>(name);
            services.TryAddSingleton<IGrainStorage>(sp => sp.GetServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
            return services.AddSingletonNamedService<IGrainStorage>(name, AzureKeyValueTableStorageFactory.Create)
                           .AddSingletonNamedService<ILifecycleParticipant<ISiloLifecycle>>(name, (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s.GetRequiredServiceByName<IGrainStorage>(n));
        }
    }

    #endregion
}
