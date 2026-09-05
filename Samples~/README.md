# Samples

Ready-to-import prefabs and example content for this package (`NpcBase`,
`WorldItemBase`, `SceneChangeTrigger`, `SpawnPoint`, ...).

This folder is intentionally suffixed with `~` so Unity hides it from the
Project window and does not compile its contents by default — the standard
convention for [UPM package samples](https://docs.unity3d.com/Manual/cus-samples.html).
Samples are declared in `package.json` under `"samples"` and surfaced to
consumers via the Package Manager window's "Samples" tab, where they can be
imported into a project's `Assets` folder on demand.

## Base Prefabs

`BasePrefabs/` contains:

- `Prefabs/NpcBase.prefab` — an Animator driven by the shared
  `NpcBaseController` (Idle/Walk/Hit/Death placeholder states), with an
  `AnimatorOverrideController` slot for remapping onto a character's real
  clips.
- `Prefabs/WorldItemBase_Physical.prefab` — an empty GameObject with a
  `BoxCollider` + `Rigidbody`, no mesh (concrete items add their own).
- `Prefabs/WorldItemBase_Pickupable.prefab` — a Prefab Variant of the above,
  adding `Interactable` + `Collectible`.
- `Animations/NpcBase/` — the shared `NpcBaseController` and its four
  placeholder clips (`Idle`/`Walk`/`Hit`/`Death`).

Import it via Package Manager → Retro FPA Core → Samples tab → **Import**.
It lands under `Assets/Samples/Retro FPA Core/<version>/Base Prefabs/` in
the consuming project.
