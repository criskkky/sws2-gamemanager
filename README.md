<div align="center">

# [SwiftlyS2] GameManager

[![GitHub Release](https://img.shields.io/github/v/release/criskkky/sws2-gamemanager?color=FFFFFF&style=flat-square)](https://github.com/criskkky/sws2-gamemanager/releases/latest)
[![GitHub Issues](https://img.shields.io/github/issues/criskkky/sws2-gamemanager?color=FF0000&style=flat-square)](https://github.com/criskkky/sws2-gamemanager/issues)
[![GitHub Downloads](https://img.shields.io/github/downloads/criskkky/sws2-gamemanager/total?color=blue&style=flat-square)](https://github.com/criskkky/sws2-gamemanager/releases)
[![GitHub Stars](https://img.shields.io/github/stars/criskkky/sws2-gamemanager?style=social)](https://github.com/criskkky/sws2-gamemanager/stargazers)<br/>
  <sub>Made with ❤️ by <a href="https://github.com/criskkky" rel="noopener noreferrer" target="_blank">criskkky</a></sub>
  <br/>
</div>

## Overview

Set of useful features to manage various aspects of your CS2 server gameplay experience. <br/>Setup your game as you want.

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
- **Optimized Performance**: The plugin registers hooks dynamically and processes only the features you enable, reducing overhead and ensuring efficient resource usage. 
- **Blockers**: Restrict the use of specific in-game actions, as detailed in the [Blockers](#blockers) section. 
- **Hide Elements**: Control the visibility of various game components, as detailed in the [Hide](#hide) section. 
- **Disable Features**: Force-disable selected game functionalities, as detailed in the [Disable](#disable) section. 
- **Sound Control**: Mute or disable specific in-game sounds, as detailed in the [Sounds](#sounds) section.
- **Message Filtering**: Suppress default HUD messages, as detailed in the [Default MSGS](#default-msgs) section.

## Screenshots
> No screenshots available yet.

## Plugin Setup
> [!WARNING]
> Make sure you **have installed SwiftlyS2 Framework** before proceeding.

1. Download and extract the latest plugin version into your `swiftlys2/plugins` folder.
2. Perform an initial run in order to allow file generation.
3. Generated file will be located at: `swiftlys2/configs/plugins/GameManager/config.jsonc`
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

## Backend Logic (How It Works)
1. On plugin load, it checks for the existence of the configuration file. If it doesn't exist, a default one is created.
2. The plugin reads the configuration file and applies the settings accordingly.
3. Various hooks and event listeners are set up to monitor player actions and game events, applying the configured restrictions and modifications in real-time.
4. Depending on the settings the plugin will perform your desired actions.

## Support and Feedback
Feel free to [open an issue](https://github.com/criskkky/sws2-gamemanager/issues/new/choose) for any bugs or feature requests. If it's all working fine, consider starring the repository to show your support!

## Contribution Guidelines
Contributions are welcome only if they align with the plugin's purpose. For major changes, please open an issue first to discuss what you would like to change.
