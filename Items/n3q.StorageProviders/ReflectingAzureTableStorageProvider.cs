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
using System.Reflection;

namespace n3q.StorageProviders
{
    public static class ReflectingAzureTableStorage
    {
        public const string StorageProviderName = "ReflectingAzureTableStorage";
    }

    public class ReflectingAzureTableStorageOptions
    {
        public string ConnectionString = "DataConnectionString";
        public string TableName = "Items";

        // if you change these, then partition keys will change
        public int PartitionCount = 100;
        public string PartitionMask = "D2";

        public int InitStage { get; set; } = DEFAULT_INIT_STAGE;
        public const int DEFAULT_INIT_STAGE = ServiceLifecycleStage.ApplicationServices;
    }

    public class ReflectingAzureTableStorageProvider : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        readonly string _name;
        readonly ReflectingAzureTableStorageOptions _options;
        readonly ILogger _logger;
        readonly ILoggerFactory _loggerFactory;
        string _connectionString;
        string _tableName;
        CloudTable _table;

        public ReflectingAzureTableStorageProvider(string name,
            ReflectingAzureTableStorageOptions options,
            IGrainFactory grainFactory,
            ITypeResolver typeResolver,
            ILoggerFactory loggerFactory
            )
        {
            _name = name;
            _options = options;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger($"{typeof(ReflectingAzureTableStorageProvider).FullName}.{name}");
        }

        #region Interface

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            try {
                var pk = KeySafeName(GetPartitionKey(grainType, grainReference, grainState));
                var rk = KeySafeName(GetRowKey(grainType, grainReference, grainState));
                _logger.LogInformation($"Write item={grainReference.GetPrimaryKeyString()} GrainType={grainType} Pk={pk} Rk={rk} Table={_tableName}");
                var entityProperties = GetEntityPropertiesFromState(grainState.State);
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
                _logger.LogInformation($"Read item={grainReference.GetPrimaryKeyString()} GrainType={grainType} Pk={pk} Rk={rk} Table={_tableName}");
                var res = await GetTable().ExecuteAsync(TableOperation.Retrieve<DynamicTableEntity>(pk, rk));
                if (res.Result is DynamicTableEntity entity) {
                    AddEntityPropertiesToState(entity.Properties, grainState.State);
                    grainState.ETag = entity.ETag;
                }
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
                _logger.LogInformation($"Clear item={grainReference.GetPrimaryKeyString()} GrainType={grainType} Pk={pk} Rk={rk} Table={_tableName}");
                var res = await GetTable().ExecuteAsync(TableOperation.Retrieve<DynamicTableEntity>(pk, rk));
                if (res.Result is DynamicTableEntity entity) {
                    await GetTable().ExecuteAsync(TableOperation.Delete(entity));
                }
            } catch (Exception ex) {
                _logger.Error(0, $"Error deleting: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Internal

        IDictionary<string, EntityProperty> GetEntityPropertiesFromState(object state)
        {
            var entityProps = new Dictionary<string, EntityProperty>();

            var targetType = state.GetType();
            var targetFields = targetType.GetFields();
            foreach (var fieldInfo in targetFields) {
                if (fieldInfo.Attributes.HasFlag(FieldAttributes.Public)) {
                    var key = fieldInfo.Name;
                    var value = fieldInfo.GetValue(state);

                    EntityProperty prop = null;

                    if (false) {
                    } else if (fieldInfo.FieldType == typeof(long) || fieldInfo.FieldType == typeof(int)) {
                        prop = new EntityProperty((long)value);
                    } else if (fieldInfo.FieldType == typeof(double) || fieldInfo.FieldType == typeof(float)) {
                        prop = new EntityProperty((double)value);
                    } else if (fieldInfo.FieldType == typeof(bool)) {
                        prop = new EntityProperty((bool)value);
                    } else if (fieldInfo.FieldType == typeof(string)) {
                        prop = new EntityProperty((string)value);
                    } else {
                        prop = new EntityProperty(value.ToString());
                    }

                    if (prop != null) {
                        entityProps.Add(key, prop);
                    }
                }
            }

            return entityProps;
        }

        void AddEntityPropertiesToState(IDictionary<string, EntityProperty> entityProps, object state)
        {
            var targetType = state.GetType();
            var targetFields = targetType.GetFields();
            foreach (var fieldInfo in targetFields) {
                if (fieldInfo.Attributes.HasFlag(FieldAttributes.Public)) {
                    var key = fieldInfo.Name;
                    if (entityProps.ContainsKey(key)) {

                        var targetField = targetType.GetField(fieldInfo.Name);
                        var fieldType = fieldInfo.FieldType;
                        var entityType = entityProps[key].PropertyType;
                        if (false) {
                        } else if (fieldType == typeof(long)) {
                            if (entityType == EdmType.Int64) {
                                targetField.SetValue(state, entityProps[key].Int64Value);
                            } else if (entityType == EdmType.Int32) {
                                targetField.SetValue(state, (long)entityProps[key].Int32Value);
                            }
                        } else if (fieldType == typeof(int)) {
                            if (entityType == EdmType.Int32) {
                                targetField.SetValue(state, entityProps[key].Int32Value);
                            }
                        } else if (fieldType == typeof(double)) {
                            if (entityType == EdmType.Double) {
                                targetField.SetValue(state, entityProps[key].DoubleValue);
                            }
                        } else if (fieldType == typeof(float)) {
                            if (entityType == EdmType.Double) {
                                targetField.SetValue(state, (float)entityProps[key].DoubleValue);
                            }
                        } else if (fieldType == typeof(bool)) {
                            if (entityType == EdmType.Boolean) {
                                targetField.SetValue(state, (bool)entityProps[key].BooleanValue);
                            }
                        } else if (fieldType == typeof(string)) {
                            if (entityType == EdmType.String) {
                                targetField.SetValue(state, (string)entityProps[key].StringValue);
                            }
                        }

                    }
                }
            }
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
            return type;
        }

        string GetRowKey(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var primaryKey = grainReference.GetPrimaryKeyString();
            return primaryKey;
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
            lifecycle.Subscribe(OptionFormattingUtilities.Name<ReflectingAzureTableStorageProvider>(_name), _options.InitStage, Init);
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

    public class ReflectingAzureTableStorageOptionsValidator : IConfigurationValidator
    {
        public ReflectingAzureTableStorageOptionsValidator(ReflectingAzureTableStorageOptions options, string name) { }
        public void ValidateConfiguration() { }
    }

    public static class ReflectingAzureTableStorageFactory
    {
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<ReflectingAzureTableStorageOptions>>();
            return ActivatorUtilities.CreateInstance<ReflectingAzureTableStorageProvider>(services, name, optionsMonitor.Get(name));
        }
    }

    public static class ReflectingAzureTableStorageSiloBuilderExtensions
    {
        public static ISiloHostBuilder AddReflectingAzureTableStorage(this ISiloHostBuilder builder, string name, Action<ReflectingAzureTableStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddReflectingAzureTableStorage(name, configureOptions));
        }

        public static ISiloBuilder AddReflectingAzureTableStorage(this ISiloBuilder builder, string name, Action<ReflectingAzureTableStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddReflectingAzureTableStorage(name, configureOptions));
        }

        public static IServiceCollection AddReflectingAzureTableStorage(this IServiceCollection services, string name, Action<ReflectingAzureTableStorageOptions> configureOptions)
        {
            return services.AddReflectingAzureTableStorage(name, ob => ob.Configure(configureOptions));
        }

        public static IServiceCollection AddReflectingAzureTableStorage(this IServiceCollection services, string name,
            Action<OptionsBuilder<ReflectingAzureTableStorageOptions>> configureOptions = null)
        {
            configureOptions?.Invoke(services.AddOptions<ReflectingAzureTableStorageOptions>(name));
            services.AddTransient<IConfigurationValidator>(sp => new ReflectingAzureTableStorageOptionsValidator(sp.GetRequiredService<IOptionsMonitor<ReflectingAzureTableStorageOptions>>().Get(name), name));
            services.ConfigureNamedOptionForLogging<ReflectingAzureTableStorageOptions>(name);
            services.TryAddSingleton<IGrainStorage>(sp => sp.GetServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
            return services.AddSingletonNamedService<IGrainStorage>(name, ReflectingAzureTableStorageFactory.Create)
                           .AddSingletonNamedService<ILifecycleParticipant<ISiloLifecycle>>(name, (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s.GetRequiredServiceByName<IGrainStorage>(n));
        }
    }

    #endregion
}
