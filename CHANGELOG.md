# Changelog

## [2.0.0] (2026-10-05)

A complete review of the library. See [CHANGES.md](CHANGES.md) for the details and the migration notes, and [docs/PROJECT_REVIEW.md](docs/PROJECT_REVIEW.md) for the review.

### Improvements
- Single `netstandard2.0` target; MonoGame is a compile-time reference and Newtonsoft.Json is the only dependency (the Microsoft.Extensions host and Serilog were removed).
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
