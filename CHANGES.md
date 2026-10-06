# Changes in version 2.0.0

Version 2.0 is a complete review of MonoGame.GameManager. The library was refactored around small interfaces and well-known design patterns, the bugs found during the review were fixed, and the systems that were missing to make 2D games were added. The review itself, with the analysis of the original code, is in [docs/PROJECT_REVIEW.md](docs/PROJECT_REVIEW.md).

| | 1.0.2 | 2.0.0 |
|---|---|---|
| Source files | 69 | 157 |
| Lines of code | about 4,550 | about 16,600 |
| Package dependencies | 9 (Microsoft.Extensions, Serilog, MonoGame, ...) | 1 (Newtonsoft.Json) |
| Target frameworks | netstandard2.0, Xamarin.iOS10, MonoAndroid10.0, uap10.0 | net8.0, net10.0 |
| MonoGame | 3.8.0.1641 | 3.8.5.1 |

Contents:

- [Improvements](#improvements)
- [New features](#new-features)
- [Bug fixes](#bug-fixes)
- [Removals](#removals)
- [Migration notes](#migration-notes)

## Improvements

### Build and package

- `net8.0` and `net10.0` targets built with the standard SDK, compiled against MonoGame 3.8.5.1 (MonoGame 3.8.1 and later ship no `netstandard2.0` assembly). The package works on every MonoGame platform for games on .NET 8, .NET 9 and .NET 10.
- The samples run on .NET 10 with MonoGame 3.8.5.1. The Android demo is an SDK-style `net10.0-android` project. The UWP demo was removed, because MonoGame no longer supports UWP. The content builder (`dotnet-mgcb`) is a local .NET tool: run `dotnet tool restore` once before building the samples. Ping-Pong and Snake use the Comic Sans MS font, because Hobo Std is not installed with Windows and the content builder needs the font on the build machine.
- MonoGame is referenced for compilation only (`PrivateAssets="all"`), so the package no longer forces `MonoGame.Framework.DesktopGL` on Android, iOS or UWP games.
- The only dependency is `Newtonsoft.Json`. The Generic Host of `Microsoft.Extensions`, Serilog, `System.Numerics.Vectors`, `MonoGame.ShaderEffects` and `Xamarin.Build.TypeRedirector` were removed.
- XML documentation is generated and shipped in the package; the package includes the README and the logo.

### Services

- `ServiceProvider` is a static facade over a small `ServiceRegistry` (Registry pattern) instead of the Microsoft Generic Host. Every service has an interface: `IClock`, `IRandom`, `IScheduler`, `IGameWindowManager`, `IContentLoader`, `IAssetCache`, `IInputManager`, `IAudioManager`, `ISaveGameService` and `IScreenNavigator`.
- Services can be registered before the screen manager starts (they replace the defaults) and replaced at any time with `ServiceProvider.Replace`.
- Control identifiers come from a thread-safe counter, so controls can be created without a running game (for example in tests).

### Screens

- A screen owns a root panel, a scheduler and a content loader, all released when it closes. Controls, animations and timers of a closed screen no longer keep running.
- Screen changes are queued and applied at the end of the frame, so they are safe inside click handlers and animation callbacks, and two transitions can no longer overlap.
- Screens can be stacked (`PushScreen`, `PopScreen`) for pause menus and dialogs, with `IsModal`, `DrawWhenCovered` and `UpdateWhenCovered`.
- New `Screen.Update(GameTime)`, `OnCovered` and `OnUncovered` hooks.
- `ScreenManagerSettings` replaces the values that were hard-coded: virtual resolution, window size, full screen, resizing, orientations, content folder, mouse visibility, pixel art, blend state, colors, default transition, title and input while the window is not active.
- Transitions are symmetric (`TransitionOut` then `TransitionIn`) and run on the global scheduler.
- The screen manager disposes its sprite batch, render target, screens and shared textures when the game closes.

### Controls

- New `Opacity` and `NestedOpacity` on every control: fading a container fades its children, without changing their colors.
- New `IsVisible`, `IsEnabled`, `Name` (with `FindByName`), `BlocksMouseEvents` and `AcceptedMouseButtons`.
- `IControl` is split into `ILayoutElement`, `IRenderable` and `IInputTarget` (Interface Segregation), and still exposes everything.
- `Dispose` removes the control from its parent, clears every event handler and raises `Disposed`; containers dispose their children.
- Containers clip with the scissor rectangle instead of a render target of the size of the screen. A render target of the size of the container is used only for rotated containers or when `ClipMode` asks for it.
- Children are sorted only when they change, and the containers no longer allocate lists every frame.
- The hit test considers the rotation of the control.
- `Label`, `MultiLineLabel`, `Image`, `Button`, `Panel`, `RectangleControl`, `ScrollViewer` and `SpriteAnimation` were reworked; see [Bug fixes](#bug-fixes) and [New features](#new-features).
- Layout and drawing helpers for custom controls: drawing in the local space of a control (`DrawLocalTexture`, `DrawLocalRectangle`), conversion of positions (`ToLocalPosition`, `LocalToParentPosition`) and a stack of sprite batch states (`SpriteBatchState`, `ControlManager.PushState`).

### Pointer input

- Events go to the top-most control first and stop at the first control that handles them. A control that handles clicks also owns the press, so the controls below it are not clicked at the same time.
- The children of a clipped container only receive input inside the container.
- Pointer capture: a control can receive the moves and the release even when the pointer leaves it (`ControlMouseEventArgs.CapturePointer`).
- Touch input shows the hover and pressed states and clears them when the finger leaves the screen; a cancelled touch releases the pressed controls.
- Every mouse button and the mouse wheel are supported.

### Animations and timers

- Animations have easing functions, a delay, a repeat count, ping-pong and reverse modes, `Pause`/`Resume`, and the callbacks `AddOnStarted`, `AddOnAnimationEnd` (every cycle), `AddOnCompleted` and `AddOnStopped`.
- Animations and timers run on the scheduler of their screen and stop when their control (or the owner given to `SetParent`) is disposed.
- The start value of an animation is read from the control when it is played, unless it was set explicitly.
- `DelayTime` can be paused and resumed, repeat a number of times and report its progress.

### Content

- `ContentLoader` (`IContentLoader`) caches sprite animation files, opens files relative to the content folder, and loads sounds and songs.
- Each screen has its own `ContentManager`, unloaded when the screen closes, because MonoGame 3.8.0 cannot unload a single asset.
- `SpriteAnimationPipelineReader` reports the asset name in its errors, and its cycles inherit every common property of the file.

### Window

- `GameWindowManager` has `LayoutChanged` and `ScreenSizeChanged` events, `SetWindowSize`, `SetFullScreen` and `ToggleFullScreen`. Margins are expressed in virtual pixels and the layout survives a minimized window.

## New features

### Screens
- `ScreenManagerSettings`, screen stack (`PushScreen`/`PopScreen`), `SlideTransition`, `Screen.Update`, `Screen.Content`, `Screen.Scheduler`, `Screen.Input`, `Screen.Audio`, `Screen.RegisterDisposable`.

### Controls
- `ToggleButton`, `Checkbox`, `Slider`, `ProgressBar` (with `AnimateTo`), `TextBox` (caret, password mode, character filter), `Tooltip`.
- `StackPanel` and `GridPanel` layouts, with the pure `StackLayout` and `GridLayout` helpers and the `Thickness`, `Orientation`, `ChildAlignment` and `FillDirection` types.
- `NineSliceImage`, `TiledImage`, `TileMap` (culling, solid tiles and collision queries), `CircleControl`, `LineControl`, `FpsCounter`, `ParticleEmitter` and `CameraPanel`.
- `Button`: solid-color buttons, disabled state, text, border, `SetTextures` and `SetBackgroundColors`.
- `Panel` and `RectangleControl`: background color and border.
- `ScrollViewer`: mouse wheel, `ScrollTo`, `ScrollIntoView`, `ScrollToTop`, `ScrollToBottom`, zoom around a point, inertia settings and a `ScrollViewerStyle` for the bars.
- `MultiLineLabel`: `SetText`, `SetTextBoxWidth`, `SetLineSpacing` and the `Lines` after wrapping, with the pure `TextWrapper`.
- `ZIndexLayers` (`Default`, `Overlay`, `Transition`).

### Animations
- `Easing`: linear plus quad, cubic, quart, quint, sine, expo, circ, back, elastic and bounce, each in, out and in-out (`EasingFunction`, `EasingType`).
- `Tween<T>` with interpolators for float, double, int, `Vector2`, `Point`, `Color`, `Rectangle` and angles.
- `AnimationSequence` and `AnimationGroup` (Composite pattern), with delays and callbacks.
- `MoveAnimation`, `OpacityAnimation`, `ColorAnimation` and `BlinkAnimation`.

### Timers
- `Scheduler` (`IScheduler`): `Delay`, `Every` (with a repeat count), `NextFrame`, `EveryFrame`, `TimeScale`, `Pause` and `Resume`, safe to modify during an update. `ScheduledAction` can be cancelled, paused and resumed.
- `IClock` with a global `TimeScale` and the frame count; `ServiceProvider.GlobalScheduler` for timers that survive screen changes.

### Input
- `KeyboardInputListener` (keys pressed and released in each frame, typed text), `GamePadInputListener` (four players, sticks, triggers, dead zone, vibration, connection events), and touch gestures.
- `InputMap` and `InputManager`: named actions and axes bound to keys, gamepad buttons, mouse buttons and gamepad axes (`IsActionPressed`, `IsActionDown`, `IsActionReleased`, `GetAxis`, `GetVector`). Every listener can be updated with simulated states.

### Game systems
- `Camera2D`: position, zoom limits, rotation, world bounds, smooth follow, shake, view matrices and world/screen conversions.
- `Collision`: points, rectangles, circles, polygons, segments and rays, with minimum translation vectors and swept rectangles. `RectangleF`, `Circle` and `LineSegment` shapes.
- `MathUtils` and `Vector2Extensions`: interpolation, angles, wrapping, smoothing and vector helpers.
- `RandomGenerator` (`IRandom`) with seeds, ranges, chances, points in shapes, weighted picks and shuffles.
- `Mover` and `ControlMover`: velocity, acceleration, gravity, frame-rate independent drag and maximum speed.
- `StateMachine<TState>` with callbacks or `IState` classes and conditional transitions.
- `ObjectPool<T>` with `IPoolable`.
- `ParticleSystem` (no garbage while emitting) with `ParticleSettings`.
- `AudioManager` (`IAudioManager`): pooled sound instances, music with fades, master, sound and music volumes, mute.
- `JsonSaveGameService` (`ISaveGameService`): save slots as JSON files with safe writes, and JSON converters for `Vector2`, `Point`, `Rectangle` and `Color` (`GameManagerJson.CreateSettings`).
- `Primitives` (lines, rectangles, circles, polygons with a `SpriteBatch`) and `TextureFactory` (circles and rounded rectangles).

## Bug fixes

### Lifecycle and memory
- Animations and timers of a closed screen kept running forever (for example the Snake menu added rotation animations every round).
- `SetParent` on an animation or a timer restarted it on its previous parent.
- An animation with a duration of zero produced NaN values, and a stack overflow when it was looping.
- `MemoryManager.CleanMemory` never released anything, and the disposables it collected (one render target of the size of the screen per clipped container) were never disposed.
- The screen manager disposed nothing when the game closed.
- `ContentLoaderManager` was registered as transient, so its texture cache was always empty, and sprite animation files were parsed again on every load.

### Input
- Click-through: a click reached the controls below the clicked one, and a control with only a click handler let the press go to the controls below.
- Children of clipped containers and the hidden content of a scroll viewer received input.
- Touch left controls in the hover state and never showed the pressed state of buttons.
- `IsTouchInput` was lost when the events reached the controls, and the time of mouse events was the duration of the frame instead of the game time.
- Input was processed while the window was not active.

### Controls
- `Button`: changing a texture after the creation had no effect until the pointer moved over it.
- `Image`: `SetSourceRectangle` did not update the size; the transparent-pixel hit test ignored the nested scale, the source rectangle and flips, threw in debug builds, and read the texture from the GPU on every test.
- `Label` and `MultiLineLabel` crashed with a null text. `MultiLineLabel` crashed without a parent, ignored the opacity, recreated all its labels on every change, lost empty lines and measured long words incorrectly.
- `RectangleControl` stored its origin as a rate instead of pixels, so its hit test was shifted.
- `ScrollViewer`: presses anywhere on the screen started a scroll, the zoom setter skipped the limits, the inertia used per-frame values and had an unreachable branch, and the pinch zoom could divide by zero.
- `SpriteAnimation`: crashed when drawn before `Play`, raised its end event twice, looped forever with a frame duration of zero, and `ChangeSpriteAnimationInfoOnAnimationEnd` was not implemented.
- `SpriteAnimationCycleBuilder` created empty cycles from frames alone, changed the frames it received, and a `.sa` file without `frameDuration` hung the game.
- `AddChild` did not remove the control from its previous parent (it was drawn twice), and `RemoveChild` changed controls that were not children.
- Rotated controls were hit-tested as if they were not rotated.

### Math, window and data
- `RandomGenerator.Random(float, float)` could return values below the minimum.
- `GameWindowManager`: asymmetric margins placed the screen incorrectly, a minimized window divided by zero, and `OnClientSizeChanged` was never invoked.
- `RectangleConverter` failed on null values.
- Typos in public names (`AddOnUpddateDestinationRectangle`, `RoationInDegreeStart`, `SetNeedToShortChildren`, `sizeWidht`).

### Samples
- Ping-Pong and Snake did not compile against the library; Snake loaded `"Food"` instead of `"food"`; the demo projects missed the `MonoGame.ShaderEffects` package.
- Demos: the Top and Right margin options changed the wrong margin; the transition preview leaked a rectangle per cycle.

## Removals

- The `OS` folder (`OSType` and `AppInfo` for each platform), which nothing used.
- `ControlCounterService` and `ServiceProvider.ControlCounterService` (identifiers come from a counter in `Control`).
- `ServiceProvider.SetScreenManager` (the screen manager registers itself).
- `ContentAsset` and `ContentAssetState` (the asset states of the memory manager).
- The `Microsoft.Extensions.*`, Serilog, `System.Numerics.Vectors`, `MonoGame.ShaderEffects` and `Xamarin.Build.TypeRedirector` package references, and the `MSBuild.Sdk.Extras` multi-targeting.
- `MonoGame.GameManager.csproj.bak` and the demo `Content.mgcb.bak`.
- The hidden panel of `ControlMouseEventHandler`, the full-screen panel of `ScrollViewer` (replaced by pointer capture) and the six empty handlers behind `BlockMouseEvents` (replaced by `BlocksMouseEvents`).
- The `System.Text.Json` reference of the UWP demo, a version with a known vulnerability that was only needed by the Microsoft.Extensions host.

## Migration notes

### Renamed members

The old names still compile, marked `[Obsolete]` with a message that gives the new name.

| 1.x | 2.0 |
|---|---|
| `ServiceProvider.ContentLoaderManager`, `ContentLoaderManager` | `ServiceProvider.ContentLoader`, `ContentLoader` (`IContentLoader`) |
| `ServiceProvider.MemoryManager`, `MemoryManager` | `ServiceProvider.AssetCache`, `AssetCache` (`IAssetCache`) |
| `CleanMemoryType`, `CleanMemory()`, `SetAllAssetsAsFixed()` | No longer needed (no effect): assets are released with the `ContentManager` that loaded them |
| `AddAssetToDispose`, `TryGetAsset`, `AddAsset` | `TrackDisposable` (or `Screen.RegisterDisposable`), `TryGet`, `Add` |
| `Screen.ContentLoader` | `Screen.Content` (released with the screen) or `ServiceProvider.ContentLoader` (kept for the whole game) |
| `GetContentFileStream` | `OpenStream` |
| `EaseAnimation` | `MoveAnimation` (with `SetEasing`) |
| `FadeAnimation` | `OpacityAnimation` (`SetOpacityStart`, `SetOpacityEnd`) |
| `ResetAnimation()` (animations) | `Reset()` |
| `RotationAnimation.RoationInDegreeStart` | `RotationInDegreeStart` |
| `DelayTime.SetIsLoop` | `SetIsLooping` |
| `SpriteAnimation.ActualCycle`, `ActualFrame` | `CurrentCycle`, `CurrentFrame` |
| `AddOnUpddateDestinationRectangle` | `AddOnUpdateDestinationRectangle` |
| `SetNeedToShortChildren` | `SetNeedToSortChildren` |
| `CalculatedNestedScaleIfDirty` | `CalculateNestedScaleIfDirty` |
| `ScrollViewer.RemoveOnZoomChange` | `RemoveOnZoomChanged` |
| `MathExtension.CapValue` | `MathUtils.Clamp` |
| `Intersection.IntersectsWithPoint` | `Collision.RectangleContainsPoint` |
| `GameWindowManager.OnClientSizeChanged`, `ClientSizeChanged()` | `LayoutChanged` event, `UpdateLayout()` |
| `FadeTransition.CreateTransitionIn`, `CreateTransitionOut` | `TransitionOut`, `TransitionIn` |

### Changes that need attention

- **MonoGame package.** The library references MonoGame for compilation only. A game must reference its own MonoGame platform package (every MonoGame game already does).
- **`ITransition`.** Custom transitions implement `TransitionOut(Action onComplete)`, which hides the current screen, and `TransitionIn(Action onComplete)`, which reveals the new one.
- **Interfaces.** `IControl` and `IContainer` gained members and use the corrected names (`AddOnUpdateDestinationRectangle`, `SetNeedToSortChildren`), so classes that implement them directly must be updated. Classes that derive from `Control<T>` or `ContainerAbstract<T>` are not affected.
- **Constructors.** `ControlManager(GraphicsDevice, Point)` and `ControlMouseEventHandler(IControl root)` replace the 1.x constructors; `ControlMouseEventArgs` has a constructor with the touch flag, the button and the position. These classes are usually created by the library.
- **Screen changes** are applied at the end of the frame: code that runs after `ChangeScreen` in the same frame still sees the current screen.
- **Dispose.** Disposing a control clears its event handlers, and disposing a container disposes its children. Use `RemoveFromScreen` for a control that is added again later.
- **Opacity.** `FadeAnimation` still multiplies the `Color` of the control, as in 1.x. `OpacityAnimation` animates the new `Opacity` property, so a fade no longer conflicts with hover colors. Code that resets a faded control with `SetColor` should also set `Opacity` back to 1.
- **Custom animations.** `OnUpdateAnimation(float progress)` receives the eased progress, already reversed for reverse and ping-pong cycles. Override `CaptureStartValues` to read the start value from the control.
- **Drawing order.** Siblings are drawn by `ZIndex` and then by creation order (as in 1.x), not by the order of `AddChild`; `StackPanel` and `GridPanel` use their own order.
- **Window margins** are in virtual pixels.
- **`RectangleControl.Origin`** is in pixels like the other controls. `SetOriginRate` works as before; `SetOrigin(new Vector2(0.5f))` must become `SetOriginRate(0.5f)`.
- **`ScrollViewer`** hides its overflow by default. Its public fields are properties with the same names, and `AddOnScrollPositionChanged` and `AddOnZoomChanged` return the viewer.
- **`SpriteAnimationFrame.Duration`** defaults to 0, which means "use the duration of the cycle".
- **Mouse events.** `MouseEventArgs.Time` is the total game time for the mouse, as it already was for touch.
- **Input** is not processed while the window is not active, unless `ScreenManagerSettings.ProcessInputWhenInactive` is true.
- **`PositionCalculations`** parameters were renamed (`sizeWidht` became `sizeWidth`), which only matters for calls with named arguments.
