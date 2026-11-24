# SHELTER

[한국어](README.ko.md)

VR zombie survival game for Meta Quest. Hold a single shelter against 10 rounds of zombies.

- Engine: Unity 6000.5.2f1 (URP)
- VR: OpenXR, XR Interaction Toolkit 3
- Platform: Meta Quest (Android)

## Gameplay

- Survive 10 rounds inside the shelter. Clearing round 10 wins; dying ends the game.
- Zombies spawn outside, tear the planks off windows and doors, and break in.
- Each kill earns money. Money is spent on:
  - Weapons from the vending shop: Pistol, SMG, AR
  - Reloads (each magazine costs money)
  - Barricade repairs during the intermission
  - Healing at the first-aid station
- The Ready button on the shop skips the intermission.
- Difficulty: Normal or Hard. Hard raises zombie health and spawn count and lowers kill rewards.
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
