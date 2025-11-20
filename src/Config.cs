
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
    // Model created from what I consider useful configuration options, PR are welcome to add more!
    public class ConfigModel
    {
        // === Blockers ===
        public bool BlockRadio { get; set; } = false;
        public bool BlockBotRadio { get; set; } = false;
        public bool BlockGrenadesRadio { get; set; } = false;
        public bool BlockChatWheel { get; set; } = false;
        public bool BlockPing { get; set; } = false;
        public byte BlockNameChanger { get; set; } = 0; // 0 = No, 1 = Just Warn, 2 = Transfer to SPEC, 3 = Kick
        public List<string> BlockedCommands { get; set; } = []; // Case Sensitive
        public string BlockedCommandsWhitelist { get; set; } = ""; // Flags

        // === Hide ===
        public bool HideRadar { get; set; } = false;
        public byte HideKillFeed { get; set; } = 0; // 0 = No, 1 = Full, 2 = Show only own kills
        public bool HideBlood { get; set; } = false;
        public bool HideHeadshotSparks { get; set; } = false;
        public byte HideTeammateHeadtags { get; set; } = 0; // 0 = No, 1 = Yes, 2 = Just behind walls, 3 = Disable by distance
        public byte HideTeammateHeadtags_Distance { get; set; } = 0; // 50, 100, 150, 250
        public byte HideCorpses { get; set; } = 0; // 0 = No, 1 = Instantly, 2 = After X seconds
        public byte HideCorpses_DelaySeconds { get; set; } = 5; // 5, 10, 15, 30
        public bool HideLegs { get; set; } = false;
        public byte HideChatHUD { get; set; } = 0; // 0 = No, 1 = Yes, 2 = Yes with delay
        public byte HideChatHUD_DelaySeconds { get; set; } = 5; // 5, 10, 15, 30
        public bool HideWeaponsHUD { get; set; } = false;

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
        public byte SoundsMuteKnife { get; set; } = 0; // 0 = No, 1 = Yes, 2 = Only teammates
        public List<uint> Sounds_MuteKnife_SoundeventHash { get; set; } = new List<uint>
        {
            427534867,
            3475734633,
            1769891506
        };

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

        // === Auto Clean Dropped Weapons ===
        public bool AutoClean_Enable { get; set; } = false;
        public int AutoClean_Timer { get; set; } = 10; // Seconds 1-999
        public int AutoClean_MaxWeaponsOnGround { get; set; } = 5; // Start cleaning when exceeding this number
        public List<string> AutoClean_TheseDroppedWeaponsOnly { get; set; } = []; // If empty, clean all

        /*
        Weapons List Reference:
            A: AWP, G3SG1, SCAR-20, SSG 08
            B: AK-47, AUG, FAMAS, Galil, M4 variants
            C: M249, Negev
            D: Mag-7, Nova, Sawed-off, XM1014
            E: Bizon, MAC-10, MP5, MP7, MP9, P90, UMP-45
            F: All pistols
            G: All grenades
            H: Defuse kits
            I: Zeus
            J: Healthshot
            K: Knives
        */

    }

    private static readonly Dictionary<string, List<string>> WeaponCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        ["A"] = ["weapon_awp", "weapon_g3sg1", "weapon_scar20", "weapon_ssg08"],
        ["B"] = ["weapon_ak47", "weapon_aug", "weapon_famas", "weapon_galilar", "weapon_m4a1", "weapon_m4a1_silencer"],
        ["C"] = ["weapon_m249", "weapon_negev"],
        ["D"] = ["weapon_mag7", "weapon_nova", "weapon_sawedoff", "weapon_xm1014"],
        ["E"] = ["weapon_bizon", "weapon_mac10", "weapon_mp5sd", "weapon_mp7", "weapon_mp9", "weapon_p90", "weapon_ump45"],
        ["F"] = ["weapon_deagle", "weapon_elite", "weapon_fiveseven", "weapon_glock", "weapon_hkp2000", "weapon_p250", "weapon_tec9", "weapon_usp_silencer", "weapon_cz75a", "weapon_revolver"],
        ["G"] = ["weapon_flashbang", "weapon_hegrenade", "weapon_smokegrenade", "weapon_molotov", "weapon_incgrenade", "weapon_decoy"],
        ["H"] = ["item_defuser"],
        ["I"] = ["weapon_taser"],
        ["J"] = ["weapon_healthshot"],
        ["K"] = ["weapon_knife", "weapon_knife_t"]
    };

    private ConfigModel? _config;

    // Guardar el Guid para poder desregistrar
    private Guid? _radioHookGuid;
    private Guid? _nameChangerHookGuid;
    private Guid? _killFeedHookGuid;
    private Guid? _bloodHookGuid;
    private Guid? _sparksHookGuid;
    private Guid? _corpsesHookGuid;
    private Guid? _legsHookGuid;
    private Guid? _chatHudHookGuid;
    private Guid? _weaponHudHookGuid;
    private Guid? _aimPunchHookGuid;
    private Guid? _toggleAimPunchCommandGuid;
    private bool _aimPunchEnabled;
    private Guid? _mvpMusicHookGuid;
    private Guid? _knifeSoundHookGuid;
    public Guid? _knifeSoundMessageHookGuid;
    private HashSet<int> _playersToMuteKnifeSound = new();
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
        // Desregistrar si ya existe o registrar según configuración
        if (_radioHookGuid.HasValue)
        {
            Core.GameEvent.Unhook(_radioHookGuid.Value);
            _radioHookGuid = null;
        }
        if (_config?.BlockRadio == true)
        {
            _radioHookGuid = Core.GameEvent.HookPost<EventPlayerRadio>(@event => HookResult.Stop);
        }

        // Desregistrar si ya existe o registrar según configuración
        if (_nameChangerHookGuid.HasValue)
        {
            Core.GameEvent.Unhook(_nameChangerHookGuid.Value);
            _nameChangerHookGuid = null;
        }
        if (_config?.BlockNameChanger > 0)
        {
            _nameChangerHookGuid = Core.GameEvent.HookPost<EventPlayerChangename>(@event =>
            {
                // 1 = Solo advertir, 2 = Transferir a SPEC, 3 = Kickear
                if (_config?.BlockNameChanger == 1)
                {
                    // Lógica de advertencia: mensaje solo al jugador
                    Core.PlayerManager.GetPlayer(@event.UserId).SendMessage(MessageType.Chat, "No cambies tu nombre!");
                    return HookResult.Stop;
                }
                else if (_config?.BlockNameChanger == 2)
                {
                    Core.PlayerManager.GetPlayer(@event.UserId).ChangeTeam(Team.Spectator);
                    Core.PlayerManager.GetPlayer(@event.UserId).SendMessage(MessageType.Chat, "Has sido transferido a espectador por cambiar tu nombre.");
                    return HookResult.Stop;
                }
                else if (_config?.BlockNameChanger == 3)
                {
                    // Lógica para kickear
                    Core.PlayerManager.GetPlayer(@event.UserId).Kick("No se permite cambiar el nombre", ENetworkDisconnectionReason.NETWORK_DISCONNECT_KICKED);
                    return HookResult.Stop;
                }
                return HookResult.Continue;
            });
        }

        // Desregistrar si ya existe o registrar según configuración
        if (_killFeedHookGuid.HasValue)
        {
            Core.GameEvent.Unhook(_killFeedHookGuid.Value);
            _killFeedHookGuid = null;
        }
        if (_config?.HideKillFeed > 0)
        {
            _killFeedHookGuid = Core.GameEvent.HookPre<EventPlayerDeath>(@event =>
            {
                if (_config?.HideKillFeed == 1)
                {
                    var attacker = Core.PlayerManager.GetPlayer(@event.Attacker);
                    if (attacker == null || !attacker.IsValid) return HookResult.Continue;

                    @event.DontBroadcast = true;
                    return HookResult.Stop;
                }
                else if (_config?.HideKillFeed == 2)
                {
                    Core.GameEvent.Fire<EventPlayerDeath>(e => e.Attacker = @event.Attacker);
                }
                return HookResult.Continue;
            });
        }

        // Desregistrar y registrar hook de sangre
        if (_bloodHookGuid.HasValue)
        {
            Core.GameEvent.Unhook(_bloodHookGuid.Value);
            _bloodHookGuid = null;
        }
        if (_config?.HideBlood == true)
        {
            _bloodHookGuid = Core.NetMessage.HookClientMessage<CMsgTEBloodStream>((msg, playerId) =>
            {
                if (_config?.HideBlood == true) return HookResult.Stop;
                return HookResult.Continue;
            });
        }

        // Desregistrar y registrar hook de chispas headshot
        if (_sparksHookGuid.HasValue)
        {
            Core.GameEvent.Unhook(_sparksHookGuid.Value);
            _sparksHookGuid = null;
        }
        if (_config?.HideHeadshotSparks == true)
        {
            _sparksHookGuid = Core.NetMessage.HookClientMessage<CMsgTESparks>((msg, playerId) =>
            {
                if (_config?.HideHeadshotSparks == true) return HookResult.Stop;
                return HookResult.Continue;
            });
        }

        // Desregistrar hook de cadáveres
        if (_corpsesHookGuid.HasValue)
        {
            Core.GameEvent.Unhook(_corpsesHookGuid.Value);
            _corpsesHookGuid = null;
        }
        if (_config?.HideCorpses > 0)
        {
            _corpsesHookGuid = Core.GameEvent.HookPost<EventPlayerDeath>(@event =>
            {
                Core.Scheduler.NextTick(() =>
                {
                    var player = Core.PlayerManager.GetPlayer(@event.UserId);
                    if (player != null && player.IsValid)
                    {
                        if (_config?.HideCorpses == 1)
                        {
                            // Lógica para esconder cadáveres instantáneamente
                        }
                        if (_config?.HideCorpses == 2) // Fade out
                        {
                            var convar = Core.ConVar.Find<int>("spec_freeze_deathanim_time");
                            float duration = convar != null ? convar.Value : 0.8f; // seconds
                            float interval = 0.1f; // 100 ms = 0.1 seconds
                            int steps = (int)Math.Ceiling(duration / interval);
                            float stepAlpha = 255f / steps; // Decremento por step

                            var playerPawn = player.PlayerPawn;
                            if (playerPawn != null)
                            {
                                var PlayerPawnValue = playerPawn; // Ya es la instancia
                                if (PlayerPawnValue == null || !PlayerPawnValue.IsValid) return;

                                // Obtener el color original
                                var originalColor = PlayerPawnValue.Render;
                                float currentAlpha = originalColor.A;

                                // Iniciar fade out recursivo con Delay
                                Action fadeAction = null!;
                                fadeAction = () =>
                                {
                                    // Decrementar alpha
                                    currentAlpha = Math.Max(0, currentAlpha - stepAlpha);

                                    // Asignar el color modificado (solo alpha cambia)
                                    PlayerPawnValue.Render = new Color(originalColor.R, originalColor.G, originalColor.B, (byte)currentAlpha);
                                    // PlayerPawnValue.ForceUpdated();

                                    // Si alpha > 0, reprogramar el siguiente fade
                                    if (currentAlpha > 0)
                                    {
                                        Core.Scheduler.DelayBySeconds(interval, fadeAction);
                                    }
                                };

                                // Iniciar el primer delay
                                Core.Scheduler.DelayBySeconds(interval, fadeAction);
                            }
                        }
                    }
                });
                return HookResult.Continue;
            });
        }

        // Desregistrar hook de spawn
        if (_legsHookGuid.HasValue)
        {
            Core.GameEvent.Unhook(_legsHookGuid.Value);
            _legsHookGuid = null;
        }
        if (_config?.HideLegs == true)
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
                        // playerPawn.ForceUpdated();
                    }
                    else if (playerPawn != null && _config != null && _config.HideLegs == false)
                    {
                        // Restaurar visibilidad completa
                        var currentColor = playerPawn.Render;
                        playerPawn.Render = new Color(currentColor.R, currentColor.G, currentColor.B, (byte)255);
                        // playerPawn.ForceUpdated();
                    }
                }
                return HookResult.Continue;
            });
        }

        // Desregistrar hook de HUD
        if (_chatHudHookGuid.HasValue)
        {
            Core.GameEvent.Unhook(_chatHudHookGuid.Value);
            _chatHudHookGuid = null;
        }
        if (_config?.HideChatHUD > 0)
        {
            _chatHudHookGuid = Core.NetMessage.HookClientMessage<CUserMessageHudText>((msg, playerId) =>
            {
                if (_config?.HideChatHUD > 0)
                {
                    HideInHUD(Core.PlayerManager.GetPlayer(playerId), 128);
                    return HookResult.Stop;
                }
                return HookResult.Continue;
            });
        }

        // Desregistrar y registrar hook de HUD de armas
        if (_weaponHudHookGuid.HasValue)
        {
            Core.GameEvent.Unhook(_weaponHudHookGuid.Value);
            _weaponHudHookGuid = null;
        }
        if (_config?.HideWeaponsHUD == true)
        {
            _weaponHudHookGuid = Core.NetMessage.HookClientMessage<CUserMessageHudMsg>((msg, playerId) =>
            {
                if (_config?.HideWeaponsHUD == true)
                {
                    HideInHUD(Core.PlayerManager.GetPlayer(playerId), 64);
                    return HookResult.Stop;
                }
                return HookResult.Continue;
            });
        }

        // Desregistrar y registrar hook de Aim Punch
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

        // Desregistrar y registrar comando toggle aim punch
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

        // Desregistrar y registrar hook de MVPMusic
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

        // Desregistrar y registrar hook de knife sounds
        if (_knifeSoundHookGuid.HasValue)
        {
            Core.GameEvent.Unhook(_knifeSoundHookGuid.Value);
            _knifeSoundHookGuid = null;
        }
        if (_config?.SoundsMuteKnife > 0)
        {
            _knifeSoundHookGuid = Core.GameEvent.HookPre<EventPlayerHurt>(@event =>
            {
                // Check if damage is from knife
                if (@event.Weapon.Contains("knife"))
                {
                    // For SoundsMuteKnife == 1: mute all knife sounds
                    // For == 2: mute only if attacker is teammate
                    bool shouldMute = _config.SoundsMuteKnife == 1 ||
                        (_config.SoundsMuteKnife == 2 && Core.PlayerManager.GetPlayer(@event.Attacker).Controller.TeamNum == Core.PlayerManager.GetPlayer(@event.UserId).Controller.TeamNum);

                    if (shouldMute)
                    {
                        _playersToMuteKnifeSound.Add(@event.UserId);
                    }
                }
                return HookResult.Continue;
            });
        }

        // Desregistrar y registrar hook de knife sound messages
        if (_knifeSoundMessageHookGuid.HasValue)
        {
            // Core.NetMessage.UnhookClientMessage(_knifeSoundMessageHookGuid.Value); // No unhook method
            _knifeSoundMessageHookGuid = null;
        }
        if (_config?.SoundsMuteKnife > 0)
        {
            _knifeSoundMessageHookGuid = Core.NetMessage.HookClientMessage<CSVCMsg_Sounds>((msg, playerId) =>
            {
                if (_playersToMuteKnifeSound.Contains(playerId) && msg.Sounds.Any(sound => _config.Sounds_MuteKnife_SoundeventHash.Contains(sound.Guid)))
                {
                    return HookResult.Stop;
                }
                return HookResult.Continue;
            });
        }

        // Desregistrar y registrar hook de ignorar mensajes HUD de bomba plantada
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

        // Desregistrar y registrar 
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

        // Desregistrar y registrar
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

        // Desregistrar y registrar
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

        // Desregistrar y registrar
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

        // Desregistrar y registrar
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

        // Desregistrar y registrar
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

        // Desregistrar y registrar
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

        // Desregistrar y registrar
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

    private static void HideInHUD(IPlayer player, uint msgId)
    {
        if (player == null || !player.IsValid) return;

        var PlayerPawn = player.PlayerPawn;
        if (PlayerPawn == null || !PlayerPawn.IsValid) return;

        if (msgId != 0)
        {
            ref uint hud = ref PlayerPawn.HideHUD;
            hud = msgId;
            PlayerPawn.HideHUDUpdated();
        }
    }

    private void CheckClientCommands()
    {
        // Hook global para comandos de cliente usando ICommandService
        var commandBlockers = new Dictionary<string, Func<ConfigModel?, bool>>
        {
            { "playerchatwheel", cfg => cfg?.BlockChatWheel == true },
            { "player_ping", cfg => cfg?.BlockPing == true }
        };

        _commandService?.HookClientCommand((playerId, commandLine) =>
        {
            var commandName = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
            if (commandBlockers.TryGetValue(commandName, out var shouldBlock) && shouldBlock(_config))
            {
                return HookResult.Stop;
            }
            return HookResult.Continue;
        });

        // Hook para comandos bloqueados específicos
        if (_config?.BlockedCommands.Count > 0)
        {
            _commandService?.HookClientCommand((playerId, commandLine) =>
            {
                var commandName = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                if (_config.BlockedCommands.Contains(commandName))
                {
                    return HookResult.Stop;
                }
                return HookResult.Continue;
            });
        }
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
        if (cfg.AutoClean_Enable)
        {
            Core.Scheduler.NextTick(() =>
            {
                Core.Scheduler.DelayBySeconds(cfg.AutoClean_Timer, () =>
                {
                    var selectedWeapons = cfg.AutoClean_TheseDroppedWeaponsOnly
                        .Select(w => w.Trim().ToLower())
                        .ToList();

                    var allWeaponsToClean = new HashSet<string>();

                    if (selectedWeapons.Contains("any") || selectedWeapons.Count == 0)
                    {
                        foreach (var category in WeaponCategories.Values)
                        {
                            allWeaponsToClean.UnionWith(category);
                        }
                    }
                    else
                    {
                        foreach (var weaponKey in selectedWeapons)
                        {
                            if (WeaponCategories.ContainsKey(weaponKey.ToUpper()))
                            {
                                allWeaponsToClean.UnionWith(WeaponCategories[weaponKey.ToUpper()]);
                            }
                            else
                            {
                                allWeaponsToClean.Add(weaponKey.ToLower());
                            }
                        }
                    }

                    var droppedWeapons = new List<CEntityInstance>();

                    foreach (var weaponClass in allWeaponsToClean)
                    {
                        var entities = Core.EntitySystem.GetAllEntities().Where(e => e.DesignerName == weaponClass);
                        foreach (var entity in entities)
                        {
                            if (entity != null && entity.IsValid) // Assuming dropped if no owner check available
                            {
                                droppedWeapons.Add(entity);
                            }
                        }
                    }

                    if (droppedWeapons.Count > cfg.AutoClean_MaxWeaponsOnGround)
                    {
                        int weaponsToRemove = droppedWeapons.Count - cfg.AutoClean_MaxWeaponsOnGround;

                        for (int i = 0; i < weaponsToRemove && droppedWeapons.Count > 0; i++)
                        {
                            int weaponToRemoveIndex = droppedWeapons.Count == 1 ? 0 : Random.Shared.Next(0, droppedWeapons.Count);

                            var weaponToRemove = droppedWeapons[weaponToRemoveIndex];
                            if (weaponToRemove == null || !weaponToRemove.IsValid) continue;

                            weaponToRemove.AcceptInput("Kill", "");
                            droppedWeapons.RemoveAt(weaponToRemoveIndex);
                        }
                    }
                });
            });
        }
        if (cfg.AutoClean_Timer < 1 || cfg.AutoClean_Timer > 999)
        {
            // Implement validation for auto clean timer logic here
        }
        if (cfg.AutoClean_MaxWeaponsOnGround > 0)
        {
            // Implement auto clean max weapons on ground logic here
        }
        if (cfg.AutoClean_TheseDroppedWeaponsOnly.Count > 0)
        {
            // Implement auto clean specific dropped weapons logic here
        }
    }
}