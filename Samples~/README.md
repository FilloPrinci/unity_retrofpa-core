# Samples

Ready-to-import prefabs and example content for this package (`NpcBase`,
`WorldItemBase`, `SceneChangeTrigger`, `SpawnPoint`, ...).

This folder is intentionally suffixed with `~` so Unity hides it from the
Project window and does not compile its contents by default — the standard
convention for [UPM package samples](https://docs.unity3d.com/Manual/cus-samples.html).
Samples are declared in `package.json` under `"samples"` and surfaced to
consumers via the Package Manager window's "Samples" tab, where they can be
imported into a project's `Assets` folder on demand.

No samples are defined yet; this will be filled in once the base prefabs
exist (see the project's development roadmap).
