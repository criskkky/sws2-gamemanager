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
| Option | Type | Example | Description |
|--------|------|---------|-------------|
| BlockRadio | bool | true | Blocks radio commands |
| BlockBotRadio | bool | true | Blocks bot radio |
| BlockGrenadesRadio | bool | true | Blocks grenade radio messages |
| BlockChatWheel | bool | true | Blocks chat wheel commands |
| BlockPing | bool | true | Blocks ping command |
| BlockedCommands | List<string> | ["some", "commands"] | List of custom commands to block (case sensitive) |

### Hide
| Option | Type | Example | Description |
|--------|------|---------|-------------|
| HideRadar | bool | true | Hides the radar |
| HideKillFeed | byte | 1 | 0 = No, 1 = Full, 2 = Show only own kills |
| HideBlood | bool | true | Hides blood effects |
| HideHeadshotSparks | bool | true | Hides headshot sparks |
| HideTeammateHeadtags | byte | 2 | 0 = No, 1 = Yes, 2 = Just behind walls, 3 = Disable by distance |
| HideTeammateHeadtags_Distance | byte | 100 | Distance for headtags (50, 100, 150, 250) |
| HideCorpses | byte | 2 | 0 = No, 1 = Instantly, 2 = Fade out |
| HideLegs | bool | true | Hides player legs |

### Disable
| Option | Type | Example | Description |
|--------|------|---------|-------------|
| DisableFallDamage | bool | true | Disables fall damage |
| DisableSVCheats | bool | true | Disables sv_cheats |
| DisableC4 | bool | true | Disables C4 in-game |
| DisableCameraSpectator | bool | true | Disables spectator camera transitions |

### Sounds
| Option | Type | Example | Description |
|--------|------|---------|-------------|
| SoundsMuteFootsteps | bool | true | Mutes footsteps |
| SoundsMuteJumpLand | bool | true | Mutes jump landing sounds |

### Default MSGS
| Option | Type | Example | Description |
|--------|------|---------|-------------|
| IgnoreBombPlantedHUDMessages | bool | true | Ignores bomb planted HUD messages |
| IgnoreTeammateAttackMessages | bool | true | Ignores teammate attack messages |
| IgnoreAwardsMoneyMessages | bool | true | Ignores awards money messages |
| IgnorePlayerSavedYouMessages | bool | true | Ignores "player saved you" messages |
| IgnoreChickenKilledMessages | bool | true | Ignores chicken killed messages |
| IgnoreJoinTeamMessages | bool | true | Ignores join team messages |
| IgnorePlantingBombMessages | bool | true | Ignores planting bomb messages |
| IgnoreDefusingBombMessages | bool | true | Ignores defusing bomb messages |
| IgnoreDisconnectMessages | bool | true | Ignores disconnect messages |

## Backend Logic (How It Works)
1. On plugin load, it checks for the existence of the configuration file. If it doesn't exist, a default one is created.
2. The plugin reads the configuration file and applies the settings accordingly.
3. Various hooks and event listeners are set up to monitor player actions and game events, applying the configured restrictions and modifications in real-time.
4. Depending on the settings the plugin will perform your desired actions.

## Support and Feedback
Feel free to [open an issue](https://github.com/criskkky/sws2-gamemanager/issues/new/choose) for any bugs or feature requests. If it's all working fine, consider starring the repository to show your support!

## Contribution Guidelines
Contributions are welcome only if they align with the plugin's purpose. For major changes, please open an issue first to discuss what you would like to change.
