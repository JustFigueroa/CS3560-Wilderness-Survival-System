# CS3560-Wilderness-Survival-System
Wilderness Survival System for CS3560


## Code structure

`Assets/Scripts` contains the ordinary C# game model in the `WildernessGameplay` namespace:

- `GameFactory` creates a `Game`, concrete `Difficulty`, `Map`, `Player`, and `Brain`.
- `Game` coordinates turns, messages, outcomes, and trade presentation.
- `Map` generates `Square` objects; each square has a concrete `Terrain` and `Item` objects.
- `Player` owns resources, movement/rest effects, collection, and trade decisions/transfers.
- `FoodBonus`, `WaterBonus`, and `GoldBonus` implement collection and depletion/respawning.
- `Patient`, `Impatient`, `Indecisive`, and `Generous` implement trader decisions.
- `EastFirst` and `ResourceAware` share a simple BFS through the current vision.
- `Vision` subclasses define visible shapes. `VisiblePaths` computes the nearest two targets and `Path` records steps, directions, and costs.
- A brain returns `Move`, `Rest`, or `NoAction`; `Game` applies it and owns game-over decisions.

`Assets/Scripts2` contains scene-facing Unity components (manager, menu, HUD, board, camera, and tile/player views). Existing serialized difficulty presets remain on `GameManager`; runtime difficulty objects copy them. Game-model classes are not attached as MonoBehaviours.

Run model checks with PowerShell:

```powershell
./Tests/RunChecks.ps1 -UnityEditorData '<Unity installation>/Editor/Data'
```

The structural refactor preserves the prior rules, random call order, and seed-based behavior. Later survival-priority adjustments let EastFirst seek supplies at 25% capacity and ResourceAware at 45%. Both seek the scarcer supply first and avoid supply detours that reduce the lowest food/water reserve percentage. Resource balance, the trade acceptance shortcut, and menu controls are unchanged by those adjustments.


## Guide for the original Scripts2 collaborator

The original rendering/menu flow is retained. The game model now owns the rules so there is only one copy of each resource value. Earlier requested changes (weighted terrain, new resource rates, terrain vision, and automatic collection) remain in effect.

| Original entry point or field | Where to look now |
| --- | --- |
| `GameManager.Instance`, `settings`, `StartGame` | Still in `Scripts2/GameManager.cs`; difficulty is a named enum with the original 1/2/3 values. |
| `GameSettings.seed`, `difficulty`, `boardWidth`, `boardHeight` | `Scripts/GameSettings.cs`; dimensions come from the selected preset. |
| `BoardManager.SpawnBoard`, `tilePrefabs`, `tileDataGrid`, `GetTileData` | Still in `Scripts2/BoardManager.cs`. Prefab order is Plains, Forest, Desert, Swamp, Mountain. |
| `SpawnPlayer` / `playerPrefab` | `BoardManager.PlacePlayer` enables the scene's assigned `player` instead of instantiating a second player. |
| `TileData.GenerateResources` | Generation moved to `Map` so the renderer never rerolls game state. `TileData.Initialize` binds that state. |
| `availableFoodAmount`, `availableWaterAmount`, `availableGoldAmount`, cost amounts and token arrays | Original names retained in `TileData`, reading the corresponding `Square` values. `ActivateTokens` still controls the tokens. |
| `PlayerTileVision` neighbor-reading methods | Retained; they use model coordinates and current vision rather than deriving coordinates from the offset sprite. |
| `BoardCamera` overview, player zoom, and follow settings | Retained in the same script. The zoom slider calls `SetZoom`. |
| Diagram `currentStrength`, `currentFood`, `currentWater`, `currentGold` | Those field names are used by the ordinary C# `Player` and read directly by the HUD. |

## Scene hierarchy and Unity boundary

- `MainMenu` contains its camera, Canvas, menu buttons/controller, EventSystem, and GameManager.
- `Game` contains its camera, BoardManager/BoardParent, inactive player, GameManager, EventSystem, and Canvas with all resource/vision labels, action messages, buttons, zoom slider, and inactive outcome popup.
- Procedural tile spawning is the only `Instantiate` call. Each tile prefab already contains its resource tokens, trader image, and finish stripe.
- Camera and player-vision references are assigned in the scene, rather than searched for at runtime. The player's sorting order is also saved in the scene.
- Only scene components derive from `MonoBehaviour`. The model contains no `UnityEngine` reference, custom editor code, or test components.
- `[Serializable]` remains on `DifficultyPreset` and `TerrainRule` because their values are edited and saved on GameManager. `[SerializeField]` remains on private scene references and menu/preset fields that need Inspector assignment. Runtime GameSettings and player resources are not serialized.
- Validation happens when selecting/creating a game. There are no validation Editor callbacks or Inspector-only minimum/range attributes in the model.

`Tests/RunChecks.ps1` is an optional plain C# verification tool outside Assets. It uses the installed Unity compiler/runtime as a local C# toolchain, but does not use Unity Test Runner, NUnit, scene test components, or mock Unity attributes. Existing Unity packages from the original project have not been removed.

## Current scene setup and balance

The resource HUD uses the `Player_HUD_1` sprite on `GameCanvas/Resources`, anchored at the top right. Its four existing Text children show black integers under the artwork's labels, reading `Player.currentStrength`, `currentFood`, `currentWater`, and `currentGold` through `GameHUD`.

Every terrain prefab uses `Tokens_1` for water, `Tokens_2` for food, and `Tokens_3` for gold, with neutral white renderer tint. This applies to available-resource and movement-cost tokens. Movement currently charges strength, food, and water; there is no gold movement cost.

| Preset setting | Easy | Medium | Hard |
| --- | --- | --- | --- |
| Board dimensions | 10 x 10 | 30 x 30 | 50 x 50 |
| Starting strength | 10 | 10 | 10 |
| Starting food / water | 20 / 20 | 18 / 18 | 15 / 15 |
| Starting gold | 10 | 8 | 8 |
| Food / water spawn chance | 50% each | 50% each | 50% each |
| Food / water amount | 2-4 | 2-4 | 2-4 |
| Gold spawn chance / amount | 30% / 3-6 | 30% / 3-6 | 30% / 3-6 |
| Trader spawn chance | 20% | 20% | 20% |
| Strength recovered per rest | 5 | 5 | 5 |

Resource respawning is off. Resting and completed trades cost one food and one water; negotiating does not charge upkeep. Maximum strength is 20, and maximum food and water are 30 each.

`DifficultyPreset.Create` supplies defaults, but the saved `GameManager` presets in both `MainMenu` and `Game` scenes must also be updated when changing balance. The latter supports opening the Game scene directly. The turn counter is not displayed; internal turn tracking remains for turn limits and resource timers. No-action remains a defeat. Both brains plan only through currently visible squares; the lower-level movement check validates adjacency and resources without checking visibility.

Traders have no stock limits or inventory balances. Patient, Impatient, and Indecisive traders independently roll 3-6 of the more-needed supply (food or water) each encounter and initially charge two gold per resource. Generous traders still offer six for one gold. Exchanges check the player's affordability and capacity; trader inventory is not tracked.
