# Retro FPA Core

A Unity UPM package providing the reusable "engine" for retro low-poly
(PS1 / N64 / GameCube-era) first-person horror/adventure games: decoupled
manager systems, data-driven `ScriptableObject` content, ready-to-place
prefabs, a custom retro-style rendering pipeline, and editor tooling for
authoring dialogue and items.

This package is the Unity counterpart of an existing Godot 4.6.3 "Retro FPA"
template. It is not a literal code port — it reproduces the same
architectural philosophy (decoupled systems via events, data-driven content,
levels assembled from prefabs) using idiomatic Unity patterns instead of
Godot ones. See [`unity_retrofpa_kickoff_brief.md`](https://github.com/FilloPrinci/unity_retrofpa-project-template/blob/main/unity_retrofpa_kickoff_brief.md)
in the sibling [`retrofpa-project-template`](https://github.com/FilloPrinci/unity_retrofpa-project-template)
repository for the full design rationale.

This package contains **no game-specific content** — it is meant to be
consumed by one or more Unity projects, such as `retrofpa-project-template`.

## Package layout

```
retrofpa-core/
  package.json
  Runtime/    Manager singletons (GameManager, SceneManager, SettingsManager, ...),
              components (Interactable, Grabbable, Collectible, Rotator,
              FresnelPulse, DialogueTrigger, SceneChangeTrigger, SpawnPoint, ...),
              ScriptableObjects (ItemData, DialogueData, VisualStyleProfile,
              EquippableBehavior + melee/ranged subclasses, ...), shaders
  Editor/     EditorWindow docks, custom PropertyDrawers, scaffolding wizards,
              content validators
  Samples~/   Base prefabs ready to import into a consuming project
              (NpcBase, WorldItemBase, SceneChangeTrigger, SpawnPoint, ...)
```

`Runtime` and `Editor` are separate Assembly Definitions
(`FilloPrinci.RetroFpa.Runtime` and `FilloPrinci.RetroFpa.Editor`, the latter
referencing the former and restricted to the `Editor` platform), so editor-only
code never ships in player builds.

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
- **Data-driven content.** Game content (items, dialogue, visual styles) is
  authored as `ScriptableObject` assets, not hardcoded.
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

## License

[MIT](LICENSE)
