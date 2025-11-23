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

    // === Sounds ===
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
  private Guid? _ignoreBombPlantedHUDMessagesHookGuid;
  private Guid? _ignoreTextMessagesHookGuid;
  private Guid? _ignoreHintMessagesHookGuid;
  private Guid? _ignoreRadioTextMessagesHookGuid;
  private Guid? _ignoreJoinTeamMessagesHookGuid;
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

                  // Get original color and alpha
                  var currentColor = playerPawn.Render;
                  float currentAlpha = currentColor.A;

                  // Start recursive fade out with Delay
                  Action fadeAction = null!;
                  fadeAction = () =>
                        {
                          // Reduce alpha
                          currentAlpha = Math.Max(0, currentAlpha - stepAlpha);

                          // Assign modified color (only alpha changes)
                          playerPawn.Render = new Color(currentColor.R, currentColor.G, currentColor.B, (byte)currentAlpha);
                          playerPawn.RenderUpdated();

                          // If alpha > 0, schedule the next fade
                          if (currentAlpha > 0)
                          {
                            Core.Scheduler.DelayBySeconds(interval, fadeAction);
                          }
                        };

                  // Start the first delay
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
        @event.DontBroadcast = true;
        return HookResult.Continue;
      });
    }

    // --- Handle Hook Creation: Ignore Text Messages (Unified for CUserMessageTextMsg) ---
    if (_ignoreTextMessagesHookGuid.HasValue)
    {
      Core.NetMessage.Unhook(_ignoreTextMessagesHookGuid.Value);
      _ignoreTextMessagesHookGuid = null;
    }
    if (_config?.IgnoreAwardsMoneyMessages == true || _config?.IgnorePlayerSavedYouMessages == true || _config?.IgnoreChickenKilledMessages == true || _config?.IgnorePlantingBombMessages == true || _config?.IgnoreDefusingBombMessages == true || _config?.IgnoreTeammateAttackMessages == true)
    {
      _ignoreTextMessagesHookGuid = Core.NetMessage.HookServerMessage<CUserMessageTextMsg>((msg) =>
      {
        if (msg.Param == null)
        {
          return HookResult.Continue;
        }
        for (int i = 0; i < msg.Param.Count; i++)
        {
          var param = msg.Param[i];
          if (string.IsNullOrEmpty(param))
          {
            continue;
          }

          if (_config?.IgnoreAwardsMoneyMessages == true && Helper.MoneyMessageArray.Contains(param))
          {
            return HookResult.Stop;
          }

          if (_config?.IgnorePlayerSavedYouMessages == true && Helper.SavedbyArray.Contains(param))
          {
            return HookResult.Stop;
          }

          if (_config?.IgnoreChickenKilledMessages == true && param.Contains("#Pet_Killed"))
          {
            return HookResult.Stop;
          }

          if (_config?.IgnorePlantingBombMessages == true && Helper.PlantingBombMessageArray.Contains(param))
          {
            return HookResult.Stop;
          }

          if (_config?.IgnoreDefusingBombMessages == true && Helper.DefusingBombMessageArray.Contains(param))
          {
            return HookResult.Stop;
          }

          if (_config?.IgnoreTeammateAttackMessages == true && Helper.TeamWarningArray.Contains(param))
          {
            return HookResult.Stop;
          }
        }
        return HookResult.Continue;
      });
    }

    // --- Handle Hook Creation: Ignore Hint Messages (for CCSUsrMsg_HintText) ---
    if (_ignoreHintMessagesHookGuid.HasValue)
    {
      Core.NetMessage.Unhook(_ignoreHintMessagesHookGuid.Value);
      _ignoreHintMessagesHookGuid = null;
    }
    if (_config?.IgnoreTeammateAttackMessages == true)
    {
      _ignoreHintMessagesHookGuid = Core.NetMessage.HookClientMessage<CCSUsrMsg_HintText>((msg, playerId) =>
      {
        var message = msg.Message;

        if (_config?.IgnoreTeammateAttackMessages == true && Helper.TeamWarningArray.Contains(message))
        {
          return HookResult.Stop;
        }

        return HookResult.Continue;
      });
    }

    // --- Handle Hook Creation: Ignore Radio Text Messages (for CCSUsrMsg_RadioText) ---
    if (_ignoreRadioTextMessagesHookGuid.HasValue)
    {
      Core.NetMessage.Unhook(_ignoreRadioTextMessagesHookGuid.Value);
      _ignoreRadioTextMessagesHookGuid = null;
    }
    if (_config?.IgnorePlantingBombMessages == true || _config?.IgnoreDefusingBombMessages == true)
    {
      _ignoreRadioTextMessagesHookGuid = Core.NetMessage.HookServerMessage<CCSUsrMsg_RadioText>((msg) =>
      {
        var msgName = msg.MsgName;

        if (_config?.IgnorePlantingBombMessages == true && msgName.Contains("#Cstrike_TitlesTXT_Planting_Bomb"))
        {
          return HookResult.Stop;
        }

        if (_config?.IgnoreDefusingBombMessages == true && msgName.Contains("#Cstrike_TitlesTXT_Defusing_Bomb"))
        {
          return HookResult.Stop;
        }

        return HookResult.Continue;
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
    // List of command blocked
    var commandBlockers = new List<Func<string, ConfigModel?, bool>>
    {
      (cmd, cfg) => cmd == "playerchatwheel" && cfg?.BlockChatWheel == true,
      (cmd, cfg) => cmd == "player_ping" && cfg?.BlockPing == true,
      (cmd, cfg) => cmd.StartsWith("+radialradio") && cfg?.BlockChatWheel == true
    };

    Core.Command.HookClientCommand((playerId, commandLine) =>
    {
      var commandName = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

      // Check all blockers in the list
      foreach (var blocker in commandBlockers)
      {
        if (blocker(commandName, _config))
        {
          return HookResult.Stop;
        }
      }

      // Check custom blocked commands
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