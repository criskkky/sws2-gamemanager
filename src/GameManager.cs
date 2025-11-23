using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.GameEvents;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.GameEventDefinitions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GameManager;

public partial class GameManager : BasePlugin
{
  private ServiceProvider? _serviceProvider;
  private IOptionsMonitor<ConfigModel>? _configMonitor;

  public override void Load(bool hotReload)
  {
    // Start configuration with DI for hot-reload support
    Core.Configuration
    .InitializeJsonWithModel<ConfigModel>("config.jsonc", "GameManager")
    .Configure(builder =>
    {
      builder.AddJsonFile("config.jsonc", optional: false, reloadOnChange: true);
    });

    var collection = new ServiceCollection();
    collection.AddSwiftly(Core);

    collection
    .AddOptionsWithValidateOnStart<ConfigModel>()
    .BindConfiguration("GameManager");

    _serviceProvider = collection.BuildServiceProvider();

    _configMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<ConfigModel>>();
    _config = _configMonitor.CurrentValue;

    _configMonitor.OnChange(newConfig =>
    {
      _config = newConfig;
      // Reload everything on config change
      RegisterNeededHooks();
      ExecuteNativeCommands();
      CheckClientCommands();
    });

    RegisterNeededHooks();
    ExecuteNativeCommands();
    CheckClientCommands();
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