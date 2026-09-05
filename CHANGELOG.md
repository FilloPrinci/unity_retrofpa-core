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

### Changed

- `FresnelPulse` now pulses once (min → max → min) and then holds at
  `minValue` for a configurable `pulsePause` (seconds) before repeating,
  instead of oscillating continuously.

## [0.1.0] - 2026-09-05

### Added

- Initial package skeleton: `package.json`, Runtime/Editor Assembly
  Definitions, `Samples~` folder.
