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

## [0.1.0] - 2026-09-05

### Added

- Initial package skeleton: `package.json`, Runtime/Editor Assembly
  Definitions, `Samples~` folder.
