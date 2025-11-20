using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.GameEvents;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GameManager;

public partial class GameManager : BasePlugin
{
  private readonly ICommandService? _commandService = null;
  private ServiceProvider? _serviceProvider;
  private IOptionsMonitor<ConfigModel>? _configMonitor;

  private void InitPlugin()
  {
    // Inicializar configuración con DI para soporte de recarga en caliente
    Core.Configuration
        .InitializeJsonWithModel<ConfigModel>("config.jsonc", "Main")
        .Configure(builder =>
        {
            builder.AddJsonFile("config.jsonc", optional: false, reloadOnChange: true);
        });

    // Ruta y existencia (informativa / fallback)
    string configPath = Core.Configuration.GetConfigPath("config.jsonc");
    bool exists = Core.Configuration.BasePathExists;
    if (!exists)
    {
        // Si la carpeta base no existe, intentar inicializar con plantilla si procede
        Core.Configuration.InitializeWithTemplate("config.jsonc", "template.jsonc");
    }

    var collection = new ServiceCollection();
    collection.AddSwiftly(Core);

    collection
        .AddOptionsWithValidateOnStart<ConfigModel>()
        .BindConfiguration("Main");

    _serviceProvider = collection.BuildServiceProvider();

    _configMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<ConfigModel>>();
    _config = _configMonitor.CurrentValue;

    _configMonitor.OnChange(newConfig =>
    {
        _config = newConfig;
        // Opcional: log de recarga
        Core.Logger.LogInformation("Configuración recargada en caliente.");
    });

    RegisterNeededHooks();
    ExecuteNativeCommands();
    CheckClientCommands();
  }

  public override void Load(bool hotReload)
  {
    InitPlugin();
  }

  public override void Unload()
  {
    _serviceProvider?.Dispose();
  }

  [GameEventHandler(HookMode.Post)]
  public HookResult OnRoundStart(EventRoundStart @event)
  {
    ExecuteNativeCommands();
    return HookResult.Continue;
  }
}