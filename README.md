# IAV26 — Individual Final Project: Behavior Trees and Tactical Waypoints

**Author**
- Leopold Popper ([Kang-Hsi](https://github.com/Kang-Hsi))

# Summary

This is the individual final project for *Inteligencia Artificial para Videojuegos* at UCM. It extends the group practice **P3 — Disturbios Orbitales**, a 3D FPS prison scenario in which an intelligent agent (bot) competes against a human player in an infinite-respawn skirmish. P3 used a **Hierarchical Finite State Machine (HFSM)** as the bot's decision system, Unity NavMesh navigation driven by a singleton waypoint manager, and a kills / deaths / score HUD.

For this individual extension, the HFSM is **replaced** by a **Behavior Tree (BT)**, kept fully data-driven via ScriptableObjects in the same spirit as P3's HFSM. On top of that, the project's plain navigation waypoints are enriched into a **Tactical Waypoint** layer with a *cover-quality* score per point. A new BT leaf queries this layer when the bot is wounded so it retreats to good cover instead of standing in the open, with the goal of measurably improving the *Kills − Deaths* metric.

# Installation and usage

All project content is available in this repository. Unity 6 **(6000.3.12f1)** is used. The executable of the project is located in the repository so that the behaviors can be experienced.

# Introduction

The virtual environment is a 3D space prison populated by:

- **Prisoners** — mortal enemies of each other. Each can be controlled by a human player or a bot. They spawn at predefined respawn points and can move, shoot, aim, change weapons, jump, crouch, run, and fly with a jetpack.
- **Weapons** — collectible only by prisoners: pistol, missile launcher, sniper rifle, flamethrower, rifle, dual pistols, shotgun.
- **Utensils** — health kits and jetpack pickups.
- **Robotic guards** — patrol the prison and fire at any prisoner. Two kinds: stationary **turrets** (powerful but immobile) and **hover bots** (weaker but mobile).

The game is endless: a prisoner respawns each time it is eliminated. The bot's goal is to maximise *Kills − Deaths* over a session.

# Starting point

This repository was duplicated from the group practice [IAV26-G07-P3](https://github.com/IAV26-G07/IAV26-G07-P3) with its full commit history preserved for attribution. Individual work for this project begins at commit `Individual Project Start`. The group practice itself derives from the teacher's [FPS-Online](https://github.com/narratech/fps-online) starter, available on the Narratech [*Disturbios Orbitales*](https://narratech.com/es/inteligencia-artificial-para-videojuegos/decision/disturbios-orbitales/) page.

Architecturally, the human player and the bot share the same body (`CharacterController`, animations, weapons) and differ only in who decides their actions — input devices for the human, **Behavior Tree** for the bot:

```mermaid
---
config:
  theme: dark
  look: neo
---
flowchart LR
  Input[Keyboard / Mouse] --> PIH[PlayerInputHandler]
  PIH --> PCC[PlayerCharacterController]
  PCC --> CC[CharacterController + Animator]
  CC --> Body1[Movement / fire of<br>Human_Prefab]

  BT[Behavior Tree<br>BTLoader] --> BGA[BotGameplayActions<br>action façade]
  BGA --> NMA[NavMeshAgent + PlayerWeaponsManager]
  NMA --> Body2[Movement / fire of<br>UCM_Bot]

  TW[Tactical Waypoints<br>cover values] -.queried by.-> BT
```

# Project structure

The project root keeps the standard Unity layout. Most source code lives under `Assets/FPS/Scripts/`:

- `AI/` — bot perception, gameplay actions, weapons handling shared by player and bot.
- `Editor/` — editor-only utilities, including the tactical-waypoint baker.
- `Game/`, `Gameplay/` — gameplay and match scripts.
- `MiMultiplayer/` — multiplayer scaffolding and the HUD (`ClientScoreboardHUD`).
- `UI/` — UI screens.
- `PracticaIAV/` — the HFSM ScriptableObjects from P3 (kept for reference, unwired).
- `BT/` *(new)* — Behavior Tree runtime + ScriptableObject node definitions.
- `Tactical/` *(new)* — `TacticalWaypoint` type and the cover-quality baker.

# Problem statement

The project's scenes are: intro menu, win screen, lose screen, main scene, secondary scene, and the prison scene. The flow is simple — the player enters the prison scene from the menu and exits when they choose to:

```mermaid
stateDiagram-v2
  [*] --> IntroMenu : Game launch
  IntroMenu --> PrisonScene : Press Play button
  PrisonScene --> [*] : Press Exit button
```

The prototype delivers the following five characteristics.

# Characteristics

- **A — Adapted starting point.** P3's prison scenario reused as the environment: NavMesh, waypoints, respawn points, first-person and minimap cameras.

- **B — Player control.** The human player can move, jump, crouch, run, jetpack, aim, shoot, and switch weapons.

- **C — NavMesh navigation.** The bot uses Unity's NavMesh and the `WaypointsManager` to explore rooms, collect weapons and health packs, and pursue other prisoners.

- **D — Behavior Tree.** The bot's decision system is a Behavior Tree of `Task` nodes returning `run() -> bool`, composed with `Selector`, `Sequence`, and decorators, all data-driven via ScriptableObjects and instantiated by a `BTLoader`. It replaces P3's HFSM (kept in the codebase as historical context but unwired).

- **E — Tactical Waypoints (cover-point).** Plain navigation waypoints are enriched into a `TacticalWaypoint` carrying a `coverValue ∈ [0, 1]`, baked at editor time by the `getCoverQuality` ray-cast algorithm. A new BT leaf `GoToBestCover` filters waypoints by a runtime line-of-sight check against the current enemy and picks the one that maximises `coverValue × distFromEnemy / distToBot`.

# Design

## Design decisions

- **BT over HFSM.** A BT encodes priority by node ordering inside the root `Selector` rather than by separately-defined transitions and levels. Every node is a `Task` returning `true` / `false`, which keeps the implementation small and the tree easy to extend.
- **Data-driven nodes.** Every BT node is a ScriptableObject, edited from the Unity inspector and instantiated at runtime by `BTLoader`. Designers can rewire the tree without recompiling.
- **Façade preserved.** The existing `BotGameplayActions` (`TryFireCurrentWeaponPrimary`, `TryPickHealth`, …) remains the only contract the BT calls into. The body of the bot is untouched.
- **Editor-time baking.** Cover values are computed once in editor (via menu item) using ray-casts, then serialised onto the asset. Runtime cost is zero: the BT only does a sort.
- **Tick from root.** Because `Task.run()` returns `bool` (no `RUNNING` state), the bot ticks the whole tree from the root every frame. This gives reactive behavior — the bot drops Combat for Survive the moment its life crosses the threshold.

## BT serialization

The BT graph is stored as `ScriptableObject` assets under `Assets/FPS/Scripts/BT/SO/`:

- **`TaskSO`** — abstract base. Holds editor metadata and a `Create()` factory that returns the runtime `Task` instance.
- **`CompositeTaskSO`** (extends `TaskSO`) — holds an ordered list of `children`. Subclassed by `SelectorSO` and `SequenceSO`.
- **`DecoratorTaskSO`** (extends `TaskSO`) — wraps a single `child`. Subclassed by `InverterSO` and `UntilFailSO`.
- **`ActionTaskSO` / `ConditionTaskSO`** — leaf wrappers that adapt an existing HFSM `ActionSO` / `ConditionSO` into a BT `Task`, so all gameplay primitives from P3 (`ShootActionSO`, `FaceActionSO`, `EnemyNearConditionSO`, …) are reused without modification.

`BTLoader.GetTask(TaskSO)` maps each SO to its runtime instance and caches it in a dictionary, so the same SO referenced from two places shares the same runtime node.

# Game controls

| Input | Function |
| -- | -- |
| Mouse | Camera rotation |
| Left mouse button | Fire the currently equipped weapon |
| Right mouse button | Zoom / aim down sights |
| Mouse wheel / number keys / E / Q | Switch weapons |
| Tab / P | Toggle options / pause menu |
| WASD / arrow keys | Move |
| C | Crouch |
| Space | Jump / jetpack |
| Shift | Sprint |

# Implementation

## Behavior Tree

### Tree structure

The bot's decision tree mirrors P3's HFSM priorities (Survive > Combat > Explore), expressed as a priority-ordered `Selector` at the root. Following the standard notation, `?` denotes a Selector and `→` denotes a Sequence:

```mermaid
---
config:
  theme: dark
  look: neo
---
graph TD
  Root["Root [?]"] --> SurviveBranch["Survive [→]"]
  Root --> CombatBranch["Combat [→]"]
  Root --> ExploreBranch["Explore [?]"]

  SurviveBranch --> SCond["LifeHysteresis?<br>engage <100, disengage >160"]
  SurviveBranch --> SSel["[?]"]
  SSel --> Defend["Defend [→]<br>EnemyVeryNear? → SelectWeapon → SelectTarget → Face → Shoot"]
  SSel --> Retreat["Retreat [→]<br>EnemyNear? → SelectTarget → GoToBestCover"]
  SSel --> Wait["Wait (AlwaysTrue)<br>no-op fallback while regen heals"]

  CombatBranch --> CCond["EnemyNear?"]
  CombatBranch --> CST["SelectTarget"]
  CombatBranch --> ShootSeq["Shoot [→]<br>SelectWeapon → Face → Shoot"]

  ExploreBranch --> FW["FindWeapon [→]<br>NoHasAllWeapons? → PickWeapon"]
  ExploreBranch --> WA["Wander"]
```

The bot ticks the root every frame. A higher-priority subtree (Survive) preempts a lower one (Combat or Explore) as soon as its guard task returns `true`. The `Wait` fallback inside Survive keeps the bot committed to recovery once wounded — `Defend` and `Retreat` both require an enemy in line of sight, so without `Wait` the bot would flicker into `Explore` whenever cover broke LOS. Passive regen on `BotControllerBT` heals while Survive is engaged, so the bot returns to combat once `LifeHysteresis` disengages at >160 HP.

### Pseudocode

**1. Base task**

```py
class Task:
    # Return success (true) or failure (false).
    function run() -> bool
```

Every node in the behavior tree is a `Task`. Running the task returns either success or failure.

**2. Example leaf — condition**

```py
class EnemyNear extends Task:
    function run() -> bool:
        # Task fails if there is no enemy nearby.
        return distanceToEnemy < 10
```

**3. Example leaf — action**

```py
class PlayAnimation extends Task:
    animationId: int
    speed: float = 1.0
    function run() -> bool:
        if animationEngine.ready:
            animationEngine.play(animationId, speed)
            return true
        else:
            # Task fails: the animation could not be played.
            return false
```

**4. Selector**

```py
class Selector extends Task:
    children: Task[]
    function run() -> bool:
        for c in children:
            if c.run():
                return true
        return false
```

A selector tries each child in order. It succeeds as soon as one child succeeds.

**5. Sequence**

```py
class Sequence extends Task:
    children: Task[]
    function run() -> bool:
        for c in children:
            if not c.run():
                return false
        return true
```

A sequence tries each child in order. It fails as soon as one child fails. It only succeeds if all children succeed.

**6. Decorators**

Decorators wrap a single child task and modify how its result propagates. Common decorators include:

- limit the number of executions of a task,
- continue until a task returns failure,
- invert the returned value,
- act as a guard around a resource.

**7. Loader (singleton, mirrors `HFSMLoader`)**

```py
class BTLoader: (singleton)
    tasks: Dict[TaskSO, Task]

    function getTask(so) -> Task:
        if so not in tasks:
            tasks[so] = so.Create(this)
        return tasks[so]
```

## Tactical Waypoints

The plain `WaypointsManager` waypoints carry only a `Vector3` position. The new `TacticalWaypoint` extends this with a `coverValue ∈ [0, 1]` field, computed once at editor time by the cover-quality algorithm and stored on the asset.

### `getCoverQuality` pseudocode

From `iterations` candidate "enemy" positions around the location, rays are projected toward the location and the ratio of obstructed rays is returned.

```py
function getCoverQuality(location: Vector,
                         iterations: int,
                         characterSize: float) -> float:
    # Set the initial angle.
    theta = 0
    # Start with no hits.
    hits = 0
    # No valid rays yet.
    valid = 0
    for i in 0...iterations:
        # Create a point near the location.
        from = location
        from.x += RADIUS * cos(theta) + randomBinomial() * RAND_RADIUS
        from.y += random() * 2 * RAND_RADIUS
        from.z += RADIUS * sin(theta) + randomBinomial() * RAND_RADIUS
        # Check for a valid source point.
        # Example: do not allow the two points to be in the same room.
        if not sameRoom(from, location):
            valid += 1
            # Create the target point near the location.
            to = location
            to.x += randomBinomial() * characterSize.x
            to.y += random() * characterSize.y
            to.z += randomBinomial() * characterSize.z
            # Project a ray.
            if doesRayCollide(from, to):
                hits += 1
        # Update the angle.
        theta += ANGLE
    return float(hits) / float(valid)
```

A higher result means the position gives better cover.

### BT integration

A new BT leaf `GoToBestCover` is added to the Survive subtree (inside `Retreat`). Before scoring, every waypoint is filtered by a runtime line-of-sight raycast against the current enemy — only waypoints the enemy *cannot* see are eligible. The score then rewards waypoints far from the enemy and close to the bot:

```py
class GoToBestCover extends Task:
    function run() -> bool:
        target = currentEnemy()
        if target == null: return false
        best = null
        bestScore = -infinity
        for wp in TacticalWaypointsManager.all():
            # Skip waypoints the enemy can already see (not real cover right now).
            if not rayBlocked(wp.position, target.position):
                continue
            distToBot = distance(bot, wp)
            distFromEnemy = distance(target, wp)
            score = wp.coverValue * distFromEnemy / distToBot
            if score > bestScore:
                bestScore = score
                best = wp
        if best == null: return false
        bot.NavigateTo(best.position)
        return true
```

## Runtime features

A few additions that live outside the tree itself but make the BT behave well in the prison scene:

- **`LifeHysteresis` condition.** A stateful condition gating the Survive subtree. Returns `true` once the bot drops below 100 HP, and keeps returning `true` until the bot climbs back above 160 HP. Prevents the bot from flipping out of Survive on the very first regenerated HP and lets the full retreat-and-recover arc play out.
- **`Wait` fallback.** The innermost Selector of Survive ends with an `AlwaysTrue` leaf. When the bot is in cover and `Defend` / `Retreat` both fail, `Wait` keeps the bot committed to Survive instead of flickering into Explore.
- **Passive regen on `BotControllerBT`.** Outside the tree, `BotControllerBT.Update` heals the bot at **5 HP/sec** by default. While the BT's last action was `Shoot`, the rate drops to **2 HP/sec**, so trading fire is a poor place to heal and retreating is rewarded.
- **Debug overlay (`BTDebug`).** A static one-frame buffer that `ActionTask` / `ConditionTask` write to on each tick. `BotControllerBT.OnGUI` draws the current BT action and last condition as a top-left text overlay, which is how you can *see* the tree's decisions while playing.
- **Editor builder.** `Assets/FPS/Scripts/BT/Editor/BuildBotBT.cs` exposes a menu item `BT → Build Bot Tree` that wipes `BT/SO/Tree/` and regenerates the full tree of SO assets from code. The leaves wrap the existing HFSM `ActionSO` / `ConditionSO` primitives, so all gameplay logic is reused without modification.
- **Cover-quality baker.** `Assets/FPS/Scripts/Tactical/Editor/CoverQualityBaker.cs` exposes `BT → Bake Tactical Waypoints`, which runs `getCoverQuality` on every `TacticalWaypoint` in the scene and writes the result back to the asset. Spheres in the scene view colour-shift red→green by baked cover value.

# Tests and metrics

Test plan, organised by characteristic:

- **Section A**
    1. Verify weapons, utensils, and robotic guards spawn at their designated waypoints.
    2. Verify the first-person camera follows the protagonist.
    3. Verify the minimap camera shows a top-down overview of the scene.
    4. Verify the minimap updates in real time as the agent moves.
    ---
- **Section B**
    1. Choose `Human_Prefab`. Verify it spawns at a predefined respawn point.
    2. Verify the character moves correctly (WASD).
    3. Verify the character can shoot and switch weapons.
    4. Verify the character can sprint, crouch, and jump.
    ---
- **Section C**
    1. Choose `UCM_Bot`. Verify it explores the prison autonomously.
    2. Verify it avoids walls and obstacles correctly.
    3. Verify it does not get stuck in a single location.
    4. Verify it walks to weapons and picks them up.
    5. Verify it detects and attacks other prisoners.
    6. Verify Kills - Death HUD updates correctly.
    ---
- **Section D**
    1. Verify the Behavior Tree loads from ScriptableObjects without errors.
    2. Verify the bot switches behavior correctly: Explore → Combat on enemy near, Combat → Survive on low life, Survive → Explore once healed.
    3. Verify the on-screen GUI overlay correctly tracks and displays the current BT action during play.
    ---
- **Section E**
    1. When wounded mid-range, verify the bot moves to the best `TacticalWaypoint` (high cover value, far from the enemy).
    2. Verify the bot's passive regen restores its health while it waits at the cover waypoint.
    3. Once healed, verify the bot leaves cover and resumes Explore (PickWeapon or Wander).

# Evolution

| Status | Task | Date |
| -- | -- | -- |
| ✔ | Individual project setup | 2026-05-18 |
| ✔ | Behavior Tree system (runtime, ScriptableObjects, loader, editor builder) | 2026-05-25 |
| ✔ | UCM_Bot wired to the BT; Explore / Combat / Survive rebuilt | 2026-05-25 |
| ✔ | Tactical waypoints + `GoToBestCover` + passive regen | 2026-05-25 |
| ✔ | Hysteresis on Survive and on-screen debug overlay | 2026-05-25 |
| ✔ | Final documentation and demo video | 2026-05-26 |

# Conclusions

In this project a functional prototype has been developed that integrates fundamental concepts of Artificial Intelligence applied to video games within an interactive environment in Unity. Through the orbital prison scenario, classic decision techniques have been combined: **Behavior Trees**, navigation using Unity's **NavMesh** with waypoints, and a **Tactical Waypoint** layer with cover scoring.

One of the most relevant aspects has been the modularity of the Behavior Tree, which allowed designing data-oriented independent tasks using Unity's `ScriptableObject`, facilitating their reuse and extension. This is good practice because it is closely related to common practices in the industry.

The on-screen debug overlay has allowed the observation of which BT action the bot is performing at any moment, helping verify the priority order between the Survive, Combat, and Explore branches.

During development, typical problems were addressed: the difference between actions and guard conditions; the flicker between Survive and Explore when line of sight to the enemy broke (solved by the `Wait` fallback and hysteresis on the life threshold); and the selection of unsafe cover points (solved with a runtime LOS check and a far-from-enemy scoring term).

Overall, the project meets the proposed objectives and serves as a solid base for future extensions, such as more sophisticated tactical scoring that accounts for path exposure.

# Demo video

[Demo on YouTube](https://youtu.be/BgLdopWeWxg)

# License

- Educational and research use — UCM
- Copyright (c) 2026 Leopold Popper
- Permanent permission is granted to the professors of the Faculty of Informatics of the Complutense University of Madrid to use, reproduce, modify, or distribute, in whole or in part, this material (documentation, code, and resources), for educational or research purposes. Aggregated anonymous data may be extracted, and the material may be used with explicit attribution to the author.
- This license does not grant commercial use rights outside of the educational or research scope, except with explicit authorisation from the author.

# References

### Bibliography
- Millington, I. (2019). *AI for Games* (3rd ed.). CRC Press.
- Peinado, F. *Árbol de comportamiento* — lecture slides, UCM IAV26.
- Peinado, F. *Puntos de ruta tácticos* — lecture slides, UCM IAV26.
- Peinado, F. *Análisis táctico* — lecture slides, UCM IAV26.

### Starter resources
- Narratech: [*Disturbios Orbitales* — exercise statement](https://narratech.com/es/inteligencia-artificial-para-videojuegos/decision/disturbios-orbitales/)
- Narratech: [*FPS-Online* — starter project](https://github.com/narratech/fps-online)

### Reference video games
- Bungie (2001). *Halo: Combat Evolved* — Behavior Tree-based enemy AI.
- Crytek (2007). *Crysis* — tactical cover use in open-combat AI.
