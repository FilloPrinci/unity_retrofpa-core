# Retro FPA Core

A Unity UPM package providing the reusable "engine" for retro low-poly
(PS1 / N64 / GameCube-era) first-person horror/adventure games: decoupled
manager systems, a full first-person player controller, a data-driven UI
shell (dialogue, inventory, menus, settings), data-driven `ScriptableObject`
content, ready-to-place prefabs, a custom retro-style rendering pipeline, and
editor tooling for authoring dialogue and items.

This package is the Unity counterpart of an existing Godot 4.6.3 "Retro FPA"
template. It is not a literal code port — it reproduces the same
architectural philosophy (decoupled systems via events, data-driven content,
levels assembled from prefabs) using idiomatic Unity patterns instead of
Godot ones. See [`unity_retrofpa_kickoff_brief.md`](https://github.com/FilloPrinci/unity_retrofpa-project-template/blob/main/unity_retrofpa_kickoff_brief.md)
in the sibling [`retrofpa-project-template`](https://github.com/FilloPrinci/unity_retrofpa-project-template)
repository for the full design rationale.

This package contains **no game-specific content** — it is meant to be
consumed by one or more Unity projects, such as `retrofpa-project-template`.

## What's here

- **Core managers** (`Runtime/Core/`) — `GameManager` (game state),
  `LevelSceneManager` (additive level load/unload, spawn placement),
  `GameBootstrapper` (drives the first level load, gate-able from a main
  menu), `InventoryManager`, `DialogueManager`, `SettingsManager`
  (persisted audio/look-sensitivity/locale/graphics settings),
  `StyleManager` (the game's global visual style — see below),
  `SaveManager` (one JSON save slot — level, exact player position,
  inventory, collected items — see below), `AudioManager` (global UI/menu
  sounds + one-shot world sounds — see "Global vs. per-object/per-scene
  audio" below), and `PersistentSingleton<T>`, the base class all of them
  share (Unity's equivalent of a Godot autoload).
- **Player** (`Runtime/Components/Player/`) — `FirstPersonController`
  (move/look/cursor-lock, New Input System), `PlayerInteractor` (raycasts
  for `Interactable`s, drives the interaction prompt), `PlayerEquipmentController`
  (attack input → the equipped item's `EquippableBehavior`), `FootstepAudio`
  (raycasts down for a `SurfaceAudio` while walking).
- **World components** (`Runtime/Components/`) — `Interactable` (generic
  "this can be interacted with" building block, optional interact sound),
  `Collectible`/`CollectibleItem` (pickup → `InventoryManager`, optional
  pickup sound), `DialogueTrigger` (interact → starts a `DialogueData`),
  `SceneChangeTrigger` (walk into a volume → load a level),
  `InteractableSceneChangeTrigger` (interact with an object → load a level —
  a door/ladder/exit, as opposed to a volume), `SceneAtmosphere` (gives one
  level scene its own fog/skybox — see below), `SceneAmbientAudio` (gives
  one level scene its own looping ambient track), `SurfaceAudio` (optional
  per-surface footstep sound), `SaveableId` (a stable per-instance id) +
  `SaveableCollectible` (bridges `Collectible` → `SaveManager`, so a pickup
  stays gone across saves/revisits), `SpawnPoint`, `NpcBase` (Animator +
  `AnimatorOverrideController` slot), `Rotator`, `FresnelPulse`.
- **UI shell** (`Runtime/UI/`) — `UIScreen`, the `CanvasGroup`-based base
  class every screen below builds on (show/hide without disabling the
  GameObject, shared cursor-lock/unlock counting across however many screens
  are open at once): `DialogueUIController` (speaker/body text, linear or
  branching choices), `InventoryUIController` + `InventorySlotUI` (a fixed
  grid of slots, selection → name/description detail panel, an Equip/Unequip
  toggle, a live 3D preview of the equipped item rendered by a dedicated
  camera into a `RenderTexture`), `MainMenuUIController` (New Game, Continue
  — disabled with no save file, Settings, Quit), `PauseMenuUIController`
  (Resume, Save, Settings, Quit), `SettingsUIController` (audio volumes,
  look sensitivity, locale, VSync, fullscreen, resolution),
  `InteractionPromptUI`, `UIButtonSound` (add to any `Button` for the global
  hover/confirm sounds).
- **Data** (`Runtime/Data/`) — `ItemData` (icon, world prefab, equipped-model
  prefab, optional `EquippableBehavior`), `EquippableBehavior` +
  `MeleeEquippableBehavior`/`RangedEquippableBehavior`/`HeldItemEquippableBehavior`
  subclasses, `DialogueData` (nodes + branching choices, localized),
  `VisualStyleProfile` (the game's global look), `SceneAtmosphereProfile`
  (one level's fog/skybox), `AudioProfile` (the game's global sounds — see
  below), `ItemDatabase` (hand-maintained item-id → asset lookup, used by
  `SaveManager` to resolve a saved item back to its asset).
- **Shaders** (`Runtime/Shaders/`) — `RetroTwoLayer` (2-layer blend + fresnel
  + hit-flash Shader Graph), `Retro FPA/Gradient Skybox` (flat 2-color
  vertical gradient, since URP's procedural sky can't produce a stylized
  flat look).
- **Editor tooling** (`Editor/`) — a `Window/Retro FPA/Dashboard` EditorWindow,
  a custom inspector for `DialogueData`, content validators
  (`Retro FPA/Validate/Items` and `/Dialogues`), and a custom inspector for
  `SceneAtmosphereProfile` (groups fog/skybox colors with a "sync" button).
- **Samples~** — ready-to-import base prefabs (`NpcBase`, `WorldItemBase`
  Physical/Pickupable variants). See [`Samples~/README.md`](Samples~/README.md).

`Runtime` and `Editor` are separate Assembly Definitions
(`FilloPrinci.RetroFpa.Runtime` and `FilloPrinci.RetroFpa.Editor`, the latter
referencing the former and restricted to the `Editor` platform), so editor-only
code never ships in player builds.

## Global style vs. per-level atmosphere

`VisualStyleProfile` and `SceneAtmosphereProfile` look similar (both drive
render settings) but are intentionally split by scope:

- **`VisualStyleProfile`** (ambient light, color grading, bloom, tonemapping,
  texture filtering) is the game's overall "look" — assigned once to
  `StyleManager` and kept applied across every level load. Two levels in the
  same game should look like the same game.
- **`SceneAtmosphereProfile`** (fog, skybox) is per-level on purpose — a
  `SceneAtmosphere` component in a level scene applies it via
  `StyleManager.ApplySceneAtmosphere` whenever that scene finishes loading
  (`LevelSceneManager.LevelLoaded`, not `Start()` — see the class doc comment
  for why the timing matters). Two rooms in the same game can reasonably
  want completely different fog color/density or sky.

Changing the whole game's style is reassigning `StyleManager`'s
`VisualStyleProfile`; changing one level's mood is reassigning that level's
`SceneAtmosphere` component's `SceneAtmosphereProfile` — independently.

## Global vs. per-object/per-scene audio

Same split again, this time for sound, through `AudioManager`:

- **Global, via `AudioProfile`** — UI hover/confirm sounds, the main menu
  music, and the *default* footstep/pickup/interact sounds. Assigned once to
  `AudioManager` and kept for the whole session.
- **Per-object override, optional** — `Interactable.interactSound`,
  `Collectible.pickupSound`, `SurfaceAudio.footstepSound` (checked via a
  downward raycast from `FootstepAudio` on the player). Each falls back to
  `AudioProfile`'s matching default when left empty, so a game can ship with
  just the defaults set and add per-object variety later without touching
  any code.
- **Per-scene, via `SceneAmbientAudio`** — one level's looping ambient
  track/music, independent of every other level. No default/fallback here,
  same as `SceneAtmosphereProfile`'s skybox: a level with none just plays
  nothing.

All three play through `AudioManager`, which is safe to call with a null
clip anywhere (nothing plays, no error) — a project can wire up the whole
audio system before a single sound asset exists.

### Using your own audio

This package ships **no audio files** — you bring your own, and nothing in
it depends on any particular sound library. To set it up in a project:

1. Create an `AudioProfile` (`Assets → Create → Retro FPA → Audio Profile`)
   and assign your clips: UI hover/confirm, main menu music, and the default
   footstep/pickup/interact sounds. Any field left empty is simply silent.
2. Add an `AudioManager` to your persistent scene (next to the other
   managers) and assign the profile to its *Initial Profile*.
3. Add `UIButtonSound` to each `Button` that should make UI sounds, and
   `FootstepAudio` to the player.
4. Optionally add per-level `SceneAmbientAudio`, per-surface `SurfaceAudio`,
   and per-object clips on `Interactable`/`Collectible`.

Because assets are referenced by Unity GUID, the audio you assign lives in
*your* project, not in this package.

## Installing into a project

While developing this package alongside a consuming project, reference it by
local file path in the project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.filloprinci.retrofpa": "file:../../unity_retrofpa-core"
  }
}
```

(the exact relative path depends on where the two repositories sit relative
to each other on disk — see `retrofpa-project-template`'s manifest for a
working example).

Once the package stabilizes and a per-project "pull updates" workflow is
needed instead of paired local development, this will be switched to a
git URL pinned to a release tag, e.g.:

```json
{
  "dependencies": {
    "com.filloprinci.retrofpa": "https://github.com/FilloPrinci/unity_retrofpa-core.git#v0.1.0"
  }
}
```

## Versioning

This package follows [Semantic Versioning](https://semver.org/) and tags
releases accordingly. See [`CHANGELOG.md`](CHANGELOG.md) for release notes.

## Design principles

- **Decoupled systems.** Managers communicate through C# events, not direct
  references, mirroring the Godot template's autoload + signal pattern.
- **Data-driven content.** Game content (items, dialogue, visual style,
  level atmosphere) is authored as `ScriptableObject` assets, not hardcoded.
- **Prefabs over runtime construction.** Content that Godot built by having
  `@tool` scripts self-assemble in the editor is instead built as real Prefab
  Variants in Unity — visible immediately in the Scene view, no runtime
  construction code. Any one-off regeneration action (e.g. "rebuild collider
  from mesh") is an explicit button in a Custom Editor, never automatic
  `OnValidate` logic.
- **No shared-Material animation.** Per-instance visual effects (hit flash,
  fresnel pulse) are driven through `MaterialPropertyBlock`, never by cloning
  or animating a shared `Material`.
- **Additive scene loading only.** Levels are always loaded additively over a
  persistent scene; `LoadSceneMode.Single` would destroy persistent state and
  is never used for levels.
- **Only `Start()` may assume another singleton already exists.** `Awake()`
  and `OnEnable()` run in an order Unity doesn't guarantee across different
  objects/scenes — code that reads another manager's `Instance` synchronously
  (rather than reacting to an event) belongs in `Start()`, not `OnEnable()`.

## Known gaps

Not yet implemented — real gaps, not oversights:

- **Combat/damage.** `MeleeEquippableBehavior`/`RangedEquippableBehavior.PerformAction`
  are hook points only; there's no `Health`, `TakeDamage`, or death/respawn
  flow yet. Equipping a weapon plays no gameplay role beyond the input path.
- **HUD.** Only `InteractionPromptUI` exists; no health/ammo/objective
  display (would follow naturally once combat exists).
- **Scene Template.** No Unity Scene Template asset yet for scaffolding a
  new level (fog/skybox/SpawnPoint pre-wired) — new levels are still built
  by hand or duplicated from an existing one.

## License

[MIT](LICENSE)
