# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

### Changed

- `FresnelPulse` now pulses once (min → max → min) and then holds at
  `minValue` for a configurable `pulsePause` (seconds) before repeating,
  instead of oscillating continuously.

## [0.1.0] - 2026-09-05

### Added

- Initial package skeleton: `package.json`, Runtime/Editor Assembly
  Definitions, `Samples~` folder.
