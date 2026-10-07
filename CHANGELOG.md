# Changelog

## [2.1.0] (2026-10-06)

A complete particle system, with a playground and scenes in the demos.

### Improvements
- `CameraPanel` no longer culls controls without a size (such as particle emitters), so what they draw outside of their bounds stays visible.
- `TextureFactory` generates glows, rings, stars and diamonds; `ColorExtensions.Multiply` tints a color with another.

### New features
- `ParticleSettings` grew from 18 to more than 60 options: emitter shapes (`EmitterShape`), emission duration, loop, delay and prewarm, `ParticleBurst`s, emission per distance and inherited velocity, world or local `SimulationSpace`, palettes, random scale, rotation and flips, `ParticleCurve` and `ParticleGradient` over the lifetime with easings, turbulence, attraction and vortex forces, `Floor` and `Bounds` with `ParticleBoundsMode`, sprite-sheet `Frames` with `ParticleFrameMode`, procedural `ParticleShape`s, a `BlendState` per system, velocity alignment and stretch, and `ParticleSubEmitter`s on death and as trails.
- `ParticleSystem` is an `IPlayable` (`Play`, `Pause`, `Resume`, `Stop`, `Reset`, `Restart`, `Prewarm`) with `Time`, `IsComplete` and `Completed`, replaceable `Settings`, `EmitterVelocity` and helpers for custom renderers (`GetDrawScale`, `GetRotation`, `GetTexture`, `GetSourceRectangle`, `GetEffects`). Still no garbage while emitting.
- `ParticleEmitter` plays, pauses and restarts like an animation, bursts at any position, removes itself when its effect ends (`RemoveWhenCompleted`, `AddOnCompleted`, `ParticleEmitter.Spawn` and `SpawnBurst`), draws the procedural shapes with additive blending and follows the control in local space.
- `ParticlePresets`: Fire, Smoke, Explosion, Sparks, Rain, Snow, Confetti, Fireworks, Magic, Fountain, Bubbles, Fireflies, Vortex and Stars. `ParticleResources` holds the shape textures and the additive blend state.
- Demos: a Particles tile that opens a playground where every option is editable live on the presets, and a scenes screen (campfire, fireworks, rainy day, confetti cannon, magic cursor, fountain, portal and spaceships). A Slider tile with every option of the slider and three examples; the demo pages use sliders for their numeric options.

## [2.0.0] (2026-10-05)

A complete review of the library. See [CHANGES.md](CHANGES.md) for the details and the migration notes, and [docs/PROJECT_REVIEW.md](docs/PROJECT_REVIEW.md) for the review.

### Improvements
- Targets `net8.0` and `net10.0`, compiled against MonoGame 3.8.5.1; MonoGame is a compile-time reference and Newtonsoft.Json is the only dependency (the Microsoft.Extensions host and Serilog were removed).
- Samples on .NET 10 and MonoGame 3.8.5.1: desktop samples on `net10.0`, the Android demo converted to an SDK-style `net10.0-android` project, and the content builder restored as a local .NET tool.
- Services behind interfaces (`ServiceRegistry` + `ServiceProvider` facade), replaceable by the game.
- Screens own their controls, scheduler and content, all released when they close; screen changes are applied at the end of the frame; screen stack for overlays; `ScreenManagerSettings`.
- Pointer input without click-through, clipped input, pointer capture, every mouse button and the wheel, touch hover and pressed states.
- Scissor clipping instead of screen-sized render targets; `Opacity` on every control.
- Reworked `Button`, `Image`, `Label`, `MultiLineLabel`, `Panel`, `RectangleControl`, `ScrollViewer` and `SpriteAnimation`.

### New features
- Controls: `ToggleButton`, `Checkbox`, `Slider`, `ProgressBar`, `TextBox`, `Tooltip`, `StackPanel`, `GridPanel`, `NineSliceImage`, `TiledImage`, `TileMap`, `CircleControl`, `LineControl`, `FpsCounter`, `ParticleEmitter`, `CameraPanel`.
- Animations: easing functions, `Tween`, `AnimationSequence`, `AnimationGroup`, `MoveAnimation`, `OpacityAnimation`, `ColorAnimation`, `BlinkAnimation`.
- Timers: `Scheduler` per screen and global, `IClock` with time scale.
- Input: keyboard, gamepads, touch gestures, `InputMap` with named actions and axes.
- Game systems: `Camera2D`, `Collision` and shapes, `MathUtils`, `RandomGenerator`, `Mover`, `StateMachine`, `ObjectPool`, `ParticleSystem`, `AudioManager`, `JsonSaveGameService`, `Primitives`, `TextureFactory`, `SlideTransition`.

### Bug fixes
- More than 40 bugs, among them: animations and timers running after their screen closed, render targets and assets never released, click-through, input in clipped areas, sprite animations that crashed or looped forever, and samples that did not compile.

### Removals
- Unused `OS` folder, `ControlCounterService`, the memory manager asset states, and the `.bak` files.
- The UWP demo, because MonoGame no longer supports UWP.

## [1.0.2] (2022-05-27)

- Touch pinch zoom for ScrollViewer
- Possibility to scale the button background

## [1.0.1] (2022-01-26)

- Add Scale option to Panel and Button controls
- Add hide overflow option to Panel and Button controls
- New Control: ScrollViewer

## [1.0.0] (2022-01-09)

### Controls
- Image
- Button
- Label
- Multi-line Labels
- Panel
- Sprite Animation
- Rectangle

### Animations
- Fade Animation
- Ease Animation
- Scale Animation
- Rotation Animation

### Screen
- Screen Transition
- Screen Manager

### Samples - Games
Ping-Pong
Snake
Tic-Tac-Toe
