# Retro FPA Core — User Guide

For people **building a game in the Unity Editor** with this package: level
designers, artists, writers, and programmers wiring up content. You don't
need to read any C# to follow it. If you want to change the package itself,
read the [Developer Guide](developer-guide.md) instead.

Package version covered: **0.2.x** (Unity 6000.3, URP).

---

## Contents

1. [Install](#1-install)
2. [The big picture](#2-the-big-picture)
3. [Setting up a project](#3-setting-up-a-project)
4. [Building a level](#4-building-a-level)
5. [Moving between levels](#5-moving-between-levels)
6. [Interactive objects](#6-interactive-objects)
7. [Items and inventory](#7-items-and-inventory)
8. [Dialogue](#8-dialogue)
9. [Visual style, fog and sky](#9-visual-style-fog-and-sky)
10. [The RetroTwoLayer shader](#10-the-retrotwolayer-shader)
11. [Audio](#11-audio)
12. [Saving and loading](#12-saving-and-loading)
13. [Player settings](#13-player-settings)
14. [Level dressing tools](#14-level-dressing-tools)
15. [Editor tools and validators](#15-editor-tools-and-validators)
16. [Troubleshooting](#16-troubleshooting)
17. [What's not there yet](#17-whats-not-there-yet)

---

## 1. Install

**Requirements:** Unity **6000.3** (17f1 or later) with URP. The package pulls
in URP, the Input System, Localization and uGUI/TextMesh Pro as dependencies.
Your project must actually *use* them: URP as the render pipeline, the new
Input System as the active input handling, and a Localization Settings asset
with at least one locale.

Add the package to `Packages/manifest.json`, pinned to a release tag:

```json
{
  "dependencies": {
    "com.filloprinci.retrofpa": "https://github.com/FilloPrinci/unity_retrofpa-core.git#v0.2.0"
  }
}
```

To upgrade, change the tag and let Unity re-resolve. Read
[`CHANGELOG.md`](../CHANGELOG.md) first: some releases need a change in your
project (for example 0.2.0 needs you to remove the *Hold* interaction from
your Interact action, see [§3.3](#33-input-actions)).

**Samples.** *Window → Package Manager → Retro FPA Core → Samples → Base
Prefabs → Import* copies `NpcBase` and the `WorldItemBase` prefabs into
`Assets/Samples/Retro FPA Core/<version>/`. They're starting points to make
Prefab Variants from.

**Fastest start:** the
[`retrofpa-project-template`](https://github.com/FilloPrinci/unity_retrofpa-project-template)
repository is a complete, working project. Its two Scene Templates (*File →
New Scene*) give you a pre-wired persistent scene and an empty level. If you
can, start from there instead of wiring everything by hand.

> **Never edit files under `Library/PackageCache/`.** That's a read-only copy
> Unity can overwrite at any time; your changes will be lost. To change the
> package, see the Developer Guide.

---

## 2. The big picture

A game built on this package has **one persistent scene** and **many level
scenes**.

```
┌──────────────────────── Persistent scene (always loaded) ───────────────────────┐
│  Managers: GameBootstrapper, GameManager, LevelSceneManager, InventoryManager,  │
│            DialogueManager, SettingsManager, StyleManager, SaveManager,         │
│            AudioManager                                                         │
│  Player (FirstPersonController + camera)                                        │
│  UI Canvas (main menu, pause, settings, dialogue, inventory, prompt, ...)        │
│  Global Volume (post-processing), EventSystem                                   │
└─────────────────────────────────────────────────────────────────────────────────┘
        ▲ one level at a time is loaded additively on top, then unloaded
┌──────── Level scene ────────┐
│  Geometry, lights           │
│  SpawnPoint(s)              │
│  SceneAtmosphere (fog/sky)  │
│  SceneAmbientAudio (music)  │
│  Interactables, pickups,    │
│  NPCs, triggers ...         │
└─────────────────────────────┘
```

Rules that follow from this:

- **Managers, the Player and the UI live only in the persistent scene.** A
  level never contains them. If a level does contain a manager, the duplicate
  is destroyed at runtime.
- **A level contains only its own content**, plus a `SpawnPoint` where the
  player arrives.
- **Content is data.** Items, dialogue, the visual style, fog/sky and global
  sounds are `ScriptableObject` assets you create with *Assets → Create →
  Retro FPA → …* and assign in the Inspector.
- **Global vs. per-level** is a split that comes back everywhere:

  | Global (whole game, set once on a manager) | Per level (a component in the level scene) |
  |---|---|
  | `VisualStyleProfile`: ambient light, color grading, bloom, tonemapping, texture filtering | `SceneAtmosphereProfile`: fog and skybox colors |
  | `AudioProfile`: UI sounds, menu music, default footstep/pickup/interact sounds | `SceneAmbientAudio`: the level's looping music/ambience |

---

## 3. Setting up a project

If you started from the project template's *Retro FPA Persistent* Scene
Template, all of this is already done. Use this section as a reference for
what each piece needs.

### 3.1 The persistent scene

Create a scene (e.g. `Persistent.unity`) and put it **first (index 0) in
Build Settings**, since it's the one Unity loads at startup. Add these
components (one GameObject per manager, or all on one, either works):

| Component | Required? | Inspector setup |
|---|---|---|
| `LevelSceneManager` | **Required** | *Persistent Player Root*: the Player's root transform. |
| `GameBootstrapper` | **Required** | *Starting Level Scene Name* (as in Build Settings), optional *Starting Spawn Point Id*. Turn **off** *Auto Start On Awake* if a main menu should gate the first load. |
| `GameManager` | **Required** | Nothing. Drives pause (`Time.timeScale`). |
| `InventoryManager` | Recommended | *Capacity*: number of slots (default 12). |
| `DialogueManager` | Recommended | Nothing. |
| `SettingsManager` | Recommended | Nothing. |
| `StyleManager` | Recommended | *Target Volume*: the global URP Volume. *Initial Profile*: a `VisualStyleProfile`. *Skybox Material Template*: a material using **Retro FPA/Gradient Skybox**. |
| `SaveManager` | Recommended | *Item Database*: your `ItemDatabase` asset. |
| `AudioManager` | Recommended | *Initial Profile*: an `AudioProfile`. *Max Music/SFX Volume*: the real volume at the settings sliders' maximum. |

Also add a **Global Volume** (its profile is written to at runtime by
`StyleManager`; give it its own profile asset) and an **EventSystem** with
the Input System UI module.

Every system tolerates a missing optional manager (it just does nothing), but
the [level validator](#15-editor-tools-and-validators) will warn you about it.

### 3.2 The Player

One GameObject in the persistent scene, tagged **`Player`** (the tag
`SceneChangeTrigger` looks for):

```
Player   [Tag: Player]
│  CharacterController
│  FirstPersonController      Move / Look actions, Camera Transform, speeds, look sensitivity
│  PlayerInteractor           Interact action, Ray Origin = Camera, Interact Range, Interact Mask
│  PlayerEquipmentController  Attack action
│  FootstepAudio              Step Interval, Min Speed, Ground Mask
└── Camera                    (pitch is applied here)
```

Assign it to `LevelSceneManager`'s *Persistent Player Root*. On every level
load it is moved to the level's `SpawnPoint`.

### 3.3 Input actions

The components take `InputActionReference`s, so you can use any Input
Actions asset. You need:

| Action | Type | Used by |
|---|---|---|
| Move | Value / Vector2 | `FirstPersonController` |
| Look | Value / Vector2 (mouse delta) | `FirstPersonController` |
| Interact | Button, **no interactions** | `PlayerInteractor`, `DescriptionUIController` (close) |
| Attack | Button | `PlayerEquipmentController` |
| ToggleInventory | Button | `InventoryUIController` |
| Cancel (UI map, e.g. Escape) | Button | `PauseMenuUIController` |

> **Remove any *Hold* interaction from Interact.** Hold-to-interact is now a
> per-object option on `Interactable` (see [§6.1](#61-interactable)). With a
> Hold interaction on the action, *every* interaction would need holding.

### 3.4 The UI

Every screen is a `UIScreen`: it needs a **`CanvasGroup`** and is shown/hidden
by fading that group, never by disabling its GameObject. Each screen has two
common options:

- **Hidden On Start** (default on). Turn it off for the main menu.
- **Unlocks Cursor While Visible** (default on). While at least one such
  screen is open, the cursor is free and the player can't move or look.

| Screen | What to assign |
|---|---|
| `MainMenuUIController` | *Bootstrapper*, the four buttons, *Settings Screen*, localized labels. Set *Hidden On Start* **off**. |
| `PauseMenuUIController` | *Toggle Action* (UI/Cancel), Resume/Save/Settings/Quit buttons, *Settings Screen*. |
| `SettingsUIController` | Sliders (master/music/SFX/look sensitivity), locale & resolution dropdowns (TMP), VSync/fullscreen toggles, close button, row labels. |
| `DialogueUIController` | Speaker/body TMP texts, Continue button, *Choices Container*, *Choice Button Prefab* (a Button with a TMP label, kept inactive). |
| `InventoryUIController` | *Toggle Action*, *Slots Container* (e.g. a Grid Layout Group), *Slot Prefab* (`InventorySlotUI`), detail texts, equip button + label, close button, 3D preview (see [§7.4](#74-the-equipped-item-preview)). |
| `DescriptionUIController` | *Body Text*, *Close Action* = the same Interact action. Set *Unlocks Cursor While Visible* **off**. |

Two UI helpers that are *not* screens:

- **`InteractionPromptUI`**: the "[E] Open" prompt. *Prompt Root* (the object
  shown/hidden), *Prompt Text*, *Default Prompt* (localized), *Key Label*.
  Optional: *Key Text* (key label on its own, e.g. on a key icon), *Hold
  Indicator* and *Hold Fill* (an `Image` with *Image Type = Filled*) for
  hold-to-interact objects.
- **`UIButtonSound`**: add to any `Button` to play the global hover/confirm
  sounds.

All visible text comes from **Localization** (`LocalizedString` fields).
Leave a label empty to keep whatever text the TMP component already has.

---

## 4. Building a level

1. *File → New Scene → Retro FPA Level (Base)* (from the project template), or
   an empty scene.
2. Make sure it contains:
   - at least one **`SpawnPoint`**. Its *Id* (default `Default`) lets triggers
     pick it. The cyan gizmo shows position and facing.
   - exactly one **`SceneAtmosphere`** with a `SceneAtmosphereProfile`.
     **Use a separate profile per level** (the Scene Template clones one for
     you), otherwise editing one level's fog changes the other.
   - at most one **`SceneAmbientAudio`**, with the level's looping track and
     its *Volume*.
3. **Do not** add managers, the Player, cameras for gameplay, or UI.
4. Add the scene to **Build Settings**. Levels are loaded by name, and only
   enabled Build Settings scenes can be loaded.
5. Run *Retro FPA → Validate → Levels*.

**Testing a level:** always press Play with the **persistent scene** open
(the level loads through `GameBootstrapper` or the main menu). Pressing Play
with only a level open gives you no player, no managers and no UI.

**Lighting bake:** bake a level with that level open on its own, or as the
*active* scene. If the persistent scene is active, the lightmaps end up in the
wrong folder.

---

## 5. Moving between levels

Both triggers take a *Target Scene Name* (exactly as in Build Settings) and an
optional *Target Spawn Point Id*. If the id is empty or doesn't match, the
first `SpawnPoint` in the target scene is used.

- **`SceneChangeTrigger`**: walk into a volume. Needs a Collider with *Is
  Trigger* (set automatically when you add the component). Fires only for
  colliders tagged *Player Tag* (default `Player`).
- **`InteractableSceneChangeTrigger`**: press Interact on an object (door,
  ladder, exit sign). Needs an `Interactable` (added automatically).

Typical two-way door: in level A put a door with
`InteractableSceneChangeTrigger → B / "FromA"`, and in B put a `SpawnPoint`
with id `FromA` just in front of B's matching door.

What happens on a level change: the old level is unloaded, the new one is
loaded, the player is teleported to the spawn point, then the level's
atmosphere and ambient music are applied. The game is in the *Loading* state
meanwhile.

---

## 6. Interactive objects

Everything interactive is built from **`Interactable`** plus one or more
behaviour components next to it. The player's `PlayerInteractor` raycasts
from the camera; any collider on the object **or its children** works, as
long as it's on a layer in *Interact Mask* and within *Interact Range*.

### 6.1 Interactable

| Field | Meaning |
|---|---|
| Prompt Text | Localized text for the prompt ("Open", "Talk"). Empty means the prompt's default. |
| Interact Sound | Played on every interaction. Empty means `AudioProfile`'s default interact sound. |
| Require Hold / Hold Duration | The button must be held this many seconds (looking away or releasing cancels). |
| On Interact (UnityEvent) | Hook anything here without code: play an Animator trigger, toggle a light, enable an object. Receives the player GameObject. |

### 6.2 Recipes

| You want… | Components |
|---|---|
| A door/switch/anything scripted | `Interactable` + use *On Interact*, or a project script that listens to it |
| An object you can examine ("A dusty photo.") | `Interactable` + **`Checkable`** (*Description*, localized). Opens a modal text box; Interact again closes it. |
| A pickup that goes into the inventory | `Interactable` + **`Collectible`** + **`CollectibleItem`** (*Item*, *Quantity*) |
| …that stays picked up after save/revisit | + **`SaveableCollectible`** (adds a **`SaveableId`**) |
| A pickup that only fires an event | `Interactable` + `Collectible` + use its *On Collected* event |
| An NPC you can talk to | `NpcBase` (from the sample) + `Interactable` + **`DialogueTrigger`** (*Dialogue*) |
| A door to another level | `Interactable` + **`InteractableSceneChangeTrigger`** |
| A spinning key/coin | + **`Rotator`** (*Degrees Per Second*) |
| A glowing pulse on a pickup | + **`FresnelPulse`** (material must use RetroTwoLayer, see [§10](#10-the-retrotwolayer-shader)) |

`Collectible` deactivates its GameObject when picked up and plays its own
*Pickup Sound* (or the profile default). The `Interactable`'s interact sound
plays too. If that's one sound too many, leave the pickup's *Interact Sound*
empty and the `AudioProfile`'s *Default Interact Sound* empty as well (then
only the pickup sound plays; other interactables need their own clip).

The **Base Prefabs** sample has `WorldItemBase_Pickupable` (BoxCollider +
Rigidbody + Interactable + Collectible): make a Prefab Variant, add your mesh,
`CollectibleItem` and `SaveableCollectible`.

### 6.3 NPCs

`NpcBase` needs an `Animator` whose controller is the shared
`NpcBaseController` (from the sample: Idle/Walk/Hit/Death placeholder states).
For each character, create an **Animator Override Controller** that replaces
the placeholder clips with the real ones, and assign it to *Animator
Override*. There is no NPC AI or movement yet.

---

## 7. Items and inventory

### 7.1 ItemData

*Assets → Create → Retro FPA → Item Data*.

| Field | Meaning |
|---|---|
| Item Id | **Stable, unique id** used by save files (e.g. `rusty_key`). Never change it after shipping a build: old saves would lose the item. |
| Display Name / Description | Localized, shown in the inventory detail panel. |
| Icon | Sprite for the inventory slot. |
| World Prefab | The pickup prefab for this item. |
| Equipped Model Prefab | Model shown in the inventory's 3D preview while equipped. |
| Max Stack Size | How many fit in one slot. |
| Equippable Behavior | Empty means not equippable (keys, notes). |

### 7.2 Equippable behaviours

*Assets → Create → Retro FPA → Equippable → Melee / Ranged / Held Item*. One
asset can be shared by many items. The Attack input calls the equipped
item's behaviour. **Melee and Ranged do nothing yet** (no combat system);
*Held Item* is for tools/keys you just hold.

### 7.3 ItemDatabase

*Assets → Create → Retro FPA → Item Database*, assigned to `SaveManager`.
**Every item must be listed in it**, or that item silently disappears from
the inventory when a save is loaded (a warning is logged). Keep it up to date
by hand when you add items.

### 7.4 The equipped-item preview

The inventory screen can show the equipped item spinning in 3D:

1. Create a dedicated layer (e.g. `ItemPreview`).
2. Add a **Preview Camera** that renders only that layer (and exclude that
   layer from the player camera), with a **Preview Anchor** child placed in
   front of it, on the same layer.
3. Assign the camera, the anchor and a **RawImage** (*Preview Image*) to
   `InventoryUIController`. A 256×256 RenderTexture is created at runtime.

The model is instantiated under the anchor and moved to the anchor's layer
automatically.

---

## 8. Dialogue

*Assets → Create → Retro FPA → Dialogue Data*. A dialogue is a list of
**nodes**; node 0 is where it starts.

- Each node has a *Speaker* (plain text, not localized) and a localized
  *Text*.
- A node with **no choices** continues to its *Next Node* when the player
  presses Continue.
- A node **with choices** shows one button per choice; each choice has its
  own *Next Node*.
- *End Dialogue* as a target ends the conversation.

The custom Inspector replaces raw indices with dropdowns ("Node 2: Alice").
Buttons: *Add Node*, *Add Choice*, *Remove Node*, *Remove Choice*. Careful:
**removing a node shifts the indices of the nodes after it**, and the links
pointing at them aren't updated, so run *Retro FPA → Validate → Dialogues*
afterwards (it reports out-of-range links and nodes unreachable from node 0).

Start a dialogue with a `DialogueTrigger` on an `Interactable`.

---

## 9. Visual style, fog and sky

### 9.1 VisualStyleProfile (global)

*Assets → Create → Retro FPA → Visual Style Profile*, assigned to
`StyleManager`'s *Initial Profile*. Re-applied after every level load.

- **Ambient Light**: mode and color.
- **Color Adjustments, Bloom, Tonemapping**: written into the global Volume's
  profile (overrides are added if missing).
- **Texture Filtering**: *Point* for crisp PS1-style pixels,
  *Bilinear/Trilinear* for N64-style smoothing. It's applied to the textures
  used by renderers in the loaded scenes. *Apply To UI Textures* extends it
  to every texture in memory (icons, font atlases).

> Texture filtering only takes effect on materials whose shader samples with
> the texture's own sampler. RetroTwoLayer does. A Shader Graph with an
> explicit *Sampler State* node ignores it.

### 9.2 SceneAtmosphereProfile (per level)

*Assets → Create → Retro FPA → Scene Atmosphere Profile*, assigned to the
level's `SceneAtmosphere`.

- **Fog**: on/off, mode, color, density (exponential) or start/end (linear).
- **Skybox**: horizon and zenith colors, curve, exposure, drawn by the
  *Retro FPA/Gradient Skybox* shader via `StyleManager`'s skybox template.
  Turn *Skybox Enabled* off to keep whatever skybox the level scene has.

Keep **Skybox Horizon ≈ Fog Color** so distant geometry fades into the sky.
The Inspector has a *Sync Skybox Horizon ← Fog Color* button.

Fog and sky are applied **when the level finishes loading at runtime**. The
Scene view while editing shows the scene's own Lighting settings, not the
profile.

---

## 10. The RetroTwoLayer shader

`Runtime/Shaders/RetroTwoLayer.shadergraph`, a lit URP Shader Graph for
retro surfaces: two texture layers blended together, plus fresnel rim and
hit flash.

| Property | Meaning |
|---|---|
| L1Tex / L1Color / L1Tiling / L1Offset / L1Scroll | Layer 1 texture, tint, tiling, offset, scroll speed (UV animation). |
| L2Tex / L2Color / L2Tiling / L2Offset / L2Scroll | Layer 2, same. Leave it white for a single-layer material. |
| Blend Mode (keyword) | How the layers combine where both are opaque: *Alpha Over*, *Multiply*, *Additive*, *Subtract*, *Divide*. With *Multiply* the alpha is multiplied too, so either layer can cut the other out. |
| Emission_texture / Emission_color | Emission. |
| Smoothness / Metallicness | Lighting response. |
| Fresnel Color / Fresnel Power / Fresnel Insensity | Always-on rim light. |
| Fresnel Pulse (`_FresnelPulse`) | Multiplier for the rim, driven by `FresnelPulse` per object. |
| Flash Color / Flash Intensity | Hit flash (for a future combat system). |

Materials are **Opaque** by default. For see-through areas set the material's
*Surface Type* to *Transparent*, or enable *Alpha Clipping* (the graph has
*Allow Material Override* on).

Per-object effects (pulse, flash) are driven through a
`MaterialPropertyBlock`, so they never modify the shared material asset.

---

## 11. Audio

The package ships **no audio files**. Everything plays through
`AudioManager`, and every field can be left empty (silence, no errors).

- **`AudioProfile`** (*Create → Retro FPA → Audio Profile*), on
  `AudioManager`'s *Initial Profile*: UI hover/confirm, main menu music,
  default footstep/pickup/interact sounds.
- **Per object**: *Interact Sound* on `Interactable`, *Pickup Sound* on
  `Collectible`, **`SurfaceAudio`** on a floor object for its own footsteps
  (found by `FootstepAudio`'s downward raycast, on the hit collider or a
  parent).
- **Per level**: `SceneAmbientAudio`. Only one music track plays at a time;
  the newest request wins (the main menu music is replaced by the first
  level's track).
- **Volumes**: the Settings sliders scale music (main menu + level tracks) and
  SFX (UI + world sounds); Master scales everything. `AudioManager`'s *Max
  Music/SFX Volume* set how loud "100%" really is.

---

## 12. Saving and loading

One save slot, a JSON file at `Application.persistentDataPath/save.json`.

**Saved:** current level, player position and yaw, every inventory slot, the
equipped item, and the ids of collected `SaveableCollectible`s.
**Not saved:** anything else (opened doors, light switches, dialogue
progress, NPC state). A project needs its own code for that today.

- *Save* is on the pause menu; *Continue* on the main menu (disabled while no
  save file exists).
- **`SaveableId`**: gets a fresh GUID when added. **If you duplicate or
  copy-paste a saveable object, regenerate the copy's id** (component context
  menu → *Generate New Id*), or picking up one hides both. The level
  validator detects duplicates across all levels.
- Collected objects stay gone **for the whole session** even without saving
  (walking back into a level keeps them hidden).
- To reset your save while testing, delete `save.json` from
  `persistentDataPath` (on Linux `~/.config/unity3d/<Company>/<Product>/`, on
  Windows `%USERPROFILE%\AppData\LocalLow\<Company>\<Product>\`).

---

## 13. Player settings

`SettingsManager` stores the player's options in **PlayerPrefs** (keys
`Settings.MasterVolume`, `Settings.MusicVolume`, `Settings.SfxVolume`,
`Settings.LookSensitivity`, `Settings.Locale`, `Settings.VSync`,
`Settings.Fullscreen`, `Settings.ResolutionWidth/Height`) and applies them at
startup. Look sensitivity keeps the `FirstPersonController`'s Inspector value
until the player changes it once. To reset: *Edit → Clear All PlayerPrefs*.

---

## 14. Level dressing tools

Both work on the **direct children** of the object they're on, and are meant
to be applied **in the Editor** with their Inspector buttons (undoable, saved
with the scene, compatible with static/lightmapped objects). Each also has a
runtime option, which won't work on static children.

- **`ChildTransformRandomizer`**: random rotation (min/max per axis, default
  0–360° on Y) and uniform scale (default 0.8–1.2). Buttons: *Randomize (New
  Seed)* and *Apply Current Seed*. Same seed means same result, and values are
  absolute (re-applying doesn't accumulate).
- **`ChildGroundAnchor`**: drops each child straight down onto the first
  collider below it (ignoring triggers and the children's own colliders).
  *Ground Mask*, *Max Distance*, *Start Height* (look from a bit above, so a
  child slightly buried is lifted out), *Vertical Offset* (e.g. negative to
  sink trunks). Button: *Anchor Children To Ground*. The ground needs a
  collider.

Typical use: put 30 tree prefabs under an empty `Trees` object, add both
components, randomize, then anchor.

---

## 15. Editor tools and validators

- **Window → Retro FPA → Dashboard**: buttons for all validators, results
  listed in the window.
- **Retro FPA → Validate → Items**: empty/duplicate *Item Id*, missing name,
  icon, world prefab, or equipped model on an equippable item.
- **Retro FPA → Validate → Dialogues**: empty dialogues, out-of-range links,
  unreachable nodes.
- **Retro FPA → Validate → Levels**: scans the enabled Build Settings scenes:
  - persistent scene: missing managers or Player;
  - each level: no/duplicate `SpawnPoint` ids, missing or multiple
    `SceneAtmosphere`, missing profile, more than one `SceneAmbientAudio`,
    managers or a Player inside a level, empty or duplicate `SaveableId`s,
    `Collectible`s without `SaveableCollectible` (fine if they should
    respawn), scene-change triggers pointing at a missing scene or spawn id;
  - scenes with a `SpawnPoint` that aren't in Build Settings.

  Scenes you don't have open are opened and closed for the scan. Not available
  in Play mode.

Run the validators before every commit that touches content.

---

## 16. Troubleshooting

| Symptom | Likely cause / fix |
|---|---|
| Every interaction needs a long press | The Interact action has a *Hold* interaction. Remove it ([§3.3](#33-input-actions)). |
| Pressing Play shows nothing / no player | You played a level scene alone. Play from the persistent scene. |
| "No LevelSceneManager in the scene" errors | Same as above, or the manager is missing from the persistent scene. |
| Level doesn't load, scene-not-found error | Scene not in Build Settings, disabled there, or the name is misspelled (validator catches it). |
| Player arrives at the wrong place | *Target Spawn Point Id* doesn't match any `SpawnPoint` *Id* in the target level, so the first one was used. |
| Fog/sky look wrong in the Scene view | Normal: profiles apply at runtime. Check in Play mode. |
| Two levels change fog together | They share the same `SceneAtmosphereProfile` asset. Give each its own. |
| Texture filtering setting has no effect | The material's shader uses an explicit Sampler State, or the texture isn't on a renderer in a loaded scene. |
| No sound at all | No `AudioManager`, no `AudioProfile`, or volumes at zero in Settings/PlayerPrefs. |
| Hover sound missing on buttons | `UIButtonSound` not on that Button, or the profile's UI Hover Sound is empty. |
| Picking up one object hides another | Both have the same `SaveableId`: regenerate one. |
| Item missing after loading a save | Not listed in `ItemDatabase`, or its *Item Id* changed. |
| Player can't move or look, cursor hidden | Something blocks gameplay input: a project script called `FirstPersonController.BlockGameplayInput()` without a matching `UnblockGameplayInput()`. |
| Cursor visible while playing, player frozen | A `UIScreen` with *Unlocks Cursor While Visible* is still shown, e.g. one with *Hidden On Start* off by mistake. |
| Collected item reappears when revisiting | It has no `SaveableCollectible`. |
| "Duplicate … destroyed" / manager missing after load | A manager was placed in a level scene. Move it to the persistent scene. |

---

## 17. What's not there yet

- **Combat**: no health, damage or death. Melee/Ranged behaviours are hooks
  only; the shader's flash is unused.
- **HUD**: only the interaction prompt.
- **World state saving**: doors, switches and other state beyond inventory
  and collected pickups aren't saved.
- **Player movement**: walk only. No jump, crouch, sprint or head-bob (the
  template's input asset has those actions, but nothing uses them).
- **NPC behaviour**: animation slot only, no AI.
- **Multiple save slots**: one slot.
