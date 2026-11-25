# SHELTER

[한국어](README.ko.md)

VR zombie survival game for Meta Quest. Hold a single shelter against 10 rounds of zombies.

- Engine: Unity 6000.5.2f1 (URP)
- VR: OpenXR, XR Interaction Toolkit 3
- Platform: Meta Quest (Android)

## About

- Team project by 3 developers, July–November 2026.
- Development has ended. The repository is kept as an archive.
- The current build is single-player. 1–4 player co-op with Photon Fusion 2 was planned but not implemented.

## Gameplay

- Survive 10 rounds inside the shelter. Clearing round 10 wins; dying ends the game.
- Zombies spawn outside, tear the planks off windows and doors, and break in.
- Each kill earns money. Money is spent on:
  - Weapons from the vending shop: Pistol, SMG, AR
  - Reloads (each magazine costs money)
  - Barricade repairs during the intermission
  - Healing at the first-aid station
- The Ready button on the shop skips the intermission.
- Difficulty: Normal or Hard, chosen after Game Start on the title screen.
- Clearing round 10 or dying opens a panel with Retry and Title.
- The tutorial covers movement, buying, shooting, reloading, holstering, and repairs.

## Controls

| Input | Action |
|---|---|
| Left stick | Move |
| Right stick | Snap turn |
| Trigger | Grab, select, buy, fire |
| A / X | Reload |
| B / Y | Switch single / automatic fire |
| Left menu button | Pause menu |
| Release a weapon at the hip | Holster |

Health and money are shown on the back of the hand.

## Systems

| System | Implementation |
|---|---|
| Rounds | `RoundManager` state machine: intermission, in progress, cleared, victory, defeat. Other systems subscribe to its events. |
| Waves | One `WaveDefinition` asset per round sets zombie types and counts. `WaveManager` spreads spawns across tagged spawn points and caps how many zombies are alive at once. |
| Zombies | `NavMeshAgent` movement. Outside, a zombie attacks its assigned (or nearest) barricade; once inside, it chases the nearest player. Zombies vault through windows, ragdoll on death, and are pooled. |
| Barricades | Each window and door has planks. A zombie removes one plank per hit. Players repair planks for money at a repair prompt. |
| Economy | Wallet, kill rewards, vending shop, paid reloads, heal station. |
| Combat | Raycast guns with single and automatic fire, magazines, haptics, and hip holsters. |
| Player | Health and damage feedback. The avatar body follows the headset, and arm IK follows the controllers. |
| Difficulty | `DifficultyConfig` multiplies zombie health, spawn count, max alive, and kill reward. |
| UI / Audio | World-space menus placed in front of the player, wrist display, round music, spatial zombie sounds. |

Balance values are ScriptableObjects in `Assets/_Project/ScriptableObjects`.

## Balance

Zombies

| Type | Health | Speed (m/s) | Reward | Appears from |
|---|---|---|---|---|
| Walker | 100 | 1.5 | 80 | Round 1 |
| Runner | 80 | 3.2 | 100 | Round 4 |

Weapons

| Weapon | Price | Damage | Ammo |
|---|---|---|---|
| Pistol | 300 | 37 | 15 |
| SMG | 900 | 22 | 40 |
| AR | 1800 | 28 | 60 |

- Starting money: 800
- Healing: 400, restores full health
- Zombies per round (Normal): 6, 8, 10, 12, 15, 18, 21, 24, 28, 32
- Intermission between rounds: 60 seconds

Hard mode multipliers

| Zombie health | Spawn count | Max alive | Kill reward |
|---|---|---|---|
| ×1.75 | ×1.5 | ×1.2 | ×0.6 |

## Project Setup

1. Install Unity 6000.5.2f1 with Android Build Support.
2. Clone this repository and open it in Unity.
3. Import the third-party assets listed below into the listed folders.
4. Open `Assets/Scenes/Main.unity` (title) or `Assets/Scenes/TestScene.unity` (game).

The code compiles without the third-party assets. Without them, models, animations, and sounds are missing.

## Third-Party Assets (not included)

These assets are excluded from the repository because their licenses do not allow redistribution.

| Folder | Content |
|---|---|
| `Assets/Plugins/RootMotion` | Final IK (RootMotion) — player body IK |
| `Assets/Survivalist` | Player character model (`StarterAssets` is included) |
| `Assets/ZombieMale_AAB` | Zombie character model |
| `Assets/Kevin Iglesias` | Human Basic Motions FREE, Human Melee Animations FREE |
| `Assets/Infima Games` | Low Poly Shooter Pack - Free Sample |
| `Assets/Weapon/Low Poly Pistol Weapon Pack 3` | Weapon models |
| `Assets/Weapon/Low Poly SMG Weapon Pack 2` | Weapon models |
| `Assets/Weapon/LowPoly_Tactical_Dagger` | Dagger model |
| `Assets/PBR_Materials_AL8310` | PBR materials |
| `Assets/YughuesFreeBushes2018` | Bushes |
| `Assets/Map`, `Assets/Plank` | Shelter map and plank models |
| `Assets/Resources/Animations` | Zombie animations (Mixamo) |
| `Assets/Resources/SFX`, `Assets/Sounds` | Sound effects and music |

## Project Structure

| Path | Content |
|---|---|
| `Assets/_Project/Scripts` | Game code (`Shelter.*`): rounds, waves, enemies, barricades, economy, player, difficulty, UI, audio |
| `Assets/_Project/ScriptableObjects` | Balance data: rounds, waves, enemies, economy, difficulty |
| `Assets/_Project/Prefabs` | Weapons, UI, player |
| `Assets/Scenes` | `Main` (title), `TestScene` (game) |
