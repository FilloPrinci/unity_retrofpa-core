# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed

- `DialogueUIController`, `PauseMenuUIController`, `SettingsUIController`,
  `InventoryUIController`: their `OnDisable()` was a plain method that
  hid (rather than overrode) `UIScreen.OnDisable()`, so the base cleanup
  (resetting `IsVisible`, decrementing the shared visible-screen count,
  potentially leaving the cursor locked) never ran when one of these was
  disabled. Now `protected override void OnDisable()` calling
  `base.OnDisable()`.

### Changed

- **Breaking:** `InventoryManager` now holds a fixed number of slots
  (`Capacity`, `[SerializeField] capacity`, default 12) instead of an
  unbounded growing list. `Entries` replaced by `Slots`
  (`IReadOnlyList<InventoryEntry>`, same length as `Capacity`, null entries
  are empty slots). `AddItem` now returns `int` (how many were actually
  added — less than requested, possibly 0, when full) instead of `void`.
  `ItemChanged` event replaced by `SlotChanged(int index, InventoryEntry)`,
  fired per affected slot instead of per item. `CollectibleItem` updated to
  log a warning when a pickup doesn't fully fit.
- `InventorySlotUI`: reworked into a clickable, selectable grid cell
  (icon + quantity badge only — no longer shows the item name inline).
  Requires a `Button`; exposes `Clicked`, `Set`, `SetEmpty`, `SetSelected`.
- `InventoryUIController`: rebuilt around a fixed grid (empty slots stay
  visible), slot selection driving a name/description detail panel, an
  Equip/Unequip toggle button, and a live 3D preview of the equipped
  item's `WorldPrefab` — rendered by a dedicated `previewCamera` into a
  `RenderTexture` created at runtime (assigned to a `RawImage`), using a
  new `ItemPreview` layer (project-template `TagManager.asset`, index 8)
  so only that camera sees the preview instance. Needs a `wielder`
  reference (the player) wired in the Inspector to call Equip/Unequip.

### Added

- `PersistentSingleton<T>`: base class for MonoBehaviour singletons that
  survive additive scene loads (Unity equivalent of a Godot autoload).
- `GameManager`: tracks `GameState` (Boot/Loading/Playing/Paused), reacts to
  `LevelSceneManager`'s events, drives `Time.timeScale` on pause.
- `LevelSceneManager`: additive level loading/unloading (never
  `LoadSceneMode.Single`), places a persistent player root at a `SpawnPoint`
  after each load.
- `SpawnPoint` component, with an id to disambiguate multiple spawn points
  per level.
- `GameBootstrapper`: drop-in component that triggers the first level load
  on startup, configured entirely from the Inspector (no project-specific
  code required).
- `VisualStyleProfile`: data-driven fog/ambient/color-adjustments/bloom/
  tonemapping style, applied to RenderSettings and a URP `VolumeProfile`.
- `StyleManager`: applies a `VisualStyleProfile` to a persistent global
  `Volume`; switching style is just assigning a different profile.
- Package now depends on `com.unity.render-pipelines.universal`; the
  Runtime assembly references the URP/Core Rendering assemblies.
- `FresnelPulse`: pulses a shader fresnel-intensity property via
  `MaterialPropertyBlock` (never clones/animates the shared Material).
  Expects the future 2-layer retro Shader Graph to expose a matching
  float property (`_FresnelPulse` by default).
- `RetroTwoLayer` Shader Graph (URP Lit): 2 independent texture layers
  (tiling/offset/scroll), 5 blend modes (Alpha Over/Multiply/Additive/
  Subtract/Divide) via an Enum Keyword, always-on fresnel rim-light, and
  a code-driven additive flash — both fed into Emission.

- `Rotator`: continuously rotates its object (e.g. a spinning collectible).
- `Interactable`: generic "can be interacted with" building block, exposing
  both a `UnityEvent` (Inspector-wired) and a C# event (code subscribers).
- `Collectible`: requires `Interactable`; raises `Collected` once then
  disables the object. Knows nothing about inventory/items — a future
  InventoryManager hooks into the event instead.
- `SceneChangeTrigger`: loads another level (via `LevelSceneManager`) when
  the player enters its trigger volume.
- `NpcBase`: wraps the Animator on NPC prefabs, with an
  `AnimatorOverrideController` slot to remap a shared base controller's
  placeholder clips onto a character's real clips.
- `ItemData`: data describing one kind of item (id, name, description, icon,
  world prefab, max stack size), with an optional `EquippableBehavior` for
  equippable items.
- `EquippableBehavior` (abstract) + `MeleeEquippableBehavior` /
  `RangedEquippableBehavior`: define OnEquip/OnUnequip/PerformAction for
  weapons; actual hit detection/damage is left to a future combat system.
- `InventoryManager`: stacks `ItemData` by `MaxStackSize`, tracks the
  currently equipped item, raises `ItemChanged`/`EquippedItemChanged`.
- `CollectibleItem`: bridges a `Collectible` to `InventoryManager` (adds an
  `ItemData` on collection), keeping `Collectible` itself item-agnostic.
- `DialogueData`: a dialogue as a flat, index-linked list of `DialogueNode`s
  (speaker name + `LocalizedString` text, optional branching `DialogueChoice`s) —
  a lightweight custom format, not a third-party plugin/graph tool.
  Dialogue text uses Unity's Localization package (`LocalizedString`).
- `DialogueManager`: runs a `DialogueData` node by node
  (`StartDialogue`/`Advance`/`SelectChoice`/`EndDialogue`), with no UI code
  of its own — raises `DialogueStarted`/`NodeChanged`/`DialogueEnded` for a
  future dialogue UI to subscribe to.
- `DialogueTrigger`: starts a `DialogueData` in `DialogueManager` when
  interacted with.
- Package now also depends on `com.unity.localization`; the Runtime
  assembly references `Unity.Localization`.
- `DialogueDataEditor` (custom Inspector): replaces raw "Next Node Index"
  integers with dropdowns listing every node (by index + text preview) plus
  "End Dialogue" — no more counting node indices by hand.
- `DialogueValidator` / `ItemValidator` (Retro FPA/Validate/... menu items):
  scan all `DialogueData`/`ItemData` assets for structural issues (dangling
  node references, unreachable nodes, empty/duplicate item ids, missing
  icon/world prefab).
- `RetroFpaWindow` (Window/Retro FPA/Dashboard): dock window running both
  validators and listing their results.

### Fixed

- Editor assembly now references `Unity.Localization` (needed to resolve
  `DialogueNode.Text`'s `LocalizedString` type from `DialogueDataEditor`).
- `PlayerEquipmentController`, `CollectibleItem`, `DialogueTrigger`,
  `GameBootstrapper`, `SceneChangeTrigger`: guard against the relevant
  manager singleton not existing in the scene (logs a clear error instead
  of throwing a `NullReferenceException`).
- `FirstPersonController`, `PlayerInteractor`, `PlayerEquipmentController`:
  gameplay input (look/move/interact/attack) now gated on
  `FirstPersonController.IsCursorLocked`, so it stops while any UI screen
  is open instead of still reading mouse look / raycasting / firing
  actions underneath the menu.
- `FirstPersonController.Teleport` (implements new `ITeleportable`)
  properly resets accumulated fall velocity and briefly disables the
  `CharacterController` during the move; `LevelSceneManager` now calls
  this (falling back to a plain Transform move) instead of only ever
  setting the Transform directly, which left stale fall velocity from
  before the teleport and could tunnel the player through the floor.
  Also added a `maxFallSpeed` clamp.
- `MainMenuUIController`, `PauseMenuUIController`, `DialogueUIController`:
  added `LocalizedString` label fields (New Game/Settings/Quit,
  Resume/Settings/Quit, Continue) applied to each button's child
  `TMP_Text` on `Awake`, so static button labels go through Localization
  instead of being hardcoded in the Inspector.
- `SettingsManager`: loads/saves/applies master/music/sfx volume, look
  sensitivity, locale, vsync and resolution via `PlayerPrefs` — persists
  across sessions. Reacted to via static events
  (`MasterVolumeChanged`/`MusicVolumeChanged`/`SfxVolumeChanged`/
  `LookSensitivityChanged`/`LocaleChanged`) rather than reaching into other
  systems directly; `FirstPersonController` listens for
  `LookSensitivityChanged`. Music/SFX volume are stored and exposed for a
  future audio system to consume — none exists yet.
- `SettingsUIController`: settings screen (sliders for volumes/look
  sensitivity, a language dropdown, a vsync toggle, a resolution
  dropdown), opened as an overlay from the main menu and/or pause menu's
  new "Settings" button (`settingsScreen` field on each). Added a
  `closeLabel` field, applied the same way as the other button labels.
  Added a row-label `TMP_Text` + `LocalizedString` pair per control
  (master/music/sfx volume, look sensitivity, language, vsync,
  fullscreen, resolution) — the controls had no text explaining what they
  were.
- `SettingsManager`/`SettingsUIController`: added a Fullscreen toggle
  (`Screen.fullScreenMode`, `FullScreenWindow` vs `Windowed`), persisted
  like the other settings.
- `UIScreen`: screens with `hiddenOnStart` true were never actually hidden
  at startup. A fresh `CanvasGroup` defaults to alpha 1 regardless of
  `hiddenOnStart`, and the old `Start()` routed through the same
  equal-value-skips-update guard used by `Show`/`Hide`, so the initial
  "hide" call for a screen whose `IsVisible` field already defaulted to
  `false` was silently skipped. `Start()` now applies the initial
  alpha/interactable/cursor-count state directly instead of going through
  that guard.

### Changed

- `NpcBase`/`WorldItemBase_Physical`/`WorldItemBase_Pickupable` prefabs and
  the shared `NpcBaseController` + placeholder clips moved from the test
  project into `Samples~/BasePrefabs`, declared in `package.json` as the
  "Base Prefabs" sample. GUIDs preserved so existing scene references keep
  resolving after re-importing the sample.

### Added

- `FirstPersonController`: minimal first-person movement (CharacterController
  move + mouse look), reading from Input System `InputActionReference`s.
  No jump/crouch/sprint/head-bob yet. Locks and hides the cursor while
  enabled; exposes a static `SetCursorLocked` for a future UI (pause menu,
  inventory) to release it without this controller knowing about that UI.
- `PlayerInteractor`: raycasts forward and calls `Interactable.Interact` on
  hit, on the interact input.
- `PlayerEquipmentController`: fires `InventoryManager.EquippedItem`'s
  `EquippableBehavior.PerformAction` on the attack input.
- Package now also depends on `com.unity.inputsystem`; the Runtime assembly
  references `Unity.InputSystem`.
- `UIScreen`: base class for a `CanvasGroup`-driven show/hide UI panel;
  tracks how many screens are visible across the whole game so the cursor
  only re-locks once none are.
- `DialogueUIController`: speaker/text/choice-or-continue UI driven by
  `DialogueManager`'s events.
- `InventoryUIController` + `InventorySlotUI`: toggleable inventory screen
  listing `InventoryManager`'s entries.
- `InteractionPromptUI`: shows `Interactable.PromptText` (or a default)
  while looking at an interactable object.
- `MainMenuUIController`: "New Game" calls `GameBootstrapper.StartGame`;
  "Quit" exits.
- `PauseMenuUIController`: toggled by the UI action map's Cancel action;
  pause/resume goes through `GameManager.SetGameState`.
- `PlayerInteractor` now raycasts every frame (not just on interact) and
  raises a static `LookTargetChanged` event, for `InteractionPromptUI`.
- `GameBootstrapper` gained `autoStartOnAwake` (default true) and a public
  `StartGame()`, so a main menu can gate the first level load instead of
  it happening automatically.
- Package now also depends on `com.unity.ugui`; the Runtime assembly
  references `UnityEngine.UI` and `Unity.TextMeshPro`.

### Changed

- `Interactable.PromptText`, `ItemData.DisplayName`, `ItemData.Description`
  changed from plain `string` to `LocalizedString`, so item names/
  descriptions and interaction prompts go through the Localization package
  like `DialogueData` already did. `InventorySlotUI`/`InteractionPromptUI`
  resolve them via `GetLocalizedString()`. `ItemValidator` updated
  (`DisplayName.IsEmpty` instead of `string.IsNullOrEmpty`).
  Note: existing `ItemData` assets authored before this change will have
  their Display Name/Description reset to empty (Unity doesn't migrate a
  field's serialized value across an incompatible type change) — re-enter
  them via the Localization workflow.

### Changed

- `FresnelPulse` now pulses once (min → max → min) and then holds at
  `minValue` for a configurable `pulsePause` (seconds) before repeating,
  instead of oscillating continuously.

## [0.1.0] - 2026-09-05

### Added

- Initial package skeleton: `package.json`, Runtime/Editor Assembly
  Definitions, `Samples~` folder.
