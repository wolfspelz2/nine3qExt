using System;
using System.Threading.Tasks;
using System.IO;
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

namespace nine3q.StorageProviders
{
    public class InventoryFileStorage
    {
        public const string StorageProviderName = "InventoryFileStorage";
    }

    public class InventoryFileStorageOptions
    {
        public string RootDirectory = @".\InventoryFileStorage\";
        public string FileExtension = ".txt";

        public int InitStage { get; set; } = DEFAULT_INIT_STAGE;
        public const int DEFAULT_INIT_STAGE = ServiceLifecycleStage.ApplicationServices;
    }

    public class InventoryFileStorageProvider : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly string _name;
        private readonly InventoryFileStorageOptions _options;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        public InventoryFileStorageProvider(string name,
            InventoryFileStorageOptions options,
            ILoggerFactory loggerFactory
            )
        {
            _name = name;
            _options = options;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger($"{typeof(InventoryFileStorageProvider).FullName}.{name}");
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
                var data = "";
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
                    var result = new object();
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
            lifecycle.Subscribe(OptionFormattingUtilities.Name<InventoryFileStorageProvider>(_name), _options.InitStage, Init);
        }

        private Task Init(CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        #endregion

    }

    #region Provider registration

    public class InventoryFileStorageOptionsValidator : IConfigurationValidator
    {
        public InventoryFileStorageOptionsValidator(InventoryFileStorageOptions options, string name) { }
        public void ValidateConfiguration() { }
    }

    public static class InventoryFileStorageFactory
    {
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<InventoryFileStorageOptions>>();
            return ActivatorUtilities.CreateInstance<InventoryFileStorageProvider>(services, name, optionsMonitor.Get(name));
        }
    }

    public static class InventoryFileStorageSiloBuilderExtensions
    {
        public static ISiloHostBuilder AddInventoryFileStorage(this ISiloHostBuilder builder, string name, Action<InventoryFileStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddInventoryFileStorage(name, configureOptions));
        }

        public static ISiloBuilder AddInventoryFileStorage(this ISiloBuilder builder, string name, Action<InventoryFileStorageOptions> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddInventoryFileStorage(name, configureOptions));
        }

        public static IServiceCollection AddInventoryFileStorage(this IServiceCollection services, string name, Action<InventoryFileStorageOptions> configureOptions)
        {
            return services.AddInventoryFileStorage(name, ob => ob.Configure(configureOptions));
        }

        public static IServiceCollection AddInventoryFileStorage(this IServiceCollection services, string name,
            Action<OptionsBuilder<InventoryFileStorageOptions>> configureOptions = null)
        {
            configureOptions?.Invoke(services.AddOptions<InventoryFileStorageOptions>(name));
            services.AddTransient<IConfigurationValidator>(sp => new InventoryFileStorageOptionsValidator(sp.GetRequiredService<IOptionsMonitor<InventoryFileStorageOptions>>().Get(name), name));
            services.ConfigureNamedOptionForLogging<InventoryFileStorageOptions>(name);
            services.TryAddSingleton<IGrainStorage>(sp => sp.GetServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
            return services.AddSingletonNamedService<IGrainStorage>(name, InventoryFileStorageFactory.Create)
                           .AddSingletonNamedService<ILifecycleParticipant<ISiloLifecycle>>(name, (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s.GetRequiredServiceByName<IGrainStorage>(n));
        }
    }

    #endregion
}
