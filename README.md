# [SwiftlyS2] GameManager

[![GitHub Release](https://img.shields.io/github/v/release/criskkky/sws2-gamemanager?color=FFFFFF&style=flat-square)](https://github.com/criskkky/sws2-gamemanager/releases/latest)
[![GitHub Issues](https://img.shields.io/github/issues/criskkky/sws2-gamemanager?color=FF0000&style=flat-square)](https://github.com/criskkky/sws2-gamemanager/issues)
[![GitHub Downloads](https://img.shields.io/github/downloads/criskkky/sws2-gamemanager/total?color=blue&style=flat-square)](https://github.com/criskkky/sws2-gamemanager/releases)
[![GitHub Stars](https://img.shields.io/github/stars/criskkky/sws2-gamemanager?style=social)](https://github.com/criskkky/sws2-gamemanager/stargazers)

Setup your game as you want.

## Download Shortcuts
<ul>
  <li>
    <code>📦</code>
    <strong>&nbspDownload Latest Plugin Version</strong> ⇢
    <a href="https://github.com/criskkky/sws2-gamemanager/releases/latest" target="_blank" rel="noopener noreferrer">Click Here</a>
  </li>
  <li>
    <code>⚙️</code>
    <strong>&nbspDownload Latest SwiftlyS2 Version</strong> ⇢
    <a href="https://github.com/swiftly-solution/swiftlys2/releases/latest" target="_blank" rel="noopener noreferrer">Click Here</a>
  </li>
</ul>

## Features
- **Blockers**: Block radio commands, bot radio chatter, grenade radio messages, chat wheel, ping, and custom commands.
- **Hide Elements**: Hide radar, killfeed (full or own kills only), blood effects, headshot sparks, teammate headtags (with distance options), corpses (instantly or fade out), and player legs.
- **Disable Features**: Disable fall damage, sv_cheats, C4 planting, spectator camera, and aim punch (with togglable options).
- **Sound Control**: Mute MVP music, footsteps, and jump landing sounds.
- **Message Filtering**: Ignore HUD messages for bomb planted, teammate attacks, awards money, player saved you, chicken killed, join team, planting/defusing bomb, and disconnects.
- **Auto Clean Weapons**: Automatically clean dropped weapons on the ground based on timer and limits, with options for specific weapon categories.

## Screenshots
> No screenshots available yet.

## Plugin Setup
> [!WARNING]
> Make sure you **have installed SwiftlyS2 Framework** before proceeding.

1. Download and extract the latest plugin version into your `swiftlys2/plugins` folder.
2. Perform an initial run in order to allow file generation.
3. Generated file will be located at: `swiftlys2/plugins/GameManager/configs/config.jsonc`
4. Edit the configuration file as needed.
5. Enjoy!

## Configuration Guide

### Blockers
| Option | Type | Default | Description |
|--------|------|---------|-------------|
| BlockRadio | bool | false | Blocks radio commands |
| BlockBotRadio | bool | false | Blocks bot radio chatter |
| BlockGrenadesRadio | bool | false | Blocks grenade radio messages |
| BlockChatWheel | bool | false | Blocks chat wheel commands |
| BlockPing | bool | false | Blocks ping commands |
| BlockedCommands | List<string> | [] | List of commands to block (case sensitive) |
| BlockedCommandsWhitelist | string | "" | Flags for whitelisting blocked commands |

### Hide
| Option | Type | Default | Description |
|--------|------|---------|-------------|
| HideRadar | bool | false | Hides the radar |
| HideKillFeed | byte | 0 | 0 = No, 1 = Full, 2 = Show only own kills |
| HideBlood | bool | false | Hides blood effects |
| HideHeadshotSparks | bool | false | Hides headshot sparks |
| HideTeammateHeadtags | byte | 0 | 0 = No, 1 = Yes, 2 = Just behind walls, 3 = Disable by distance |
| HideTeammateHeadtags_Distance | byte | 0 | Distance for headtags (50, 100, 150, 250) |
| HideCorpses | byte | 0 | 0 = No, 1 = Instantly, 2 = Fade out |
| HideLegs | bool | false | Hides player legs |

### Disable
| Option | Type | Default | Description |
|--------|------|---------|-------------|
| DisableFallDamage | bool | false | Disables fall damage |
| DisableSVCheats | bool | false | Disables sv_cheats |
| DisableC4 | bool | false | Disables C4 planting |
| DisableCameraSpectator | bool | false | Disables spectator camera |
| DisableAimPunch | byte | 0 | 0 = No, 1 = Yes, 2 = Togglable (default ON), 3 = Togglable (default OFF) |
| DisableAimPunch_Flags | List<string> | [] | Flags for togglable aim punch |

### Sounds
| Option | Type | Default | Description |
|--------|------|---------|-------------|
| SoundsMuteMVPMusic | byte | 0 | 0 = No, 1 = Yes, 2 = MVP + Round End Music |
| SoundsMuteFootsteps | bool | false | Mutes footsteps |
| SoundsMuteJumpLand | bool | false | Mutes jump landing sounds |

### Default MSGS
| Option | Type | Default | Description |
|--------|------|---------|-------------|
| IgnoreBombPlantedHUDMessages | bool | false | Ignores bomb planted HUD messages |
| IgnoreTeammateAttackMessages | bool | false | Ignores teammate attack messages |
| IgnoreAwardsMoneyMessages | bool | false | Ignores awards money messages |
| IgnorePlayerSavedYouMessages | bool | false | Ignores "player saved you" messages |
| IgnoreChickenKilledMessages | bool | false | Ignores chicken killed messages |
| IgnoreJoinTeamMessages | bool | false | Ignores join team messages |
| IgnorePlantingBombMessages | bool | false | Ignores planting bomb messages |
| IgnoreDefusingBombMessages | bool | false | Ignores defusing bomb messages |
| IgnoreDisconnectMessages | bool | false | Ignores disconnect messages |

### Auto Clean Dropped Weapons
| Option | Type | Default | Description |
|--------|------|---------|-------------|
| AutoClean_Enable | bool | false | Enables auto cleaning of dropped weapons |
| AutoClean_Timer | int | 10 | Timer in seconds (1-999) |
| AutoClean_MaxWeaponsOnGround | int | 5 | Max weapons on ground before cleaning |
| AutoClean_TheseDroppedWeaponsOnly | List<string> | [] | Specific weapons to clean (if empty, clean all) |

Weapons List Reference:
- A: AWP, G3SG1, SCAR-20, SSG 08
- B: AK-47, AUG, FAMAS, Galil, M4 variants
- C: M249, Negev
- D: Mag-7, Nova, Sawed-off, XM1014
- E: Bizon, MAC-10, MP5, MP7, MP9, P90, UMP-45
- F: All pistols
- G: All grenades
- H: Defuse kits
- I: Zeus
- J: Healthshot
- K: Knives
1. On plugin load, it checks for the existence of the `MapConfigs` folder.
2. If the folder does not exist, it creates it automatically.
3. After a map change or server start, it checks for a configuration file matching the current map name.
4. If a matching file is found, it executes that configuration file.
5. If no matching file is found, it checks for a common prefix configuration file (e.g., `de_.cfg` for all de_ maps).
6. If a common prefix file is found, it executes that file.
7. If neither file is found, no configuration is executed.

## Support and Feedback
Feel free to [open an issue](https://github.com/criskkky/sws2-gamemanager/issues/new/choose) for any bugs or feature requests. If it's all working fine, consider starring the repository to show your support!

## Contribution Guidelines
Contributions are welcome only if they align with the plugin's purpose. For major changes, please open an issue first to discuss what you would like to change.

---
<div align="center">
  <p>
  Made with ❤️ by <a href="https://github.com/criskkky" rel="noopener noreferrer" target="_blank">criskkky</a>
  </p>
</div>