using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.ProtobufDefinitions;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace GameManager;

public partial class GameManager(ISwiftlyCore core) : BasePlugin(core)
{
  // Model created from what I consider useful configuration options, PRs are welcome to add more!
  public class ConfigModel
  {
    // === Blockers ===
    public bool BlockRadio { get; set; } = false;
    public bool BlockBotRadio { get; set; } = false;
    public bool BlockGrenadesRadio { get; set; } = false;
    public bool BlockChatWheel { get; set; } = false;
    public bool BlockPing { get; set; } = false;
    public List<string> BlockedCommands { get; set; } = []; // Case Sensitive
    public string BlockedCommandsWhitelist { get; set; } = ""; // Flags

    // === Hide ===
    public bool HideRadar { get; set; } = false;
    public byte HideKillFeed { get; set; } = 0; // 0 = No, 1 = Full, 2 = Show only own kills
    public bool HideBlood { get; set; } = false;
    public bool HideHeadshotSparks { get; set; } = false;
    public byte HideTeammateHeadtags { get; set; } = 0; // 0 = No, 1 = Yes, 2 = Just behind walls, 3 = Disable by distance
    public byte HideTeammateHeadtags_Distance { get; set; } = 0; // 50, 100, 150, 250
    public byte HideCorpses { get; set; } = 0; // 0 = No, 1 = Instantly, 2 = Fade out
    public bool HideLegs { get; set; } = false;

    // === Disable ===
    public bool DisableFallDamage { get; set; } = false;
    public bool DisableSVCheats { get; set; } = false;
    public bool DisableC4 { get; set; } = false;
    public bool DisableCameraSpectator { get; set; } = false;
    public byte DisableAimPunch { get; set; } = 0; // 0 = No, 1 = Yes, 2 = Togglable (default ON), 3 = Togglable (default OFF)
    public List<string> DisableAimPunch_Flags { get; set; } = []; // Flags for togglable

    // === Sounds ===
    public byte SoundsMuteMVPMusic { get; set; } = 0; // 0 = No, 1 = Yes, 2 = MVP + Round End Music
    public bool SoundsMuteFootsteps { get; set; } = false;
    public bool SoundsMuteJumpLand { get; set; } = false;

    // === Default MSGS ===
    public bool IgnoreBombPlantedHUDMessages { get; set; } = false;
    public bool IgnoreTeammateAttackMessages { get; set; } = false;
    public bool IgnoreAwardsMoneyMessages { get; set; } = false;
    public bool IgnorePlayerSavedYouMessages { get; set; } = false;
    public bool IgnoreChickenKilledMessages { get; set; } = false;
    public bool IgnoreJoinTeamMessages { get; set; } = false;
    public bool IgnorePlantingBombMessages { get; set; } = false;
    public bool IgnoreDefusingBombMessages { get; set; } = false;
    public bool IgnoreDisconnectMessages { get; set; } = false;

  }

  private ConfigModel? _config;

  // --- GUIDs for Hooks ---
  private Guid? _radioHookGuid;
  private Guid? _deathEventHookGuid;
  private Guid? _bloodHookGuid;
  private Guid? _sparksHookGuid;
  private Guid? _legsHookGuid;
  private Guid? _aimPunchHookGuid;
  private Guid? _toggleAimPunchCommandGuid;
  private bool _aimPunchEnabled;
  private Guid? _mvpMusicHookGuid;
  private Guid? _ignoreBombPlantedHUDMessagesHookGuid;
  private Guid? _ignoreTeammateAttackMessagesHookGuid;
  private Guid? _ignoreAwardsMoneyMessagesHookGuid;
  private Guid? _ignorePlayerSavedYouMessagesHookGuid;
  private Guid? _ignoreChickenKilledMessagesHookGuid;
  private Guid? _ignoreJoinTeamMessagesHookGuid;
  private Guid? _ignorePlantingBombMessagesHookGuid;
  private Guid? _ignoreDefusingBombMessagesHookGuid;
  private Guid? _ignoreDisconnectMessagesHookGuid;

  private void RegisterNeededHooks()
  {
    // --- Handle Hook Creation: Radio ---
    if (_radioHookGuid.HasValue)
    {
      Core.Command.UnhookClientCommand(_radioHookGuid.Value);
      _radioHookGuid = null;
    }
    if (_config?.BlockRadio == true)
    {
      _radioHookGuid = Core.Command.HookClientCommand((playerId, commandLine) =>
      {
        var commandName = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
        if (Helper.RadioArray.Contains(commandName))
        {
          return HookResult.Stop;
        }
        return HookResult.Continue;
      });
    }

    // --- Handle Hook Creation: Death Event ---
    if (_deathEventHookGuid.HasValue)
    {
      Core.GameEvent.Unhook(_deathEventHookGuid.Value);
      _deathEventHookGuid = null;
    }
    if (_config?.HideKillFeed > 0 || _config?.HideCorpses > 0)
    {
      _deathEventHookGuid = Core.GameEvent.HookPre<EventPlayerDeath>(@event =>
      {
        var attacker = Core.PlayerManager.GetPlayer(@event.Attacker);
        if (attacker == null || !attacker.IsValid) return HookResult.Continue;

        // First handle corpses
        if (_config?.HideCorpses > 0)
        {
          var player = Core.PlayerManager.GetPlayer(@event.UserId);
          if (player != null && player.IsValid)
          {
            var playerPawn = player.PlayerPawn;
            if (playerPawn != null)
            {
              Core.Scheduler.NextTick(() =>
              {
                if (_config?.HideCorpses == 1)
                {
                  var currentColor = playerPawn.Render;
                  playerPawn.Render = new Color(currentColor.R, currentColor.G, currentColor.B, (byte)0);
                  playerPawn.RenderUpdated();
                }
                if (_config?.HideCorpses == 2) // Fade out
                {
                  var convar = Core.ConVar.Find<float>("spec_freeze_deathanim_time");
                  float duration = convar != null ? convar.Value : 0.8f; // seconds
                  float interval = 0.1f; // 100 ms = 0.1 seconds
                  int steps = (int)Math.Ceiling(duration / interval);
                  float stepAlpha = 255f / steps; // Decremento por step

                  if (!playerPawn.IsValid) return;

                  // Obtener el color original
                  var currentColor = playerPawn.Render;
                  float currentAlpha = currentColor.A;

                  // Iniciar fade out recursivo con Delay
                  Action fadeAction = null!;
                  fadeAction = () =>
                        {
                          // Decrementar alpha
                          currentAlpha = Math.Max(0, currentAlpha - stepAlpha);

                          // Asignar el color modificado (solo alpha cambia)
                          playerPawn.Render = new Color(currentColor.R, currentColor.G, currentColor.B, (byte)currentAlpha);
                          playerPawn.RenderUpdated();

                          // Si alpha > 0, reprogramar el siguiente fade
                          if (currentAlpha > 0)
                          {
                            Core.Scheduler.DelayBySeconds(interval, fadeAction);
                          }
                        };

                  // Iniciar el primer delay
                  Core.Scheduler.DelayBySeconds(interval, fadeAction);
                }
              });
            }
          }
        }

        // Then handle killfeed
        if (_config?.HideKillFeed == 1)
        {
          @event.DontBroadcast = true;
          return HookResult.Continue;
        }
        else if (_config?.HideKillFeed == 2)
        {
          if (!attacker.IsFakeClient)
          {
            Core.GameEvent.FireToPlayer<EventPlayerDeath>(attacker.PlayerID, ev =>
            {
              ev.UserId = @event.UserId;
              ev.Attacker = @event.Attacker;
              ev.Weapon = @event.Weapon;
              ev.Headshot = @event.Headshot;
              ev.Assister = @event.Assister;
              ev.Penetrated = @event.Penetrated;
              ev.Dominated = @event.Dominated;
              ev.Revenge = @event.Revenge;
            });
          }
          return HookResult.Stop;
        }
        return HookResult.Continue;
      });
    }

    // --- Handle Hook Creation: Blood ---
    if (_bloodHookGuid.HasValue)
    {
      Core.NetMessage.Unhook(_bloodHookGuid.Value);
      _bloodHookGuid = null;
    }
    if (_config?.HideBlood == true)
    {
      _bloodHookGuid = Core.NetMessage.HookServerMessage<CMsgTEWorldDecal>((msg) =>
      {
        msg.Recipients.RemoveAllPlayers();
        return HookResult.Stop;
      });
    }

    // --- Handle Hook Creation: Headshot Sparks ---
    if (_sparksHookGuid.HasValue)
    {
      Core.NetMessage.Unhook(_sparksHookGuid.Value);
      _sparksHookGuid = null;
    }
    if (_config?.HideHeadshotSparks == true)
    {
      _sparksHookGuid = Core.NetMessage.HookServerMessage<CMsgTEEffectDispatch>((msg) =>
      {
        msg.Recipients.RemoveAllPlayers();
        return HookResult.Stop;
      });
    }

    // --- Handle Hook Creation: Legs ---
    if (_legsHookGuid.HasValue)
    {
      Core.GameEvent.Unhook(_legsHookGuid.Value);
      _legsHookGuid = null;
    }
    if (_config?.HideLegs == true || _config?.HideLegs == false)
    {
      _legsHookGuid = Core.GameEvent.HookPost<EventPlayerSpawn>(@event =>
      {
        var player = Core.PlayerManager.GetPlayer(@event.UserId);
        if (player != null && player.IsValid)
        {
          var playerPawn = player.PlayerPawn;
          if (playerPawn != null && _config != null && _config.HideLegs)
          {
            // Obtener el color actual y restaurar solo el alpha a 254 (para ocultar piernas)
            var currentColor = playerPawn.Render;
            playerPawn.Render = new Color(currentColor.R, currentColor.G, currentColor.B, (byte)254);
            playerPawn.RenderUpdated();
          }
          else if (playerPawn != null && _config != null && _config.HideLegs == false)
          {
            // Restaurar visibilidad completa
            var currentColor = playerPawn.Render;
            playerPawn.Render = new Color(currentColor.R, currentColor.G, currentColor.B, (byte)255);
            playerPawn.RenderUpdated();
          }
        }
        return HookResult.Continue;
      });
    }

    // --- Handle Hook Creation: Aim Punch ---
    if (_aimPunchHookGuid.HasValue)
    {
      Core.GameEvent.Unhook(_aimPunchHookGuid.Value);
      _aimPunchHookGuid = null;
    }
    if (_config?.DisableAimPunch > 0)
    {
      _aimPunchHookGuid = Core.GameEvent.HookPre<EventBulletDamage>(@event =>
      {
        if (_config == null || _config.DisableAimPunch == 0) return HookResult.Continue;

        if (_config.DisableAimPunch == 1 || (_config.DisableAimPunch >= 2 && _aimPunchEnabled))
        {
          var VictimPawn = Core.PlayerManager.GetPlayer(@event.Victim).PlayerPawn;

          if (VictimPawn != null && VictimPawn.IsValid)
          {
            @event.AimPunchX = 0;
            @event.AimPunchY = 0;
            @event.AimPunchZ = 0;
          }
        }
        return HookResult.Continue;
      });
    }

    // --- Handle Command Creation: Toggle Aim Punch ---
    if (_toggleAimPunchCommandGuid.HasValue)
    {
      Core.Command.UnregisterCommand(_toggleAimPunchCommandGuid.Value);
      _toggleAimPunchCommandGuid = null;
    }
    if (_config?.DisableAimPunch == 2 || _config?.DisableAimPunch == 3)
    {
      _aimPunchEnabled = _config.DisableAimPunch == 2; // true for 2 (default ON), false for 3 (default OFF)
      _toggleAimPunchCommandGuid = Core.Command.RegisterCommand("toggleaimpunch", (context) =>
      {
        _aimPunchEnabled = !_aimPunchEnabled;
        context.Reply($"Aim punch {(_aimPunchEnabled ? "enabled" : "disabled")}.");
      });
    }

    // --- Handle Hook Creation: MVPMusic ---
    if (_mvpMusicHookGuid.HasValue)
    {
      Core.GameEvent.Unhook(_mvpMusicHookGuid.Value);
      _mvpMusicHookGuid = null;
    }
    if (_config?.SoundsMuteMVPMusic > 0)
    {
      _mvpMusicHookGuid = Core.GameEvent.HookPre<EventRoundMvp>(@event =>
      {
        if (_config == null || _config.SoundsMuteMVPMusic == 0) return HookResult.Continue;

        if (_config.SoundsMuteMVPMusic == 1)
        {
          @event.MusickItID = 0;
        }
        else if (_config.SoundsMuteMVPMusic == 2)
        {
          @event.NoMusic = 1;
        }
        return HookResult.Continue;
      });
    }

    // --- Handle Hook Creation: Ignore Bomb Planted HUD Messages ---
    if (_ignoreBombPlantedHUDMessagesHookGuid.HasValue)
    {
      Core.GameEvent.Unhook(_ignoreBombPlantedHUDMessagesHookGuid.Value);
      _ignoreBombPlantedHUDMessagesHookGuid = null;
    }
    if (_config?.IgnoreBombPlantedHUDMessages == true)
    {
      _ignoreBombPlantedHUDMessagesHookGuid = Core.GameEvent.HookPre<EventBombPlanted>(@event =>
      {
        return HookResult.Stop;
      });
    }

    // --- Handle Hook Creation: Ignore Teammate Attack Messages ---
    if (_ignoreTeammateAttackMessagesHookGuid.HasValue)
    {
      Core.GameEvent.Unhook(_ignoreTeammateAttackMessagesHookGuid.Value);
      _ignoreTeammateAttackMessagesHookGuid = null;
    }
    if (_config?.IgnoreTeammateAttackMessages == true)
    {
      _ignoreTeammateAttackMessagesHookGuid = Core.GameEvent.HookPre<EventPlayerHurt>(@event =>
      {
        var attacker = Core.PlayerManager.GetPlayer(@event.Attacker);
        var victim = Core.PlayerManager.GetPlayer(@event.UserId);
        if (attacker != null && victim != null && attacker.IsValid && victim.IsValid &&
                  attacker.Controller.TeamNum == victim.Controller.TeamNum)
        {
          return HookResult.Stop;
        }
        return HookResult.Continue;
      });
    }

    // --- Handle Hook Creation: Ignore Awards Money Messages ---
    if (_ignoreAwardsMoneyMessagesHookGuid.HasValue)
    {
      Core.GameEvent.Unhook(_ignoreAwardsMoneyMessagesHookGuid.Value);
      _ignoreAwardsMoneyMessagesHookGuid = null;
    }
    if (_config?.IgnoreAwardsMoneyMessages == true)
    {
      _ignoreAwardsMoneyMessagesHookGuid = Core.NetMessage.HookClientMessage<CUserMessageTextMsg>((msg, playerId) =>
      {
        return Helper.FilterMessageByParams(msg, Helper.MoneyMessageArray);
      });
    }

    // --- Handle Hook Creation: Ignore Player Saved You Messages ---
    if (_ignorePlayerSavedYouMessagesHookGuid.HasValue)
    {
      Core.GameEvent.Unhook(_ignorePlayerSavedYouMessagesHookGuid.Value);
      _ignorePlayerSavedYouMessagesHookGuid = null;
    }
    if (_config?.IgnorePlayerSavedYouMessages == true)
    {
      _ignorePlayerSavedYouMessagesHookGuid = Core.NetMessage.HookClientMessage<CUserMessageTextMsg>((msg, playerId) =>
      {
        return Helper.FilterMessageByParams(msg, Helper.SavedbyArray);
      });
    }

    // --- Handle Hook Creation: Ignore Chicken Killed Messages ---
    if (_ignoreChickenKilledMessagesHookGuid.HasValue)
    {
      Core.GameEvent.Unhook(_ignoreChickenKilledMessagesHookGuid.Value);
      _ignoreChickenKilledMessagesHookGuid = null;
    }
    if (_config?.IgnoreChickenKilledMessages == true)
    {
      _ignoreChickenKilledMessagesHookGuid = Core.NetMessage.HookClientMessage<CUserMessageTextMsg>((msg, playerId) =>
      {
        return Helper.FilterMessageByParams(msg, Helper.ChickenMessageArray);
      });
    }

    // --- Handle Hook Creation: Ignore Join Team Messages ---
    if (_ignoreJoinTeamMessagesHookGuid.HasValue)
    {
      Core.GameEvent.Unhook(_ignoreJoinTeamMessagesHookGuid.Value);
      _ignoreJoinTeamMessagesHookGuid = null;
    }
    if (_config?.IgnoreJoinTeamMessages == true)
    {
      _ignoreJoinTeamMessagesHookGuid = Core.GameEvent.HookPre<EventPlayerTeam>(@event =>
      {
        @event.DontBroadcast = true;
        return HookResult.Continue;
      });
    }

    // --- Handle Hook Creation: Ignore Planting Bomb Messages ---
    if (_ignorePlantingBombMessagesHookGuid.HasValue)
    {
      Core.GameEvent.Unhook(_ignorePlantingBombMessagesHookGuid.Value);
      _ignorePlantingBombMessagesHookGuid = null;
    }
    if (_config?.IgnorePlantingBombMessages == true)
    {
      _ignorePlantingBombMessagesHookGuid = Core.NetMessage.HookClientMessage<CUserMessageTextMsg>((msg, playerId) =>
      {
        return Helper.FilterMessageByParams(msg, Helper.PlantingBombMessageArray);
      });
    }

    // --- Handle Hook Creation: Ignore Defusing Bomb Messages ---
    if (_ignoreDefusingBombMessagesHookGuid.HasValue)
    {
      Core.GameEvent.Unhook(_ignoreDefusingBombMessagesHookGuid.Value);
      _ignoreDefusingBombMessagesHookGuid = null;
    }
    if (_config?.IgnoreDefusingBombMessages == true)
    {
      _ignoreDefusingBombMessagesHookGuid = Core.NetMessage.HookClientMessage<CUserMessageTextMsg>((msg, playerId) =>
      {
        return Helper.FilterMessageByParams(msg, Helper.DefusingBombMessageArray);
      });
    }

    // --- Handle Hook Creation: Ignore Disconnect Messages ---
    if (_ignoreDisconnectMessagesHookGuid.HasValue)
    {
      Core.GameEvent.Unhook(_ignoreDisconnectMessagesHookGuid.Value);
      _ignoreDisconnectMessagesHookGuid = null;
    }
    if (_config?.IgnoreDisconnectMessages == true)
    {
      _ignoreDisconnectMessagesHookGuid = Core.GameEvent.HookPre<EventPlayerDisconnect>(@event =>
      {
        @event.DontBroadcast = true;
        return HookResult.Continue;
      });
    }
  }

  private void CheckClientCommands()
  {
    // Lista de predicados para verificar si un comando debe bloquearse
    var commandBlockers = new List<Func<string, ConfigModel?, bool>>
    {
      (cmd, cfg) => cmd == "playerchatwheel" && cfg?.BlockChatWheel == true,
      (cmd, cfg) => cmd == "player_ping" && cfg?.BlockPing == true,
      (cmd, cfg) => cmd.StartsWith("+radialradio") && cfg?.BlockChatWheel == true
    };

    Core.Command.HookClientCommand((playerId, commandLine) =>
    {
      var commandName = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

      // Verificar todos los bloqueadores en la lista
      foreach (var blocker in commandBlockers)
      {
        if (blocker(commandName, _config))
        {
          return HookResult.Stop;
        }
      }

      // Verificar comandos bloqueados personalizados
      if (_config?.BlockedCommands.Contains(commandName) == true)
      {
        return HookResult.Stop;
      }

      return HookResult.Continue;
    });
  }

  private void ExecuteNativeCommands()
  {
    var cfg = _config!;
    if (cfg.BlockBotRadio)
    {
      Core.Engine.ExecuteCommand("bot_chatter off");
    }
    if (cfg.BlockGrenadesRadio)
    {
      Core.Engine.ExecuteCommand("sv_ignoregrenaderadio 1");
    }
    if (cfg.BlockedCommandsWhitelist.Length > 0)
    {
      // TODO
    }
    if (cfg.HideRadar)
    {
      Core.Engine.ExecuteCommand("sv_disable_radar 1");
    }
    if (cfg.HideTeammateHeadtags == 1)
    {
      Core.Engine.ExecuteCommand("sv_teamid_overhead 0");
    }
    if (cfg.HideTeammateHeadtags == 2)
    {
      Core.Engine.ExecuteCommand("sv_teamid_overhead 1; sv_teamid_overhead_always_prohibit 1; sv_teamid_overhead_maxdist 0");
    }
    if (cfg.HideTeammateHeadtags == 3 && cfg.HideTeammateHeadtags_Distance > 0)
    {
      Core.Engine.ExecuteCommand($"sv_teamid_overhead 1; sv_teamid_overhead_always_prohibit 1; sv_teamid_overhead_maxdist {cfg.HideTeammateHeadtags_Distance}");
    }
    if (cfg.DisableFallDamage)
    {
      Core.Engine.ExecuteCommand("sv_falldamage_scale 0");
    }
    if (cfg.DisableSVCheats)
    {
      Core.Engine.ExecuteCommand("sv_cheats 0");
    }
    if (cfg.DisableC4)
    {
      Core.Engine.ExecuteCommand("mp_give_player_c4 0");
    }
    if (cfg.DisableCameraSpectator)
    {
      Core.Engine.ExecuteCommand("sv_disable_observer_interpolation true");
    }
    if (cfg.DisableAimPunch_Flags.Count > 0)
    {
      // TODO
    }
    if (cfg.SoundsMuteFootsteps)
    {
      Core.Engine.ExecuteCommand("sv_footsteps 0");
    }
    if (cfg.SoundsMuteJumpLand)
    {
      Core.Engine.ExecuteCommand("sv_min_jump_landing_sound 999999");
    }
  }
}