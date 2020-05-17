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

namespace nine3q.StorageProviders
{
    public class JsonFileStorage
    {
        public const string StorageProviderName = "JsonFileStorage";
    }

    public class JsonFileStorageOptions
    {
        public string RootDirectory = @".\JsonFileStorage\";
        public string FileExtension = ".json";

        public bool UseFullAssemblyNames { get; set; } = false;
        public bool IndentJson { get; set; } = true;
        public TypeNameHandling? TypeNameHandling { get; set; } = Newtonsoft.Json.TypeNameHandling.All;

        public int InitStage { get; set; } = DEFAULT_INIT_STAGE;
        public const int DEFAULT_INIT_STAGE = ServiceLifecycleStage.ApplicationServices;
    }

    public class JsonFileStorageProvider : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly string _name;
        private readonly JsonFileStorageOptions _options;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IGrainFactory _grainFactory;
        private readonly ITypeResolver _typeResolver;
        private JsonSerializerSettings _jsonSettings;

        public JsonFileStorageProvider(string name,
            JsonFileStorageOptions options,
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
            _logger = _loggerFactory.CreateLogger($"{typeof(JsonFileStorageProvider).FullName}.{name}");
        }

        #region Interface

        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var filePath = GetFilePath(grainType, grainReference, grainState);
            try {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists) {
                    fileInfo.Delete();
                }
            } catch (Exception ex) {
                _logger.Error(0, $"Error deleting: filePath={filePath} {ex.Message}");
                throw;
            }
            return Task.CompletedTask;
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            await Task.CompletedTask;
            var filePath = GetFilePath(grainType, grainReference, grainState);
            try {
                var data = JsonConvert.SerializeObject(grainState, _jsonSettings);
                File.WriteAllText(filePath, data, Encoding.UTF8);
            } catch (Exception ex) {
                _logger.Error(0, $"Error writing: filePath={filePath} {ex.Message}");
                throw;
            }
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            await Task.CompletedTask;
            var filePath = GetFilePath(grainType, grainReference, grainState);
            try {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists) {
                    var data = File.ReadAllText(filePath, Encoding.UTF8);
                    var result = JsonConvert.DeserializeObject<object>(data, _jsonSettings);
                    grainState.State = (result as IGrainState).State;
                }
            } catch (Exception ex) {
                _logger.Error(0, $"Error reading: filePath={filePath} {ex.Message}");
                throw;
            }
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

        protected string GetFilePath(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var type = grainState.Type.Name;
            var id = GetPrimaryKey(grainReference);
            var fileName = FilesystemSafeName(type) + "-" + FilesystemSafeName(id) + this._options.FileExtension;
            var filePath = Path.Combine(_options.RootDirectory, fileName);
            return filePath;
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(OptionFormattingUtilities.Name<JsonFileStorageProvider>(_name), _options.InitStage, Init);
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

        //public Logger Log { get; set; }
        //public string Name { get; set; }
        //public string RootDirectory { get; set; }
        //public string FileExtension { get; set; }

        //public Task Close()
        //{
        //    return TaskDone.Done;
        //}

        //public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        //{
        //    this.Name = name;
        //    if (string.IsNullOrWhiteSpace(config.Properties["RootDirectory"])) { throw new ArgumentException("RootDirectory property not set"); }

        //    var directory = new System.IO.DirectoryInfo(config.Properties["RootDirectory"]);
        //    if (!directory.Exists) { directory.Create(); }

        //    this.RootDirectory = directory.FullName;
        //    this.FileExtension = config.Properties["FileExtension"];

        //    return TaskDone.Done;
        //}


        //public async Task ReadStateAsync(string grainType, GrainReference grainRef, GrainState grainState)
        //{
        //    var fileInfo = new FileInfo(FilePath(grainType, GetPrimaryKey(grainType, grainRef, grainState)));
        //    if (!fileInfo.Exists) { return; }

        //    using (var stream = fileInfo.OpenText()) {
        //        var data = await stream.ReadToEndAsync();

        //        try {
        //            var tree = new JsonTree.Node(data);
        //            var dict = NodeToDictionary(tree);
        //            grainState.SetAll(dict);
        //        } catch (Exception ex) {
        //            throw ex;
        //        }

        //    }
        //}

        //public Dictionary<string, object> NodeToDictionary(JsonTree.Node node)
        //{
        //    var dict = new Dictionary<string, object>();

        //    foreach (var pair in node.AsDictionary) {
        //        if (pair.Value.IsInt) {
        //            if (pair.Value.AsInt <= Int32.MaxValue && pair.Value.AsInt >= Int32.MinValue) {
        //                dict.Add(pair.Key, (int)pair.Value.AsInt);
        //            } else {
        //                dict.Add(pair.Key, (long)pair.Value.AsInt);
        //            }
        //        }
        //        if (pair.Value.IsFloat) {
        //            dict.Add(pair.Key, pair.Value.AsFloat);
        //        }
        //        if (pair.Value.IsBool) {
        //            dict.Add(pair.Key, pair.Value.AsBool);
        //        }
        //        if (pair.Value.IsString) {
        //            dict.Add(pair.Key, pair.Value.AsString);
        //        }
        //        if (pair.Value.IsDictionary) {
        //            var sub = NodeToDictionary(pair.Value);
        //            dict.Add(pair.Key, sub);
        //        }
        //    }

        //    return dict;
        //}

    }

    public class JsonFileStorageOptionsValidator : IConfigurationValidator
    {
        public JsonFileStorageOptionsValidator(JsonFileStorageOptions options, string name) { }
        public void ValidateConfiguration() { }
    }

    public static class JsonFileStorageFactory
    {
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<JsonFileStorageOptions>>();
            return ActivatorUtilities.CreateInstance<JsonFileStorageProvider>(services, name, optionsMonitor.Get(name));
        }
    }

    public static class JsonFileStorageSiloBuilderExtensions
    {
        public static ISiloHostBuilder AddJsonFileGrainStorage(this ISiloHostBuilder builder, string name, Action<JsonFileStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddJsonFileGrainStorage(name, configureOptions));
        }

        public static ISiloBuilder AddJsonFileGrainStorage(this ISiloBuilder builder, string name, Action<JsonFileStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddJsonFileGrainStorage(name, configureOptions));
        }

        public static IServiceCollection AddJsonFileGrainStorage(this IServiceCollection services, string name, Action<JsonFileStorageOptions> configureOptions)
        {
            return services.AddJsonFileGrainStorage(name, ob => ob.Configure(configureOptions));
        }

        public static IServiceCollection AddJsonFileGrainStorage(this IServiceCollection services, string name,
            Action<OptionsBuilder<JsonFileStorageOptions>> configureOptions = null)
        {
            configureOptions?.Invoke(services.AddOptions<JsonFileStorageOptions>(name));
            services.AddTransient<IConfigurationValidator>(sp => new JsonFileStorageOptionsValidator(sp.GetRequiredService<IOptionsMonitor<JsonFileStorageOptions>>().Get(name), name));
            services.ConfigureNamedOptionForLogging<JsonFileStorageOptions>(name);
            services.TryAddSingleton<IGrainStorage>(sp => sp.GetServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
            return services.AddSingletonNamedService<IGrainStorage>(name, JsonFileStorageFactory.Create)
                           .AddSingletonNamedService<ILifecycleParticipant<ISiloLifecycle>>(name, (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s.GetRequiredServiceByName<IGrainStorage>(n));
        }
    }
}
