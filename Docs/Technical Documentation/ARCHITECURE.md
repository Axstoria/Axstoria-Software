# Architecture Reference

**Project:** RPG Engine  
**Version:** 1.0  
**Last updated:** 2026

---

## Table of Contents

1. [Overview](#overview)
2. [Core Principles](#core-principles)
3. [Layer Reference](#layer-reference)
4. [Feature Reference](#feature-reference)
5. [Decision Guide](#decision-guide)
6. [Patterns](#patterns)
7. [Inter-Feature Communication](#inter-feature-communication)

---

## Overview

This project is a two-part RPG engine: an **Editor** for building campaigns (map, entities, events, character sheets) and a **Game Session** for playing them with a game master and players.

### Feature Map

```
Assets/Scripts/
│
├── Shared/              Types used everywhere — no business logic
├── Grid/                grid logic — coordinates, distance, line of sight
│
├── ── EDITOR ──────────────────────────────────────────────────────
│
├── MapEditor/           Terrain generation — parameters, relief, mesh
├── AssetImporter/       Import 3D files (GLTF) into the project
├── AssetLibrary/        Catalogue of available assets — names, paths, thumbnails
├── EntityEditor/        Entity templates — NPC, object, structure (stats, events, AI)
├── SceneEditor/         Scene composition — what is placed where at edit time
├── Event/               Game events — zone triggers, entity triggers, conditions, actions
├── CharacterSheet/      Player character sheets — creation and in-game use
├── Campaign/            Save and load a full campaign
├── Camera/              Camera control — movement, orbit, zoom
├── EditorShell/         Editor UI shell — panels, layout, navigation between features
│
├── ── GAME ────────────────────────────────────────────────────────
│
├── Communication/       Chat, voice, and video between players
├── GameSession/         Game simulation — live entities, movement, combat, events
└── GameMaster/          Game master tools — overrides, live spawning, full control
```

### Key Dependencies

| Feature | Depends on |
|---|---|
| MapEditor | Grid, Shared |
| EntityEditor | AssetLibrary, Shared |
| SceneEditor | Grid, MapEditor, EntityEditor, Shared |
| Event | SceneEditor, Shared |
| CharacterSheet | Shared |
| Campaign | MapEditor, EntityEditor, SceneEditor, Event, CharacterSheet |
| EditorShell | All editor features (UI orchestration only) |
| GameSession | Campaign, Grid, Event, CharacterSheet, Shared |
| GameMaster | GameSession |
| Communication | (none — standalone technical feature) |

> **Rule:** dependencies only go **downward** in this list. A feature must never import from a feature that depends on it.

---

## Core Principles

### 1. Screaming Architecture

Folder names describe **what the project does**, not how it is built. Reading the top-level folder list should tell you what this engine is about.

```
✅  MapEditor/   EntityEditor/   GameSession/   GameMaster/
❌  Controllers/   Managers/   Helpers/   Utils/
```

### 2. Separation of Concerns

Each feature has a single reason to change. If changing the combat rules requires touching the chat UI, something is wrong.

### 3. Dependency Direction

Dependencies flow in one direction only:

```
Presentation  →  Application  →  Domain
Infrastructure  →  Application  →  Domain
```

The Domain layer knows nothing about Unity, UI, files, or networks. It contains pure business logic that can be tested without launching Unity.

### 4. One Feature, One Owner

In a team of six, each feature should have a clear owner. Conflicts arise when multiple people modify the same files. Screaming Architecture minimizes this by grouping everything related to a feature in one place.

### 5. Defer Decisions

Do not add complexity before you need it. Start with the simplest implementation that works. Refactor when you have real data showing it is not enough.

---

## Layer Reference

Every feature (except `Shared`) follows the same internal structure:

```
FeatureName/
├── Domain/           Pure business logic — no Unity, no UI
├── App/              Use cases — orchestrates Domain in response to user actions
├── Infrastructure/   Technical implementations — files, Unity APIs, GameObjects
└── Presenter/
    ├── ViewModels/   Observable state + commands — bridges Application and Views
    └── Views/        MonoBehaviours — input handling, rendering, data binding
```

### Domain

- Contains the rules and data that define the concept
- No Unity engine dependencies, no `MonoBehaviour`, no file I/O
- Using Unity type (Vector3) is allowed
- Can be unit tested without launching Unity
- Emits C# events when its state changes

```csharp
// ✅ Domain — pure rule, no Unity
public class Map {
    private readonly List<Tile> _tiles = new();
    public event Action<Tile> OnTileAdded;

    public bool IsValidPosition(Coord coord) { ... }

    public void PlaceTile(Coord coord, int prefabIndex) {
        var tile = new Tile(coord, prefabIndex);
        _tiles.Add(tile);
        OnTileAdded?.Invoke(tile);
    }
}
```

### Application

- Contains use cases: one class per user action
- Validates preconditions before calling Domain
- Does not know about UI or rendering

```csharp
// ✅ Application — validates then delegates to Domain
public class PlaceTileUseCase {
    private readonly Map _map;

    public void Execute(Coord coord, int prefabIndex) {
        if (!_map.IsValidPosition(coord)) return;
        if (_map.IsCellOccupied(coord)) return;
        _map.PlaceTile(coord, prefabIndex);
    }
}
```

### Infrastructure

- Contains everything that depends on a specific technology: Unity APIs, file system, JSON, network
- Implements interfaces defined in Domain or Application
- `GameObjectFactory` lives here — it is the only place that calls `Instantiate()`

### Presenter — ViewModels

- Plain C# class (no `MonoBehaviour`)
- Exposes `ObservableProperty` and `ObservableList` for Loxodon binding
- Calls Application use cases when commands are triggered
- Listens to Domain events to keep its state up to date

### Presentater — Views

- `MonoBehaviour` scripts only
- Reads input (mouse, keyboard, UI events)
- Binds to a ViewModel via Loxodon
- Contains no business logic — delegates everything to the ViewModel

```csharp
// ✅ View — detects input, delegates to ViewModel, no logic
public class PlacementView : UIView {
    private MapEditorViewModel _vm;

    protected override void OnEnable() {
        _vm = Context.GetApplicationContext().GetService<MapEditorViewModel>();
        var binding = this.GetBindingSet<MapEditorViewModel>();
        binding.Bind(...).To(vm => vm.CurrentMode);
        binding.Build();
    }

    private void Update() {
        if (Mouse.current.leftButton.wasPressedThisFrame)
            _vm.OnCellClicked(currentCell);
    }
}
```

---

## Feature Reference

### Shared

**Responsibility:** Atomic types used by two or more features. No business logic of its own.

**Contains:** `Coord`, `Stats`, custom `Math` types  
**Depends on:** nothing  
**Must not contain:** anything with a single owner, any MonoBehaviour, any Unity-specific logic

---

### Grid

**Responsibility:** Everything about how a grid works mathematically.

**Contains:** grid coordinates, neighbour calculation, distance, line of sight, walkability  
**Depends on:** Shared  
**Must not contain:** rendering, tile data, entities, Unity GameObjects

---

### MapEditor

**Responsibility:** Terrain generation — parameters, mesh output, visual result.

**Contains:** `TerrainParameters`, `TerrainGenerator`, terrain ViewModel and View  
**Depends on:** Grid, Shared  
**Must not contain:** entity placement, asset management, campaign serialization  
**Note:** `GridRenderer` (visual overlay) lives here as a purely cosmetic View — the grid coordinates do not depend on it

---

### AssetImporter

**Responsibility:** Import an external 3D file (GLTF) and copy it into the project.

**Contains:** GLTF parser, file copy logic, import UI  
**Depends on:** AssetLibrary (to register the imported asset)  
**Must not contain:** entity template creation, scene placement

---

### AssetLibrary

**Responsibility:** Catalogue of all available assets — single source of truth for what models exist.

**Contains:** `AssetReference` (name, path, thumbnail), library ViewModel, browser UI  
**Depends on:** Shared  
**Must not contain:** import logic, entity definitions

---

### EntityEditor

**Responsibility:** Define entity templates — what a Goblin Archer *is* (stats, events, AI behaviour, which asset it uses).

**Contains:** `NpcTemplate`, `ObjectTemplate`, `StructureTemplate`, editor UI  
**Depends on:** AssetLibrary, Shared  
**Must not contain:** scene placement, position data, live simulation state

**Prefab system:** from one imported goblin model, multiple templates can be created (Goblin Archer, Goblin Scout, Goblin Chief) each with different stat configurations. These templates are the reusable building blocks for scene composition and live spawning.

---

### SceneEditor

**Responsibility:** Scene composition — what entity is placed where, at edit time.

**Contains:** `SceneComposition` (authoritative list of all placed entities), `PlacedEntity` (template reference + position + stat overrides), `GameObjectFactory` (Infrastructure — the only place that calls `Instantiate()`), placement UI  
**Depends on:** Grid, MapEditor, EntityEditor, Shared  
**Must not contain:** live simulation state, combat logic, event triggering

```csharp
// PlacedEntity — edit-time placement record
public class PlacedEntity {
    public EntityTemplate Template { get; }   // what it is
    public Coord Position { get; }         // where it is
    public StatOverrides Overrides { get; }   // per-instance customisation
}
```

---

### Event

**Responsibility:** Define and trigger game events — zone-based or entity-based.

**Contains:** `GameEvent`, `ZoneTrigger` (area on the map, fires when a player enters), `EntityTrigger` (fires on entity interaction), `EventCondition`, `EventAction`, event editor UI  
**Depends on:** SceneEditor, Grid, Shared  
**Must not contain:** simulation execution (triggering during a session is handled by `GameSession`)

---

### CharacterSheet

**Responsibility:** Player character sheets — both creation and in-game use.

**Contains:** `Character` domain model, `CharacterCreatorView` (edit mode), `CharacterPlayerView` (in-game mode — spend resources, roll dice)  
**Depends on:** Shared  
**Must not contain:** session state, combat resolution

**Note:** two views, one model. The creation view and the player view share the same `Character` domain class and `CharacterViewModel`. Only the View differs.

---

### Campaign

**Responsibility:** Serialize and deserialize a complete campaign to disk.

**Contains:** `CampaignSerializer`, `CampaignLoader`, `ICampaignRepository`, save/load UI  
**Depends on:** MapEditor, EntityEditor, SceneEditor, Event, CharacterSheet  
**Must not contain:** live simulation, GameObjects, rendering

**What a campaign file contains:** terrain parameters, scene composition (placed entities + positions), event definitions, character sheets, asset references (paths to imported models)

---

### Camera

**Responsibility:** Camera control in the editor — movement, orbit, zoom.

**Contains:** `CameraController`, `CameraMovementManager`, `CameraOrbitManager`, `CameraZoomManager`  
**Depends on:** (none — standalone Unity system)  
**Must not contain:** game logic, entity data

---

### EditorShell

**Responsibility:** Orchestrate the editor UI — which panel shows which feature at any given moment.

**Contains:** `EditorShellViewModel`, `LeftPanelView`, `RightPanelView`, toolbar  
**Depends on:** all editor features (for panel injection only)  
**Must not contain:** business logic, domain data

**Pattern:** panels are empty containers. When the user selects an entity, `EditorShellViewModel` instructs the left panel to load `EntityEditorView` for that entity. The shell does not know what is inside the panels — it only knows which feature to load.

---

### Communication

**Responsibility:** Real-time communication between players — text chat, voice, video.

**Contains:** `IChatService`, `IVoiceService`, WebRTC/SignalR implementations, `ChatView`, `VoicePanelView`  
**Depends on:** (none — standalone technical feature)  
**Must not contain:** game rules, entity data, session state

---

### GameSession

**Responsibility:** Run the game simulation — live entity state, movement, combat, event triggering.

**Contains:**
- `LiveScene` — runtime equivalent of `SceneComposition`, authoritative position of all live entities
- `LiveNpc`, `LiveObject` — runtime entity state (current HP, position, AI state)
- `SimulationRunner` — the single MonoBehaviour that drives the simulation loop
- `NpcMovementSystem`, `CombatSystem`, `EventSystem` — pure C# systems, no MonoBehaviour
- `SceneInstantiator` (Infrastructure) — creates GameObjects from campaign data at session start

**Depends on:** Campaign, Grid, Event, CharacterSheet, Shared  
**Must not contain:** campaign serialization, editor tools, game master overrides

**Simulation pattern:** `SimulationRunner` (MonoBehaviour) calls `Tick(deltaTime)` on each system every frame. Systems modify `LiveScene` and `LiveNpc` data. Domain events notify ViewModels. ViewModels update their observable properties. Loxodon propagates changes to Views automatically.

```csharp
// SimulationRunner — the only MonoBehaviour in the simulation
public class SimulationRunner : MonoBehaviour {
    private NpcMovementSystem _movement;
    private CombatSystem _combat;
    private EventSystem _events;

    private void Update() {
        _movement.Tick(Time.deltaTime);
        _events.Tick(Time.deltaTime);
    }
}
```

---

### GameMaster

**Responsibility:** Give the game master elevated control over a running session.

**Contains:** `OverrideDamageUseCase`, `SpawnNpcUseCase`, `RevealZoneUseCase`, game master UI  
**Depends on:** GameSession  
**Must not contain:** simulation logic (delegates to GameSession), player-facing UI

---

## Decision Guide

Use this flowchart when you do not know where to put a new script.

```
Does my script inherit from MonoBehaviour?
├── YES → Presentation/Views/
│         It handles input, rendering, or data binding.
│         No business logic allowed here.
│
└── NO
    │
    ├── Does it expose ObservableProperty or commands for UI binding?
    │   └── YES → Presentation/ViewModels/
    │
    ├── Does it orchestrate a user action (validate + call Domain)?
    │   └── YES → Application/
    │             Name it as a use case: PlaceTileUseCase, SaveMapUseCase
    │
    ├── Does it contain pure business rules (no Unity, no UI)?
    │   └── YES → Domain/
    │
    ├── Does it read/write files, instantiate GameObjects, or call Unity APIs?
    │   └── YES → Infrastructure/
    │
    └── Is it used by two or more features with no logic of its own?
        └── YES → Shared/
```

**When a script spans multiple categories**, it has too many responsibilities. Split it. The most common case is a MonoBehaviour that also contains business logic — extract the logic into a use case or domain class, leave only input handling and binding in the View.

---

## Patterns

### Bootstrapper

Each scene has one `Bootstrapper` MonoBehaviour that wires everything together. It creates Domain objects, injects them into use cases, injects use cases into ViewModels, and registers ViewModels in the Loxodon context.

```csharp
public class MapEditorBootstrapper : MonoBehaviour {
    private void Awake() {
        // 1. Create Domain
        var map = new Map();

        // 2. Inject into use cases
        var placeTile = new PlaceTileUseCase(map);
        var removeTile = new RemoveTileUseCase(map);

        // 3. Inject into ViewModel (ViewModel listens to Domain events)
        var vm = new MapEditorViewModel(map, placeTile, removeTile);

        // 4. Register for Views to retrieve
        Context.GetApplicationContext()
               .GetServiceContainer()
               .Register<MapEditorViewModel>(vm);
    }
}
```

No class creates its own dependencies. The Bootstrapper is the only place where `new` is called for Domain and Application objects.

### Data Flow

```
[User input]
     │
     ▼
View                 Detects input → calls ViewModel method
     │
     ▼
ViewModel            Calls use case → no direct Domain mutation
     │
     ▼
Application          Validates → calls Domain
     │
     ▼
Domain               Mutates state → emits C# event
     │
     ▼
ViewModel            Listens to event → updates ObservableProperty
     │  (Loxodon automatic)
     ▼
View                 Refreshes automatically
```

### Assembly Definitions

Each feature layer has its own `.asmdef` to enforce dependency direction at compile time.

```
MapEditor.Domain.asmdef          → references: Shared
MapEditor.Application.asmdef     → references: MapEditor.Domain, Shared
MapEditor.Infrastructure.asmdef  → references: MapEditor.Application, MapEditor.Domain
MapEditor.Presentation.asmdef    → references: MapEditor.Application, MapEditor.Domain,
                                               Loxodon.Framework, UnityEngine.InputSystem
```

If a Domain class accidentally imports from Presentation, the project will not compile. This is intentional.

---

## Inter-Feature Communication

Features must not import each other's internal classes directly (except along declared dependency lines). When two features need to communicate, use one of these approaches:

**Shared Domain type** — if the data is genuinely shared, it belongs in `Shared/`.

**Interface in the consumer** — the consuming feature defines an interface; the providing feature implements it. The Bootstrapper injects the implementation.

**C# events on Domain objects** — Domain objects emit events. Any feature that holds a reference to that Domain object can listen. No direct coupling between features.

**Loxodon context** — ViewModels registered in the Loxodon context can be retrieved by any View in the scene. Use this for cross-feature UI coordination (e.g., `EditorShell` reading state from `SceneEditorViewModel`).