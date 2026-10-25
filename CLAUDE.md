# CLAUDE.md

This repository is **Retro FPA Core** (`com.filloprinci.retrofpa`), a Unity 6
(URP) UPM package: the reusable engine for retro first-person
horror/adventure games. It is consumed by other Unity projects through a git
URL pinned to a tag. It cannot be opened in Unity on its own.

**Before changing anything, read [`Documentation~/developer-guide.md`](Documentation~/developer-guide.md)**
(architecture, event catalogue, ordering rules, conventions, recipes,
release process). [`Documentation~/user-guide.md`](Documentation~/user-guide.md)
describes the Editor-side workflow your change must keep working.

## Hard rules

- No game-specific content in this package.
- Every new file/folder outside `*~` folders needs a committed `.meta` with a
  fresh unique GUID (consumers install it immutable from git). Never change
  an existing GUID.
- Never rename a serialized field without `[FormerlySerializedAs]`, and grep
  `FindProperty("` in `Editor/` for string references to it.
- Managers communicate via static C# events. Read another singleton's
  `Instance` only from `Start()` or an event handler, never `Awake`/`OnEnable`.
- Anything that writes `RenderSettings` (fog, skybox, ambient) does it on
  `LevelSceneManager.LevelLoaded`, filtered to its own scene.
- Levels load additively only. No shared-`Material` modification at runtime
  (`MaterialPropertyBlock` or an owned instance). No auto-regeneration in
  `OnValidate`.
- Gameplay input handlers check `FirstPersonController.IsGameplayInputEnabled`.
- Player-facing text is `LocalizedString`.
- Subclasses of `UIScreen`/`NpcBase` `override` the Unity messages and call
  `base.`.

## Workflow

- Match the surrounding code style (developer guide §8): Allman braces,
  `[SerializeField] private` + `[Tooltip]`, `/// <summary>` explaining *why*,
  `[ClassName]`-prefixed logs with `this` as context.
- Update `CHANGELOG.md` under `## [Unreleased]` for every user-visible
  change (required consumer actions in **bold**), and README/guides when
  setup or behaviour changes.
- Commits: English, imperative, optionally `Component: …` prefix
  (e.g. `Fix PlayerInteractor missing Interactable on a parent object`).
- Verification needs a consuming project that references this repo with
  `"file:…"` in its `Packages/manifest.json`: compile, Play from the
  persistent scene, run *Window → Retro FPA → Dashboard* validators. If you
  can't drive the Unity Editor, say exactly what you couldn't verify.
- Line endings are LF. Files showing as modified only due to CRLF should be
  restored, not committed (`git diff --ignore-cr-at-eol`).
- Do not bump the version, tag, release or push unless explicitly asked
  (procedure: developer guide §11).
