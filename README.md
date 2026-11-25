# SHELTER

[한국어](README.ko.md)

SHELTER is a VR zombie survival game for Meta Quest. You're trapped in a small shelter, and your goal is to hold out against 10 rounds of zombies trying to break in.

- Engine: Unity 6000.5.2f1 (URP)
- VR: OpenXR, XR Interaction Toolkit 3
- Platform: Meta Quest (Android)

## About

SHELTER was built by a team of three between July and November 2026. Development has wrapped up, and this repository is kept as an archive of the project.

The game is currently single-player. We originally planned 1–4 player co-op using Photon Fusion 2, but it wasn't implemented.

## How to Play

Survive 10 rounds inside the shelter. Clear round 10 and you win; die, and the game is over.

Zombies show up outside, tear the planks off the windows and doors, and climb in. Every zombie you kill earns money, which you can spend on:

- Weapons from the vending machine shop: Pistol, SMG, and AR
- Reloads (each magazine costs money)
- Repairing barricades between rounds
- Healing at the first-aid station

If you're ready before the break is over, press the Ready button on the shop to start the next round right away.

After pressing Game Start on the title screen, you can choose Normal or Hard. Whether you clear the game or die, a panel appears that lets you retry or return to the title.

A tutorial walks you through moving, buying, shooting, reloading, holstering, and repairing.

## Controls

| Input | Action |
|---|---|
| Left stick | Move |
| Right stick | Snap turn |
| Trigger | Grab, select, buy, fire |
| A / X | Reload |
| B / Y | Switch between single and automatic fire |
| Left menu button | Pause menu |
| Let go of a weapon near your hip | Holster it |

Your health and money are shown on the back of your hand.

## How It Works

| System | Description |
|---|---|
| Rounds | `RoundManager` runs the game through its states: break, in progress, cleared, victory, and defeat. Other systems listen for its events. |
| Waves | Each round has its own `WaveDefinition` asset that sets which zombies appear and how many. `WaveManager` spreads them across the spawn points and limits how many can be alive at once. |
| Zombies | Zombies move with `NavMeshAgent`. Outside, each one goes after its assigned barricade (or the nearest one). Once inside, it chases the nearest player. They climb through windows, ragdoll when killed, and are reused through an object pool. |
| Barricades | Every window and door is boarded up with planks. Each zombie hit knocks one plank off, and players can pay to put them back. |
| Economy | Wallet, kill rewards, the vending machine shop, paid reloads, and the heal station. |
| Combat | Raycast guns with single and automatic fire, magazines, controller vibration, and hip holsters. |
| Player | Health and hit feedback. The avatar's body follows the headset, and the arms follow the controllers with IK. |
| Difficulty | `DifficultyConfig` adjusts zombie health, spawn count, max zombies alive, and kill rewards. |
| UI / Audio | Menus that appear in front of the player, a wrist display, round music, and 3D zombie sounds. |

All balance values live in ScriptableObjects under `Assets/_Project/ScriptableObjects`, so they can be tuned without touching code.

## Balance

Zombies

| Type | Health | Speed (m/s) | Reward | First appears |
|---|---|---|---|---|
| Walker | 100 | 1.5 | 80 | Round 1 |
| Runner | 80 | 3.2 | 100 | Round 4 |

Weapons

| Weapon | Price | Damage | Ammo |
|---|---|---|---|
| Pistol | 300 | 37 | 15 |
| SMG | 900 | 22 | 40 |
| AR | 1800 | 28 | 60 |

- You start with 800.
- Healing costs 400 and restores you to full health.
- Zombies per round on Normal: 6, 8, 10, 12, 15, 18, 21, 24, 28, 32
- Each break between rounds lasts 60 seconds.

Hard mode makes zombies tougher and more numerous, and pays less per kill:

| Zombie health | Spawn count | Max alive | Kill reward |
|---|---|---|---|
| ×1.75 | ×1.5 | ×1.2 | ×0.6 |

## Getting Started

1. Install Unity 6000.5.2f1 with Android Build Support.
2. Clone this repository and open it in Unity.
3. Import the third-party assets listed below into their folders.
4. Open `Assets/Scenes/Main.unity` for the title screen, or `Assets/Scenes/TestScene.unity` to jump straight into the game.

The code compiles without the third-party assets, but models, animations, and sounds will be missing.

## Third-Party Assets (not included)

These assets aren't included because their licenses don't allow redistribution.

| Folder | Content |
|---|---|
| `Assets/Plugins/RootMotion` | Final IK (RootMotion), used for the player's body IK |
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
