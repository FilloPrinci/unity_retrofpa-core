# Retro FPA Core — Developer Guide

For anyone **changing the package itself**: fixing bugs, adding features,
releasing versions. That includes people and AI coding agents (e.g. Claude
Code) working on it without prior context, so it states the rules
explicitly. If you only use the package from the Editor, read the
[User Guide](user-guide.md).

Read [§1](#1-ground-rules) before touching code. Everything after it is
reference.

---

## Contents

1. [Ground rules](#1-ground-rules)
2. [Repository layout](#2-repository-layout)
3. [Development workflow](#3-development-workflow)
4. [Architecture](#4-architecture)
5. [Event catalogue](#5-event-catalogue)
6. [Lifecycle and ordering rules](#6-lifecycle-and-ordering-rules)
7. [Subsystem notes](#7-subsystem-notes)
8. [Code conventions](#8-code-conventions)
9. [Recipes](#9-recipes)
10. [Compatibility: what breaks consumers](#10-compatibility-what-breaks-consumers)
11. [Releasing](#11-releasing)
12. [Change checklist](#12-change-checklist)
13. [Known gaps and caveats](#13-known-gaps-and-caveats)

---

## 1. Ground rules

1. **No game-specific content.** This is a reusable engine package. Anything
   that only makes sense for one game (its items, its doors, its story logic)
   belongs in that game's project, not here.
2. **Every file and folder outside `*~` folders needs a committed `.meta`
   file** with a unique GUID. Consumers install the package from git, where
   it's immutable: Unity can't generate missing metas and ignores the asset.
   **Never change an existing GUID**: consumer scenes and prefabs reference
   scripts and assets by GUID.
3. **Don't rename serialized fields** without `[FormerlySerializedAs("old")]`.
   Consumer scenes, prefabs and assets store values by field name. Also grep
   for the name as a string: editors and validators read some fields through
   `SerializedObject.FindProperty("…")` (see [§10](#10-compatibility-what-breaks-consumers)).
4. **Systems talk through static C# events**, not direct references between
   managers. A new dependency between two managers is a design smell.
5. **Only `Start()` (or an event handler) may read another singleton's
   `Instance`.** `Awake`/`OnEnable` order across objects isn't guaranteed.
   Several past bugs came from this ([§6](#6-lifecycle-and-ordering-rules)).
6. **Levels load additively, always.** Never `LoadSceneMode.Single`.
7. **Never modify a shared `Material` at runtime.** Per-instance effects use a
   `MaterialPropertyBlock`; per-session changes use a material instance you
   own (as `StyleManager` does for the skybox).
8. **No automatic `OnValidate` regeneration.** One-off editor actions are
   explicit Inspector buttons with Undo.
9. **All player-facing text is a `LocalizedString`.** (Exceptions: dialogue
   speaker names and key labels, documented as such.)
10. **Missing optional pieces must not throw.** Use `Manager.Instance?.Do()`
    for optional collaborators, and accept null clips/profiles silently where
    "nothing" is a valid configuration.
11. **Keep docs in step**: `CHANGELOG.md` (*Unreleased*) for every
    user-visible change; `README.md` and these guides when behaviour or
    setup changes.
12. **Don't release, tag or push unless asked to.** See [§11](#11-releasing).

---

## 2. Repository layout

```
package.json                  UPM manifest (name com.filloprinci.retrofpa, version, deps, samples)
README.md / CHANGELOG.md      Overview / Keep-a-Changelog release notes
CLAUDE.md                     Short brief for AI agents; points here
Documentation~/               These guides (ignored by Unity, no metas)
Runtime/                      Assembly FilloPrinci.RetroFpa.Runtime   namespace FilloPrinci.RetroFpa
  Core/                       Managers (PersistentSingleton<T>), GameState, SaveData, ITeleportable
  Components/                 MonoBehaviours placed in scenes/prefabs
    Player/                   FirstPersonController, PlayerInteractor, PlayerEquipmentController, FootstepAudio
  Data/                       ScriptableObjects (ItemData, DialogueData, profiles, behaviours, ItemDatabase)
  UI/                         UIScreen and its screens, prompt, slot, button sound
  Shaders/                    RetroTwoLayer.shadergraph, GradientSkybox.shader
Editor/                       Assembly FilloPrinci.RetroFpa.Editor    namespace FilloPrinci.RetroFpa.Editor
                              Custom inspectors, validators, Dashboard window (Editor platform only)
Samples~/BasePrefabs/         Importable prefabs (NpcBase, WorldItemBase_*), placeholder animations
```

- **Runtime asmdef** references URP Core/Universal, Localization, Input
  System, uGUI, TextMesh Pro. Adding a reference means adding the package to
  `package.json` `dependencies` too, if it isn't there yet.
- **Editor asmdef** references Runtime + Localization, `includePlatforms:
  ["Editor"]`.
- Target: Unity **6000.3** (`unityRelease 17f1`), URP 17.3.
- Line endings: **LF**. On Windows/mixed setups the working copy can show
  files as modified only because of CRLF; check with `git diff
  --ignore-cr-at-eol` and restore them rather than committing them.
- There is **no test assembly**. Verification happens in a consuming project
  ([§3.3](#33-verifying-a-change)).

---

## 3. Development workflow

### 3.1 Work against a consuming project

The package can't be opened in Unity on its own. Clone it next to a project
that uses it (the
[project template](https://github.com/FilloPrinci/unity_retrofpa-project-template),
or a game), and point that project's `Packages/manifest.json` at your clone:

```json
"com.filloprinci.retrofpa": "file:../../unity_retrofpa-core"
```

(the path is relative to `Packages/`; adjust it to your layout). Unity then
compiles your working copy directly, and edits show up on recompile.

- **Edit the package repo, never `Library/PackageCache/`** in the consuming
  project. That copy is overwritten on re-resolve.
- **Don't commit the consuming project with a `file:` dependency.** It won't
  resolve on anyone else's machine. Switch it back to a git URL with a tag
  (or a commit SHA) before committing there. If the project needs your new
  core changes, release the core first ([§11](#11-releasing)).
- New files created through Unity get a `.meta` automatically, since
  `file:` packages are writable. **Commit those metas.** Files created
  outside Unity (by hand, by an agent) get their meta only once Unity
  imports them, so open/refresh the consuming project before committing, or
  write the meta yourself with a fresh random 32-hex-digit GUID, copying the
  format of a sibling file's meta.

### 3.2 Commit messages

English, imperative, one subject line. Prefix with the component when the
change is local to one:

```
Add ChildTransformRandomizer and ChildGroundAnchor
Fix RetroTwoLayer rendering semi-transparent areas black
UIButtonSound: restore the hover sound on mouse pointer-enter
README: installing from a git tag is now the normal path
Release 0.2.0
```

A body is welcome when the *why* isn't obvious. Keep docs-only changes in
separate commits when practical.

### 3.3 Verifying a change

With the consuming project open on the `file:` dependency:

1. **Compiles** with no errors or new warnings in the Console (both
   assemblies; the Editor assembly only builds in the Editor, so also do a
   player build occasionally, or at least check you didn't use `UnityEditor`
   from Runtime).
2. **Play from the persistent scene**: main menu → New Game → walk, interact,
   pick up, open inventory/pause/settings, change level, save, Continue.
   Exercise whatever you touched, plus anything listening to the same events.
3. **Run the validators** (*Window → Retro FPA → Dashboard*). If you changed
   what a valid scene/item/dialogue is, update the validator.
4. **Check the Inspector** of anything whose serialized fields you touched,
   on an *existing* prefab/scene, so old data still loads.

Agents with access to a running Editor (e.g. through a Unity CLI or MCP
bridge) can do steps 1–3 themselves. Otherwise say clearly what was and wasn't
verified, and ask the human to run it.

---

## 4. Architecture

The package is the Unity counterpart of a Godot template: **autoloads +
signals** became **persistent singletons + static C# events**, and **a shell
scene swapping a child level** became **a persistent scene + one additive
level scene**.

### 4.1 Persistent singletons

```csharp
public abstract class PersistentSingleton<T> : MonoBehaviour where T : PersistentSingleton<T>
{
    public static T Instance { get; private set; }
    protected virtual void Awake()   { /* first wins; duplicates Destroy(gameObject); DontDestroyOnLoad */ }
    protected virtual void OnDestroy() { /* clears Instance if it was this */ }
}
```

Managers: `GameManager`, `LevelSceneManager`, `InventoryManager`,
`DialogueManager`, `SettingsManager`, `StyleManager`, `SaveManager`,
`AudioManager`. (`GameBootstrapper` is a plain MonoBehaviour in the
persistent scene.)

Every subclass `Awake` follows this exact shape:

```csharp
protected override void Awake()
{
    base.Awake();
    if (Instance != this)
    {
        // Duplicate instance, already scheduled for destruction by the base class.
        return;
    }

    // self-setup and static event subscriptions only
}

protected override void OnDestroy()
{
    base.OnDestroy();
    // unsubscribe everything subscribed in Awake
}
```

### 4.2 Who depends on whom

```
             LevelSceneManager ──events──► GameManager, StyleManager, SaveManager,
                    ▲                      SceneAtmosphere, SceneAmbientAudio,
                    │ LoadLevel()          PlayerInteractor, DescriptionUIController
   GameBootstrapper, SceneChangeTrigger,
   InteractableSceneChangeTrigger, SaveManager.LoadGame

   SettingsManager ──events──► AudioManager (music/SFX), FirstPersonController (sensitivity)
   InventoryManager ─events──► InventoryUIController
   DialogueManager ──events──► DialogueUIController
   PlayerInteractor ─events──► InteractionPromptUI
   Checkable (static) ───────► DescriptionUIController

   Calls into managers (via Instance, from components/UI):
     Interactable/Collectible/FootstepAudio/UIButtonSound/MainMenu → AudioManager
     CollectibleItem, PlayerEquipmentController, InventoryUI       → InventoryManager
     DialogueTrigger, DialogueUI                                    → DialogueManager
     SaveableCollectible, MainMenu, PauseMenu                       → SaveManager
     SceneAtmosphere                                                → StyleManager
     PauseMenu                                                      → GameManager
     SettingsUI                                                     → SettingsManager
     SaveManager                                                    → LevelSceneManager, InventoryManager
```

Managers never call UI. UI and components call managers. The only
manager-to-manager calls are `SaveManager` → `LevelSceneManager` /
`InventoryManager` (it has to orchestrate a restore) and `GameBootstrapper` →
`LevelSceneManager`.

### 4.3 Composition around `Interactable`

`Interactable` knows nothing about what an interaction *means*. It plays a
sound and raises its instance event `Interacted(GameObject interactor)` (plus
a UnityEvent for designers). Behaviours are sibling components that
`[RequireComponent(typeof(Interactable))]`, cache it in `Awake`, and
subscribe in `OnEnable`/`OnDisable`:

```
Interactable ─Interacted─► Checkable, Collectible, DialogueTrigger, InteractableSceneChangeTrigger
Collectible  ─Collected──► CollectibleItem (→ inventory), SaveableCollectible (→ save)
```

Each bridge exists so the component before it stays usable without the
system after it (`Collectible` doesn't know about inventory or saves). Follow
the same pattern for new interaction types.

### 4.4 Input gating

- `FirstPersonController.SetCursorLocked(bool)` sets the cursor state and
  `IsCursorLocked`.
- `UIScreen` counts visible screens that have `unlocksCursorWhileVisible`;
  the cursor is locked only when that count is 0.
- `FirstPersonController.BlockGameplayInput()` / `UnblockGameplayInput()` is
  a **counted** block that halts gameplay while keeping the cursor locked
  (used by the description box). Every block needs exactly one unblock.
- **`FirstPersonController.IsGameplayInputEnabled`** (`IsCursorLocked &&
  blockCount == 0`) is the single gate. Any new gameplay input handler must
  check it.

Input is always `InputActionReference` fields assigned in the Inspector. The
component that owns an action enables/disables it in `OnEnable`/`OnDisable`;
a component that only listens to someone else's action (e.g.
`DescriptionUIController.closeAction`) must not enable/disable it.

### 4.5 Data

All content types are `ScriptableObject`s with `[CreateAssetMenu(menuName =
"Retro FPA/…")]`, **private `[SerializeField]` fields exposed through
read-only properties**. Exceptions are the plain serializable classes of
`DialogueData` (`DialogueNode`, `DialogueChoice`, public fields, edited by
the custom inspector) and `SaveData` (public fields for `JsonUtility`).

---

## 5. Event catalogue

Static events (`public static event Action<…>`), unless marked *instance*.

| Event | Raised when | Package subscribers |
|---|---|---|
| `LevelSceneManager.LevelLoadStarted(string scene)` | `LoadLevel` begins, before unloading | `GameManager` (→ Loading), `PlayerInteractor` (clear target), `DescriptionUIController` (close) |
| `LevelSceneManager.LevelUnloaded(string prevScene)` | Previous level unloaded | — |
| `LevelSceneManager.LevelLoaded(string scene)` | New level loaded, **set active**, player placed | `GameManager` (→ Playing), `StyleManager` (re-apply style), `SaveManager` (hide collected; one-shot restore after `LoadGame`), `SceneAtmosphere`, `SceneAmbientAudio` (each only for its own scene) |
| `GameManager.GameStateChanged(GameState prev, GameState cur)` | `SetGameState` with a different state | — |
| `InventoryManager.SlotChanged(int index, InventoryEntry entryOrNull)` | Any slot content changes | `InventoryUIController` |
| `InventoryManager.EquippedItemChanged(ItemData prev, ItemData cur)` | Equip/unequip. Switching items fires twice: (A, null) then (null, B) | `InventoryUIController` |
| `DialogueManager.DialogueStarted(DialogueData)` | `StartDialogue` | `DialogueUIController` (show) |
| `DialogueManager.NodeChanged(DialogueNode)` | Each node, including the first | `DialogueUIController` |
| `DialogueManager.DialogueEnded()` | End reached or `EndDialogue` | `DialogueUIController` (hide) |
| `SettingsManager.MasterVolumeChanged(float)` | Setter (also at startup) | — (applied to `AudioListener.volume` directly) |
| `SettingsManager.MusicVolumeChanged(float)` / `SfxVolumeChanged(float)` | Setters (also at startup) | `AudioManager` |
| `SettingsManager.LookSensitivityChanged(float)` | Setter (at startup only if saved) | `FirstPersonController` |
| `SettingsManager.LocaleChanged(Locale)` | `SetLocale` | — |
| `StyleManager.StyleChanged(VisualStyleProfile)` | `ApplyProfile` (also on every level load) | — |
| `SaveManager.GameSaved()` / `GameLoaded()` | File written / restore fully applied | — |
| `PlayerInteractor.LookTargetChanged(Interactable orNull)` | Looked-at target changes | `InteractionPromptUI` |
| `PlayerInteractor.HoldProgressChanged(float 0..1)` | Every frame of a hold; 0 on end/cancel | `InteractionPromptUI` |
| `Checkable.Checked(Checkable)` | Any Checkable interacted (with a description) | `DescriptionUIController` |
| *instance* `Interactable.Interacted(GameObject)` + UnityEvent `onInteract` | `Interact()` | behaviour components ([§4.3](#43-composition-around-interactable)) |
| *instance* `Collectible.Collected(GameObject)` + UnityEvent `onCollected` | First interaction | `CollectibleItem`, `SaveableCollectible` |
| *instance* `InventorySlotUI.Clicked()` | Slot button clicked | `InventoryUIController` |

Subscription rules: scene components subscribe in `OnEnable` and
unsubscribe in `OnDisable`; singletons in `Awake` (after the duplicate
check) and `OnDestroy`. Subscribing to a static event is always safe early;
it's *reading `Instance`* that isn't.

---

## 6. Lifecycle and ordering rules

These are the rules behind most bugs fixed so far. Read the class doc
comments of `SceneAtmosphere` and `SettingsUIController` for the full stories.

### 6.1 Level load sequence

`LevelSceneManager.LoadLevel(name, spawnId)` starts a coroutine:

1. `LevelLoadStarted(name)`
2. If a level is loaded: `UnloadSceneAsync` → `LevelUnloaded(prev)`
3. `LoadSceneAsync(name, Additive)`: the level's objects `Awake`/`OnEnable`
4. **`SceneManager.SetActiveScene(level)`**, which **resets `RenderSettings`**
   (ambient, fog, skybox) to the values saved in the level scene
5. Player moved to the matching `SpawnPoint` (`ITeleportable.Teleport` if the
   root implements it, else a transform set). Falls back to the first spawn
   point in the scene.
6. `LevelLoaded(name)`

Consequences:

- **Anything that writes `RenderSettings` must do it on `LevelLoaded`**, never
  in a level object's `Awake`/`OnEnable`/`Start`, or step 4 wipes it.
- A component reacting to `LevelLoaded` must check `gameObject.scene.name ==
  sceneName`, because the event is global.
- Persistent objects (e.g. `PlayerInteractor`) holding references into the
  level must drop them on `LevelLoadStarted`.

### 6.2 Awake / OnEnable / Start

- **`Awake`**: own setup, cache own components, subscribe to static events.
  Don't read other singletons.
- **`OnEnable`/`OnDisable`**: (un)subscribe to events and input actions. Don't
  read other singletons' state either: across objects, one object's
  `OnEnable` can run before another's `Awake`.
- **`Start`**: the first point where every object in the scene has finished
  `Awake`/`OnEnable`. Read `XManager.Instance` here (see `AudioManager.Start`,
  `MainMenuUIController.Start`, `SettingsUIController.Start`).

### 6.3 Destroyed objects and `==`

Unity's `==` says a destroyed object equals `null`. When you compare a
*cached* reference that may have been destroyed (e.g. by a level unload)
against another value, use `ReferenceEquals` (see
`PlayerInteractor.UpdateLookTarget`/`CancelHold`). Use plain `== null` when
you actually mean "is it usable?".

### 6.4 Static state and Enter Play Mode options

Projects may disable domain reload, so static fields survive between Play
sessions. Any new static *state* (not events) must be reset in a
`[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`
method, as `FirstPersonController.ResetStatics` does. Static events are
cleaned up by their subscribers' unsubscriptions.

### 6.5 Overriding Unity messages in subclasses

`UIScreen` declares `protected virtual Awake/Start/OnDisable`, and `NpcBase`
`protected virtual Awake`. Subclasses must **`override` and call `base.`**.
Declaring a new `private void OnDisable()` in a subclass silently hides the
base one (this happened once, to four screens).

---

## 7. Subsystem notes

### Level loading (`LevelSceneManager`)
Only one level is tracked (`currentLevelScene`). There's **no guard against
overlapping loads**: calling `LoadLevel` while one is running starts a second
coroutine. Scene names must match Build Settings.

### Game state (`GameManager`)
`Boot → Loading → Playing ⇄ Paused`. Entering `Paused` sets `Time.timeScale =
0`; leaving it sets 1. Driven by level events and `PauseMenuUIController`.

### Player (`FirstPersonController`, `PlayerInteractor`, …)
`CharacterController` movement, yaw on the root, pitch on `cameraTransform`.
Implements `ITeleportable` (disables the controller, moves it, resets fall
velocity). `PlayerInteractor` raycasts every frame from `rayOrigin`, finds
the `Interactable` with `GetComponentInParent`, and handles hold-to-interact
itself; the input action must be a plain press.

### Inventory (`InventoryManager`)
Fixed `capacity` slots (`InventoryEntry[]`, null = empty), sized in `Awake`.
`AddItem` fills existing stacks first, then empty slots, and returns how many
fit. `RemoveItem` takes from the last slots first and auto-unequips when the
equipped item runs out. `Equip(item, wielder)` calls the item's
`EquippableBehavior.OnEquip(wielder)`.

### Dialogue (`DialogueManager`, `DialogueData`)
A flat list of nodes linked by index (-1 ends). `Advance()` only for nodes
without choices, `SelectChoice(i)` for nodes with choices. The custom
inspector (`DialogueDataEditor`) edits the object directly with
`Undo.RecordObject` plus `SetDirty`, and `PropertyField`s for the
`LocalizedString`s.

### Save (`SaveManager`, `SaveData`)
One JSON file (`persistentDataPath/save.json`) via `JsonUtility`. Items are
stored by `ItemData.ItemId` and resolved through `ItemDatabase` (a
hand-maintained list). `collectedIds` lives for the whole session and is
applied on every `LevelLoaded` (deactivates matching `SaveableId`s).
`LoadGame` replaces `collectedIds`, loads the level, and on that load's
`LevelLoaded` restores inventory (`ClearAll` + `AddItem`), equipped item and
the exact position. **`JsonUtility` tolerates missing fields** (they keep
their defaults), so adding fields to `SaveData` is backward compatible;
renaming or retyping them isn't.

### Settings (`SettingsManager`)
`PlayerPrefs` keys `Settings.*`, loaded and applied in `Awake`. Each setter
clamps, applies, saves and raises its event. Locale restore silently does
nothing if Localization isn't initialised yet.

### Style (`StyleManager`, profiles)
`VisualStyleProfile.ApplyAmbient` / `ApplyToVolumeProfile` (adds missing
`ColorAdjustments`/`Bloom`/`Tonemapping` overrides to the target Volume's
profile, which is a runtime-modified asset, so consumers give it its own).
Texture filtering walks the `Texture2D`s of every renderer's
`sharedMaterials` (or every loaded `Texture2D` with `applyToUITextures`).
`SceneAtmosphereProfile.ApplyFog` writes `RenderSettings`; `ApplySkybox`
writes `_HorizonColor/_ZenithColor/_Curve/_Exposure` on `StyleManager`'s own
material instance.

### Audio (`AudioManager`)
Three runtime-created `AudioSource`s: UI one-shots, hover (separate so a new
hover can cut the previous one without cutting a confirm), and looping music.
World sounds use `AudioSource.PlayClipAtPoint`. Music: one track at a time,
re-requesting the playing clip only updates its volume. Volume = setting ×
`maxMusicVolume`/`maxSfxVolume` (× track volume for music). Master is
`AudioListener.volume`, set by `SettingsManager`.

### UI (`UIScreen`)
Show/hide only through `CanvasGroup` (alpha, interactable, blocksRaycasts),
never `SetActive`, so screens keep receiving events while hidden (e.g. the
inventory's toggle action, the dialogue's `DialogueStarted`). Disabling a
visible screen hides it, keeping the cursor count consistent. Labels are
applied from `LocalizedString`s once, in `Awake`. Note that they don't
refresh on a locale change yet.

### Shaders
- **`RetroTwoLayer.shadergraph`**: URP Lit graph. Property reference names
  (`_L1Tex`, `_L2Color`, `_FresnelPulse`, `_Flash_Intensity`, …) are API:
  materials and `FresnelPulse.propertyName` depend on them. **Don't add a
  Sampler State node** (it overrides texture filter modes and breaks
  `StyleManager`'s texture filtering). Keep the alpha output and *Allow
  Material Override* on. Edit it in Shader Graph and check the result on both
  an opaque and a transparent material.
- **`GradientSkybox.shader`** (`Retro FPA/Gradient Skybox`): hand-written
  HLSL. Its property names are used by `SceneAtmosphereProfile`.

### Editor tooling
- Validators are `internal static class XValidator` with a `[MenuItem("Retro
  FPA/Validate/X")]` and a `public static List<string> ValidateAll()` that
  both logs warnings (with the asset as context) and returns the messages for
  the Dashboard (`RetroFpaWindow`).
- `LevelValidator` opens Build Settings scenes additively and closes the
  ones it opened (`try/finally`), skips Scene Template sources when looking
  for levels missing from Build Settings, and refuses to run in Play mode.
- Editors that move objects (`ChildTransformRandomizerEditor`,
  `ChildGroundAnchorEditor`) use `Undo.RecordObjects` then
  `PrefabUtility.RecordPrefabInstancePropertyModifications` on each child.

---

## 8. Code conventions

Match the surrounding code. Concretely:

- **Namespace** `FilloPrinci.RetroFpa` (Runtime) / `FilloPrinci.RetroFpa.Editor`
  (Editor), block-scoped. One public type per file, file named after it.
  Small helper types can share the file of the type they serve
  (`InventoryEntry`, `DialogueNode`).
- **Formatting**: 4 spaces, Allman braces, braces even on one-line `if`s.
  Expression-bodied members for one-liners.
- **Naming**: private fields `camelCase` (no underscore), properties, methods
  and events `PascalCase`, constants `PascalCase`. Event handlers are
  `HandleXxx`. Cached shader IDs are `static readonly int XxxId =
  Shader.PropertyToID("_Xxx")`.
- **Serialized fields**: `[SerializeField] private`, with a `[Tooltip]` whenever
  the meaning or units aren't obvious. Add `[Min]`/`[Range]`/`[Header]` where
  they help the Inspector. Expose read-only properties when other code needs
  the value.
- **Doc comments**: every public type gets a `/// <summary>` that explains its
  role and, above all, **why** it's built this way (ordering constraints,
  what it deliberately doesn't do). Public members get one when not obvious.
  Inline comments explain non-obvious *reasons*, not what the line does.
- **Logging**: prefix with the class name in brackets and pass `this` as
  context: `Debug.LogWarning("[Checkable] No description assigned.", this);`.
  Use `LogError` when a required collaborator is missing ("No XManager in the
  scene."), `LogWarning` for content misconfiguration. Stay silent in
  per-frame paths (see `FootstepAudio`).
- **Null handling**: `Instance?.Method()` for optional managers; explicit check
  + `LogError` + `return` when the call is the component's whole purpose.
  Null `AudioClip`s are no-ops.
- **Required components**: `[RequireComponent]` and `GetComponent` in `Awake`.
- **`Reset()`** for sensible defaults when a component is added (e.g.
  `SceneChangeTrigger` sets `isTrigger`, `SaveableId` generates an id).
- **No LINQ or allocations in per-frame Runtime code** (Editor code may use
  LINQ freely). Reuse buffers (`ChildGroundAnchor.hits`).
- Use modern Unity APIs: `FindObjectsByType(FindObjectsSortMode.None)`,
  `TryGetComponent`, target-typed `new()`.

---

## 9. Recipes

### 9.1 Add a manager
1. `Runtime/Core/XManager.cs`: `public class XManager :
   PersistentSingleton<XManager>`, with the `Awake`/`OnDestroy` shape from
   [§4.1](#41-persistent-singletons).
2. Expose state through properties, changes through static events.
3. Add it to `LevelValidator.RecommendedManagers` (or `RequiredManagers`)
   with a one-line "what breaks without it".
4. Document it in README (*What's here*), the User Guide's persistent-scene
   table, and CHANGELOG. Mention that the template's persistent scene/Scene
   Template needs it added.

### 9.2 Add an interaction behaviour
Copy the shape of `DialogueTrigger`: `[RequireComponent(typeof(Interactable))]`,
cache in `Awake`, subscribe to `Interacted` in `OnEnable`, unsubscribe in
`OnDisable`, keep the handler short and delegate to a manager. If it needs
to persist across saves, add a bridge component like `SaveableCollectible`
rather than coupling it to `SaveManager`.

### 9.3 Add a UI screen
Derive from `UIScreen`. Override `Awake`/`Start`/`OnDisable` with `protected
override` and call `base.`. Show/hide only with `Show()`/`Hide()`. Subscribe
to manager events in `OnEnable`. Apply `LocalizedString` labels in `Awake`.
Decide `unlocksCursorWhileVisible`'s intended value and document it in the
class summary if it must be off.

### 9.4 Add an equippable behaviour
Subclass `EquippableBehavior` (a `ScriptableObject`) with a `CreateAssetMenu`
under `Retro FPA/Equippable/…`. It is shared by every item using it, so
**keep per-wielder state off the asset** (store it on the wielder or in a
component).

### 9.5 Add a gameplay input
`InputActionReference` field; enable plus `performed +=` in `OnEnable`,
reverse in `OnDisable`; return early unless
`FirstPersonController.IsGameplayInputEnabled`. Document the action in the
User Guide's input table.

### 9.6 Add a setting
`SettingsManager`: key constant `Settings.X`, property, `SetX` (clamp → apply →
`PlayerPrefs.SetX` + `Save` → event), load it in `LoadAndApplyAll`.
`SettingsUIController`: control field, row label + `LocalizedString`,
listener in `OnEnable`/`OnDisable`, populate in `PopulateFromSettings` with
`SetValueWithoutNotify`.

### 9.7 Save more state
Add public fields to `SaveData` (new fields default when reading old
files). Fill them in `SaveGame`, restore them in `RestoreAfterLoad` (which
runs after the level loaded and the player was placed). For per-object world
state, key it by `SaveableId`.

### 9.8 Add a validator check
Add it to the relevant `ValidateX` method, using `Report(messages, …)` with a
message that says *where*, *what* and *what breaks*. Read private serialized
fields with `new SerializedObject(c).FindProperty("fieldName")`. For a new
validator, add a menu item and a Dashboard button.

### 9.9 Add a sample
Put it under `Samples~/<Name>/` **with metas** (the existing samples commit
theirs, so imported prefabs keep stable GUIDs and their internal references
resolve; author the sample in a consuming project, then copy it in together
with its `.meta` files), declare it in
`package.json` `samples`, and describe it in `Samples~/README.md`.

### 9.10 Add a file or folder
Create its `.meta` (via Unity, or by hand with a fresh GUID) and commit both.
Text-ish files (`.md`, `.txt`, `.json`) use `TextScriptImporter`; folders use
`folderAsset: yes` + `DefaultImporter`. Copy an existing sibling meta and
replace the GUID.

---

## 10. Compatibility: what breaks consumers

Consumers are real game projects with scenes and prefabs built on this
package. Treat these as **breaking** (needing a CHANGELOG note, and in 0.x a
minor version bump):

- Renaming/removing a serialized field without `[FormerlySerializedAs]`, or
  changing its type. Also fields read by name in the Editor assembly:
  `"profile"` (`SceneAtmosphere`), `"targetSceneName"`/`"targetSpawnPointId"`
  (both scene-change triggers), `"seed"` (`ChildTransformRandomizer`),
  `"fogColor"`/`"skyboxHorizonColor"`/`"skyboxZenithColor"`
  (`SceneAtmosphereProfile`). Grep `FindProperty(` before renaming.
- Renaming a class, or changing its namespace or assembly. Scenes reference a
  `MonoBehaviour`/`ScriptableObject` through its `.cs` file's GUID, which
  survives a namespace change, but the class name must still match the file
  name, and project code using the type breaks. Changing a `.meta` GUID
  breaks every reference to that file.
- Changing a `CreateAssetMenu` path (breaks muscle memory, not data).
- Removing/renaming public API, events, or changing their signatures.
- Renaming shader properties or keyword reference names.
- Changing `SaveData` field names, `ItemId` semantics, the save file name, or
  `PlayerPrefs` keys (players lose saves/settings).
- Behaviour changes that require project changes (example: 0.2.0
  hold-to-interact required removing the Hold interaction from projects'
  Interact action). State the required action in **bold** in the CHANGELOG.

---

## 11. Releasing

Only when the maintainer asks for a release.

1. Move the CHANGELOG's *Unreleased* entries under `## [x.y.z] - YYYY-MM-DD`
   (keep an empty `## [Unreleased]`). Sections: *Added*, *Changed*, *Fixed*
   (also *Removed*, *Deprecated* when needed).
2. Bump `"version"` in `package.json`; update the install example tag in
   `README.md` (and the User Guide).
3. Commit `Release x.y.z` with a one-paragraph summary body.
4. Tag `vx.y.z` as an annotated tag (`git tag -a vx.y.z -m "x.y.z"`, like
   the previous ones) and push
   the commit **and** the tag.
5. Consumers update `"…unity_retrofpa-core.git#vx.y.z"` in their manifest
   (and `packages-lock.json` re-resolves).

SemVer: while in 0.x, a minor bump (0.2 → 0.3) may contain breaking changes
(list them), and a patch bump (0.2.0 → 0.2.1) is fixes only.

---

## 12. Change checklist

Before considering a change done:

- [ ] No game-specific content added.
- [ ] Every new file/folder has a committed `.meta`; no GUID changed.
- [ ] Serialized fields: no unguarded rename; `FindProperty("…")` strings
      updated.
- [ ] Ordering: no `Instance` reads in `Awake`/`OnEnable`; `RenderSettings`
      only on `LevelLoaded`; events unsubscribed symmetrically.
- [ ] New gameplay input checks `IsGameplayInputEnabled`.
- [ ] Player-facing text is localized.
- [ ] Compiles in a consuming project, no new Console errors or warnings;
      relevant flow exercised in Play mode from the persistent scene;
      validators pass (and are updated if the rules changed).
- [ ] CHANGELOG *Unreleased* updated (with any required consumer action in
      bold); README / User Guide / this guide updated if setup or behaviour
      changed.
- [ ] Commit message follows [§3.2](#32-commit-messages). Not released or
      tagged unless asked.

---

## 13. Known gaps and caveats

Not implemented (see the README's *Known gaps*): combat/health/damage
(`Melee/RangedEquippableBehavior.PerformAction` and the shader's flash are
hooks only), HUD beyond the interaction prompt, world-state saving beyond
inventory and collectibles, jump/crouch/sprint, NPC AI, multiple save slots.

Caveats worth knowing when working nearby:

- `LevelSceneManager.LoadLevel` doesn't guard against a load already in
  progress.
- UI labels from `LocalizedString` are applied once in `Awake`; changing the
  locale at runtime doesn't refresh menus already built (dialogue lines,
  prompts and descriptions are fetched when shown, so those are fine).
- `ItemDatabase` is maintained by hand; a missing entry only shows up as a
  warning when loading a save.
- `DialogueDataEditor`'s *Remove Node* doesn't re-target links to shifted
  indices.
- `SettingsManager`'s doc comments on `MusicVolumeChanged`/`SfxVolumeChanged`
  still say no audio system exists; `AudioManager` does listen to them now.
