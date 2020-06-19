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
using System.Globalization;

namespace n3q.StorageProviders
{
    public static class KeyValueFileStorage
    {
        public const string StorageProviderName = "KeyValueFileStorage";
    }

    public class KeyValueFileStorageOptions
    {
        public string RootDirectory = @".\KeyValueFileStorage\";
        public string JsonFileExtension = ".json";
        public string SrpcFileExtension = ".txt";

        public bool UseFullAssemblyNames { get; set; } = false;
        public bool IndentJson { get; set; } = true;
        public TypeNameHandling? TypeNameHandling { get; set; } = Newtonsoft.Json.TypeNameHandling.All;

        public int InitStage { get; set; } = DEFAULT_INIT_STAGE;
        public const int DEFAULT_INIT_STAGE = ServiceLifecycleStage.ApplicationServices;
    }

    public class KeyValueFileStorageProvider : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly string _name;
        private readonly KeyValueFileStorageOptions _options;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IGrainFactory _grainFactory;
        private readonly ITypeResolver _typeResolver;
        private JsonSerializerSettings _jsonSettings;

        public KeyValueFileStorageProvider(string name,
            KeyValueFileStorageOptions options,
            IGrainFactory grainFactory,
            ITypeResolver typeResolver,
            ILoggerFactory loggerFactory
            )
        {
            _name = name;
            _options = options;
            _grainFactory = grainFactory;
            _typeResolver = typeResolver;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger($"{typeof(KeyValueFileStorageProvider).FullName}.{name}");
        }

        #region Interface

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var filePath = "";
            try {

                var data = "";
                if (grainState is GrainState<KeyValueStorageData> kvdgs) {
                    filePath = GetFilePath(grainType, grainReference, grainState, _options.SrpcFileExtension);
                    data = kvdgs.State.ToSrpc();
                } else {
                    filePath = GetFilePath(grainType, grainReference, grainState, _options.JsonFileExtension);
                    data = JsonConvert.SerializeObject(grainState, _jsonSettings);
                }

                File.WriteAllText(filePath, data, Encoding.UTF8);

            } catch (Exception ex) {
                _logger.Error(0, $"Error writing: filePath={filePath} {ex.Message}");
                throw;
            }
            await Task.CompletedTask;
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var filePath = "";
            try {

                if (grainState is GrainState<KeyValueStorageData> kvdgs) {
                    filePath = GetFilePath(grainType, grainReference, grainState, _options.SrpcFileExtension);
                    if (File.Exists(filePath)) {
                        var data = File.ReadAllText(filePath, Encoding.UTF8);
                        kvdgs.State.FromSrpc(data);
                        grainState = kvdgs;
                    }
                } else {
                    filePath = GetFilePath(grainType, grainReference, grainState, _options.JsonFileExtension);
                    if (File.Exists(filePath)) {
                        var data = File.ReadAllText(filePath, Encoding.UTF8);
                        var result = JsonConvert.DeserializeObject<object>(data, _jsonSettings);
                        grainState.State = (result as IGrainState).State;
                    }
                }

            } catch (Exception ex) {
                _logger.Error(0, $"Error reading: filePath={filePath} {ex.Message}");
                throw;
            }
            await Task.CompletedTask;
        }

        public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var filePath = "";
            try {

                if (grainState is GrainState<KeyValueStorageData> kvdgs) {
                    filePath = GetFilePath(grainType, grainReference, grainState, _options.SrpcFileExtension);
                    if (File.Exists(filePath)) {
                        File.Delete(filePath);
                    }
                } else {
                    filePath = GetFilePath(grainType, grainReference, grainState, _options.JsonFileExtension);
                    if (File.Exists(filePath)) {
                        File.Delete(filePath);
                    }
                }

            } catch (Exception ex) {
                _logger.Error(0, $"Error deleting: filePath={filePath} {ex.Message}");
                throw;
            }
            await Task.CompletedTask;
        }

        #endregion

        #region Internal

        protected static string GetPrimaryKey(GrainReference grainReference)
        {
            grainReference.GetPrimaryKey(out string primaryKey);
            return primaryKey;
        }

        static char[] _invalidFileNameChars;

        static char[] InvalidFileNameChars
        {
            get {
                if (_invalidFileNameChars == null) {
                    var chars = Path.GetInvalidFileNameChars();
                    _invalidFileNameChars = new char[chars.Length + 1];
                    for (var i = 0; i < chars.Length; i++) {
                        _invalidFileNameChars[i] = chars[i];
                    }
                    _invalidFileNameChars[chars.Length] = '%';
                }
                return _invalidFileNameChars;
            }
        }
        private string FilesystemSafeName(string name)
        {
            var safeName = new StringBuilder();
            for (var i = 0; i < name.Length; i++) {
                if (InvalidFileNameChars.Contains(name[i])) {
                    safeName.Append('%');
                    safeName.Append(Convert.ToInt32(name[i]));
                } else {
                    safeName.Append(name[i]);
                }
            }
            return safeName.ToString();
        }

        protected string GetFilePath(string grainType, GrainReference grainReference, IGrainState grainState, string fileExt)
        {
            if (!Directory.Exists(_options.RootDirectory)) {
                throw new Exception($"{nameof(KeyValueFileStorage)}: root directory does not exist: {_options.RootDirectory}");
            }

            var type = grainState.Type.Name;
            var typeListParts = grainType.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (typeListParts.Length > 0) {
                var fullType = typeListParts[typeListParts.Length - 1];
                var typeParts = fullType.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (typeParts.Length > 0) {
                    type = typeParts[typeParts.Length - 1];
                }
            }

            var id = GetPrimaryKey(grainReference);
            var fileName = FilesystemSafeName(type) + "-" + FilesystemSafeName(id) + fileExt;
            var filePath = Path.Combine(_options.RootDirectory, fileName);
            return filePath;
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(OptionFormattingUtilities.Name<KeyValueFileStorageProvider>(_name), _options.InitStage, Init);
        }

        private Task Init(CancellationToken ct)
        {
            _jsonSettings = OrleansJsonSerializer.UpdateSerializerSettings(
                OrleansJsonSerializer.GetDefaultSerializerSettings(_typeResolver, _grainFactory),
                _options.UseFullAssemblyNames,
                _options.IndentJson,
                _options.TypeNameHandling
                );

            return Task.CompletedTask;
        }

        #endregion
    }

    #region Provider registration

    public class KeyValueFileStorageOptionsValidator : IConfigurationValidator
    {
        public KeyValueFileStorageOptionsValidator(KeyValueFileStorageOptions options, string name) { }
        public void ValidateConfiguration() { }
    }

    public static class KeyValueFileStorageFactory
    {
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<KeyValueFileStorageOptions>>();
            return ActivatorUtilities.CreateInstance<KeyValueFileStorageProvider>(services, name, optionsMonitor.Get(name));
        }
    }

    public static class KeyValueFileStorageSiloBuilderExtensions
    {
        public static ISiloHostBuilder AddKeyValueFileStorage(this ISiloHostBuilder builder, string name, Action<KeyValueFileStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddKeyValueFileStorage(name, configureOptions));
        }

        public static ISiloBuilder AddKeyValueFileStorage(this ISiloBuilder builder, string name, Action<KeyValueFileStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddKeyValueFileStorage(name, configureOptions));
        }

        public static IServiceCollection AddKeyValueFileStorage(this IServiceCollection services, string name, Action<KeyValueFileStorageOptions> configureOptions)
        {
            return services.AddKeyValueFileStorage(name, ob => ob.Configure(configureOptions));
        }

        public static IServiceCollection AddKeyValueFileStorage(this IServiceCollection services, string name,
            Action<OptionsBuilder<KeyValueFileStorageOptions>> configureOptions = null)
        {
            configureOptions?.Invoke(services.AddOptions<KeyValueFileStorageOptions>(name));
            services.AddTransient<IConfigurationValidator>(sp => new KeyValueFileStorageOptionsValidator(sp.GetRequiredService<IOptionsMonitor<KeyValueFileStorageOptions>>().Get(name), name));
            services.ConfigureNamedOptionForLogging<KeyValueFileStorageOptions>(name);
            services.TryAddSingleton<IGrainStorage>(sp => sp.GetServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
            return services.AddSingletonNamedService<IGrainStorage>(name, KeyValueFileStorageFactory.Create)
                           .AddSingletonNamedService<ILifecycleParticipant<ISiloLifecycle>>(name, (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s.GetRequiredServiceByName<IGrainStorage>(n));
        }
    }

    #endregion
}
