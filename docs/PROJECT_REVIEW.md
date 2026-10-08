# MonoGame.GameManager: project review

| | |
|---|---|
| Date | 2026-10-05 |
| Reviewed version | 1.0.2 (commit `7cfe242`) |
| Result | 2.0.0 (summary of the changes in [CHANGES.md](../CHANGES.md)) |

This document records the review of the library: how it was built, what worked well, the problems found (design and bugs), and how version 2.0 addresses them. Line numbers refer to version 1.0.2.

## 1. Scope and method

The review covered the library (69 source files, about 4,550 lines), the demo application (a shared project with Windows, UWP and Android heads) and the three sample games (Ping-Pong, Snake and Tic-Tac-Toe).

The method was:

1. Read every source file of the library and of the samples.
2. Build the library with the .NET 8 SDK for `netstandard2.0`, and compile every sample against it.
3. Check which MonoGame 3.8.0.1641 APIs exist, from the metadata of the NuGet package.
4. Exercise the logic that does not need a graphics device with a throwaway console program (see [section 7](#7-verification)).

The review environment has no display: drawing, audio, real touch input and window behavior were compiled and reasoned about, but not run.

## 2. The project as found

### 2.1 Features

Version 1.0.2 offered a fluent control tree (`Image`, `Button`, `Label`, `MultiLineLabel`, `Panel`, `RectangleControl`, `SpriteAnimation`, `ScrollViewer`), four linear animations (ease, fade, rotation, scale), a timer (`DelayTime`), a screen manager with a virtual resolution and a fade transition, a content cache and a JSON format for sprite animations (`.sa`).

### 2.2 Architecture

- **Services.** `ServiceProvider` was a static class over the Microsoft Generic Host (`Microsoft.Extensions.Hosting`), with about 40 call sites in the library. Services were concrete classes without interfaces, and `ContentLoaderManager` was registered as transient (a new instance, with an empty cache, on every access).
- **Control tree.** `Control<T>` uses the curiously recurring template pattern so fluent methods return the concrete type. `ScalableControlAbstract<T>` adds the scale and `ContainerAbstract<T>` the children. A single root panel, owned by `ControlManager`, held the controls of the current screen.
- **Update loop.** Animations and timers registered themselves as update handlers of the root panel. Nothing removed them when the screen changed.
- **Screens.** `ScreenManager` (a MonoGame `Game`) changed screens synchronously from inside click handlers and animation callbacks; changing the screen cleared the children of the root panel, but not its update handlers.
- **Memory.** `MemoryManager` tracked assets with states that nothing ever changed, so `CleanMemory` released nothing. Containers with `HideOverflow` created a render target of the size of the screen and gave it to the memory manager, which never disposed it.
- **Input.** `MouseInputListener` and `TouchInputListener` handled the left button only. `ControlMouseEventHandler` wrapped the root in a hidden panel and walked the tree recursively.

### 2.3 Strengths

- The fluent API is pleasant to use and reads well (`new Label(...).SetAnchor(...).AddToScreen()`).
- Anchors, origins and nested scales make layouts resolution-independent.
- The `.sa` format and `SpriteAnimationCycleBuilder` make sprite animations easy to declare.
- The demo application covers every control and animation with live options, which made the review much easier.

### 2.4 Weaknesses

- Lifecycle leaks: animations, timers, render targets and disposables outlived their screens.
- Input bugs: click-through, input in clipped areas, touch states, left button only, no keyboard or gamepad.
- Missing systems for 2D games: input mapping, easing and tweens, a camera, collision, physics helpers, state machines, pooling, particles, audio, save games, layout containers and common input controls.
- Heavy dependencies for a game library (the Generic Host, Serilog) and a multi-targeted build that forced `MonoGame.Framework.DesktopGL` on every consumer of the `netstandard2.0` package.
- Two sample games did not compile against the library.

## 3. SOLID analysis

| Principle | Finding in 1.0.2 | Change in 2.0 |
|---|---|---|
| Single responsibility | `ScrollViewer.cs` (372 lines) mixed layout, input, inertia, bars and zoom. `Control.cs` (552 lines) mixed layout, input events, drawing and disposal. `ScreenManager.cs` mixed the game loop, navigation, rendering and window setup. | The scroll viewer delegates the bars (`ScrollViewerBar`, `ScrollViewerStyle`) and the pinch (`ScrollViewerPinchZoom`). Navigation is a queue of requests. Window layout lives in `GameWindowManager`, settings in `ScreenManagerSettings`, timing in `IClock` and `IScheduler`. Pure helpers (`TextWrapper`, `StackLayout`, `GridLayout`, `Collision`, `MathUtils`) hold the logic that controls reuse. |
| Open/closed | Services had no interfaces. Orientation, content folder and mouse visibility were hard-coded in `ScreenManager.cs:28-34`. Transitions had an asymmetric interface (`CreateTransitionIn(Action)` and `CreateTransitionOut()`). | Every service is an interface that can be replaced. `ScreenManagerSettings` holds the configuration. `ITransition` is symmetric, and `SlideTransition` was added without touching the screen manager. Buttons and animations expose template methods (`GetTexture`, `OnUpdateAnimation`, `CaptureStartValues`). |
| Liskov substitution | `RectangleControl` stored `Origin` as a rate from 0 to 1 while every other control stores pixels (`RectangleControl.cs:14-18`), so the hit test of `Control` was wrong for it. `FadeAnimation` overwrote `Color`, which `SetMouseEventsColor` also writes. | Every control stores the origin in pixels. Fades animate the new `Opacity` property and no longer conflict with colors. |
| Interface segregation | `IControl` had more than 60 members. | `IControl` is composed of `ILayoutElement`, `IRenderable` and `IInputTarget`; code that only needs one aspect depends on that interface. |
| Dependency inversion | About 40 static calls to `ServiceProvider`; the identifier of every control came from `ServiceProvider.ControlCounterService` (`Control.cs:14`), so a control could not exist without a running game. | Controls get their identifiers from a counter and can be created without a game. Services are resolved through interfaces, and classes that need one accept it in their constructor (`InputManager`, `AudioManager`, `Camera2D`, `ParticleSystem`, `TextBox`, `JsonSaveGameService`...). |

## 4. Bugs found

Every bug below is fixed in 2.0.0.

### 4.1 Lifecycle and memory

| # | Location (1.0.2) | Problem |
|---|---|---|
| 1 | `Screens/ScreenManager.cs:104-114` | Changing the screen cleared the children of the root panel but not its update handlers: looping animations and timers ran forever (the Snake menu added a rotation animation every round, `MainMenuScreen.cs:49-51`). |
| 2 | `Screens/ScreenManager.cs:55-56` | The sprite batch and the render target were never disposed; nothing was cleaned up when the game closed. |
| 3 | `Screens/ScreenManager.cs:43-49` | `ChangeScreen` ran synchronously inside click and animation callbacks, and two transitions could overlap. |
| 4 | `Controls/Abstracts/ContainerAbstract.cs:170-189` | Every container with `HideOverflow` allocated a render target and a sprite batch of the size of the screen, handed to a list that was never emptied (`Managers/MemoryManager.cs:16,23-26`). |
| 5 | `Managers/MemoryManager.cs:59-74` | `CleanMemory` only released assets in a state that was never set; `SetAsUsed` turned fixed assets back into used ones. |
| 6 | `Services/ServiceProvider.cs:37` | `ContentLoaderManager` was transient: its texture cache was always empty. |
| 7 | `Managers/ContentLoaderManager.cs:66-76` | Sprite animation files were parsed on every load, and the content folder was hard-coded as `"Content/"`. |
| 8 | `Controls/Control.cs:409-436` | `Dispose` only removed the control from its parent; its event handlers kept their targets alive. |
| 9 | `Animations/AnimationAbstract.cs:57-68`, `Timers/DelayTime.cs:41-52` | `SetParent` stopped and restarted the animation or timer on its previous parent. |
| 10 | `Animations/AnimationAbstract.cs:122-150` | A duration of zero gave NaN values, and a looping animation with a duration of zero overflowed the stack. |

### 4.2 Input

| # | Location (1.0.2) | Problem |
|---|---|---|
| 11 | `Controls/InputEvent/ControlMouseEventHandler.cs:106-116` and `Controls/Control.cs:541-550` | Click-through: `Click` and `Released` shared the same arguments, and a missing `Released` handler turned the propagation back on, so the controls below were clicked too. |
| 12 | `Controls/InputEvent/ControlMouseEventHandler.cs:88-94` | A control with only a click handler let the press go to the controls below, which were then pressed and clicked. |
| 13 | `Controls/InputEvent/ControlMouseEventHandler.cs:127-145` | No bounds check: the hidden children of clipped containers and scroll viewers received input. |
| 14 | `Controls/InputEvent/ControlMouseEventHandler.cs:66-70,172-173` | Touch never set the hover state (buttons never showed the pressed texture on touch) and cleared it without the leave events (controls stayed highlighted). |
| 15 | `Controls/InputEvent/ControlMouseEventHandler.cs:169-170` | `IsTouchInput` was lost when the event reached the controls. |
| 16 | `Services/Inputs/MouseInputListener.cs:33-60` | Only the left button was handled, the wheel event had no subscriber, and `Time` was the duration of the frame for the mouse but the total time for touch. |
| 17 | `Controls/ControlManager.cs:25-29` | Input was processed while the window was not active. |
| 18 | `Services/Inputs/TouchInputListener.cs:46-48` | A cancelled touch left the controls pressed. |
| 19 | `Controls/Control.cs:346-358` | `BlockMouseEvents` registered six empty handlers instead of a flag. |
| 20 | `Controls/Control.cs:383-384` | The hit test ignored the rotation of the control. |

### 4.3 Controls

| # | Location (1.0.2) | Problem |
|---|---|---|
| 21 | `Controls/Abstracts/ContainerAbstract.cs:49-55` | `AddChild` did not remove the control from its previous parent: it was drawn twice. |
| 22 | `Controls/Abstracts/ContainerAbstract.cs:72-78` | `RemoveChild` cleared the parent of controls that were not its children. |
| 23 | `Controls/Abstracts/ContainerAbstract.cs:103-137` | Children were copied to new lists several times per frame (iteration, search, update). |
| 24 | `Controls/Button.cs:47-63` | Texture setters did not refresh the drawn texture. |
| 25 | `Controls/Image.cs:33-64`, `Extensions/Texture2DExtension.cs:10-15` | `SetSourceRectangle` did not update the size; the transparent-pixel hit test divided by `Scale` instead of the nested scale, ignored the source rectangle and flips, threw in debug builds and read the GPU on every test. |
| 26 | `Controls/Label.cs:44` | `MeasureString(null)` crashed with a null text. |
| 27 | `Controls/MultiLineLabel.cs:81-205` | Crashed without a parent, rebuilt every line on any change, ignored opacity, compared widths with `<` (off by one) and dropped empty lines. |
| 28 | `Controls/RectangleControl.cs:14-18,38-39` | The origin was a rate instead of pixels, which shifted the hit test (see section 3). |
| 29 | `Controls/ScrollViewer.cs:92-99` | The press panel had the size of the screen, so pressing anywhere started a scroll; a second full-screen panel at `float.MaxValue - 1` caught the moves. |
| 30 | `Controls/ScrollViewer.cs:65-74,101-114,348-351` | The zoom setter skipped the limits; zero-duration looping timers were used as per-frame tickers; the inertia had an unreachable branch. |
| 31 | `Controls/ControlsUI/ScrollViewerPinchZoom.cs:117-132` | Divisions by zero with empty content or fingers at the same position. |
| 32 | `Controls/Sprites/SpriteAnimation.cs:48-58,149-208` | The constructor ignored the cycle name (drawing before `Play` crashed), the end event fired twice, `ChangeSpriteAnimationInfoOnAnimationEnd` threw `NotImplementedException`, and a frame duration of zero looped forever. |
| 33 | `Controls/Builders/SpriteAnimationCycleBuilder.cs:98-221` | Frames alone produced an empty cycle; the automatic frame count was documented but not implemented; the default frame duration was never applied because frames defaulted to 1/60. |
| 34 | `Pipeline/SpriteAnimationPipelineReader.cs:40-59` | Cycles did not inherit the starting count, the frames or the frames by index of the file. |
| 35 | `Animations/FadeAnimation.cs:16,51` | The fade multiplied `Color` every frame, fighting hover colors. |

### 4.4 Math, window and data

| # | Location (1.0.2) | Problem |
|---|---|---|
| 36 | `GameMath/RandomGame.cs:10-28` | The float overload of `Random` could return values below the minimum. |
| 37 | `Managers/GameWindowManager.cs:33,57-65,90` | `OnClientSizeChanged` was never invoked; asymmetric margins placed the screen incorrectly; a minimized window divided by zero. |
| 38 | `Converters/RectangleConverter.cs:19` | No null check, although it was used for `Rectangle?`. |
| 39 | Public names | `AddOnUpddateDestinationRectangle`, `RoationInDegreeStart`, `SetNeedToShortChildren`, `ThiasAsT`, `sizeWidht`, and the Portuguese false friend `actualScreen`/`ActualCycle` ("actual" for "current"). |

### 4.5 Samples

| # | Location (1.0.2) | Problem |
|---|---|---|
| 40 | `Samples/Games/Snake/Screens/SnakeGameScreen.cs:7,226`, `Samples/Games/Ping-Pong/Paddles/PlayerPaddle.cs:1,14` | Did not compile: the `Controls.MouseEvent` namespace and `ControlEventArgs` had been renamed. |
| 41 | `Samples/Games/Snake/ContentHandler.cs:20` | Loaded `"Food"`, but the asset is `food.png` (fails on case-sensitive file systems). |
| 42 | Demo heads (`*.csproj`) | The WinExe and UWP heads relied on `MonoGame.ShaderEffects` flowing from the library; the Android head referenced it with a broken relative path. |
| 43 | `Samples/Demos/.../ScreenManagerScreen.cs:67-73` | The Top and Right margin options changed the wrong margins; the labels said "Matin". |
| 44 | `Samples/Demos/.../ScreenTransitionScreen.cs:150-164` | The transition preview added a black rectangle every cycle and never removed it. |
| 45 | `Samples/Demos/MonoGame.GameManager.Samples.UWP/*.csproj` | Referenced `System.Text.Json` 6.0.1, a version with a known high-severity vulnerability, only needed by the Microsoft.Extensions host. |

## 5. Design patterns in 2.0

| Pattern | Where |
|---|---|
| Registry and Facade | `ServiceRegistry` holds the services; `ServiceProvider` is the static facade that keeps the 1.x call sites working. |
| Strategy | Easing functions (`EasingFunction`), interpolators of `Tween<T>`, transitions (`ITransition`), the music player behind `AudioManager` (`IMusicPlayer`). |
| Composite | The control tree; `AnimationSequence` and `AnimationGroup` made of `IAnimation` objects. |
| Template method | `AnimationAbstract.OnUpdateAnimation` and `CaptureStartValues`; screen hooks (`LoadContent`, `OnInit`, `Update`, `OnCovered`); `ButtonAbstract.GetTexture` and `GetBackgroundColor`; `ContainerAbstract.ArrangeChildren`; `Mover.BeforeMove` and `AfterMove`. |
| State | `StateMachine<TState>` and `IState`. |
| Object pool | `ObjectPool<T>`, the particle array of `ParticleSystem`, the sound instances of `AudioManager`. |
| Builder | `SpriteAnimationCycleBuilder` (kept and fixed). |
| Observer | Events of controls, input listeners, the window manager and the state machine. |
| Command queue | Screen changes are queued as requests and applied at the end of the frame. |
| Null object | `SoundHandle.None` when a sound cannot play. |
| Curiously recurring template | Fluent methods of `Control<T>`, `AnimationAbstract<T>`, `CompositeAnimation<T>` and `ButtonAbstract<T>` return the concrete type. |

## 6. MonoGame facts

These facts were first checked in the metadata of `MonoGame.Framework.DesktopGL` 3.8.0.1641, the last MonoGame version that ships a `netstandard2.0` assembly (later versions target .NET 6 or .NET 8 only). The library was then moved to `net8.0` and `net10.0` and MonoGame 3.8.5.1.

- Available and used: `GraphicsDevice.ScissorRectangle` with `RasterizerState.ScissorTestEnable` (clipping), `SamplerState.PointClamp` (pixel art), `BlendState.NonPremultiplied`, `GamePad` with `GamePadDeadZone`, `Keyboard`, `MouseState.HorizontalScrollWheelValue`, `Game.IsActive`, `TouchPanel.EnabledGestures` and `ReadGesture`, `SoundEffectInstance`, `MediaPlayer`, `RenderTargetUsage.PreserveContents` and `GraphicsDevice.GetRenderTargets`.
- `GameWindow.TextInput` exists only in the desktop builds of MonoGame. The library is compiled once for every platform, so `InputManager` subscribes to it through reflection.
- `ContentManager.UnloadAsset` does not exist in 3.8.0 (only `Unload` of every asset). This is why each screen has its own `ContentManager`.
- `Color` has no addition operator. `MathF` was not available on `netstandard2.0`; it is on .NET 8.
- MonoGame 3.8.1 added an instance method `Vector2.Rotate(float)` that changes the vector and returns nothing. It hides an extension method with the same name, so the extension that returns a rotated copy is named `Vector2Extensions.Rotated`.
- The library is compiled against 3.8.5.1. The samples run on it: the desktop samples on Windows, the Android demo on an Android 16 emulator.

## 7. Verification

No test project or CI was added, by decision of the project owner. The changes were verified as follows.

| Check | Result |
|---|---|
| Library build (`netstandard2.0`, Release) | Succeeded with no warnings. |
| NuGet pack | `lib/netstandard2.0` assembly and XML documentation, README and logo; the only dependency is Newtonsoft.Json 13.0.1. |
| Samples compiled against the library (Demos shared code with the WinExe entry point, Ping-Pong, Snake, Tic-Tac-Toe) | All succeeded with no warnings. |
| Smoke program without a graphics device | 535 checks passed (see below). |
| C# examples of the README | All compile. |

The smoke program covered easing functions, the scheduler, timers, animations and composites, pointer dispatch (click-through, clipping, capture, touch hover, disabled and hidden controls, rotated hit tests), the camera and the camera panel input, the state machine, the object pool, collision, math and random helpers, the input map with simulated keyboard, mouse and gamepad states, the save service in a temporary folder, the mover, particles, text wrapping, stack and grid layouts, buttons, checkboxes, toggle buttons, the scroll viewer (taps, drags, capture outside the viewer, inertia, wheel, animated scrolling, zoom), sliders, progress bars, text boxes, labels, the tile map and shape hit tests.

Not verified here, because it needs a display or a device:

- Drawing: scissor clipping, render-target clipping of rotated containers, camera transforms, nine-slice and tiled images, particles, primitives and text rendering.
- Audio playback, real touch screens and gestures, window resizing and full screen.
- Running the sample games.

## 8. Future work

- Turn the smoke checks into a unit test project and add a CI workflow that builds the library, packs it and compiles the samples.
- Enable nullable reference types.
- A content pipeline extension for `.sa` files (they are read as JSON at runtime today).
- Use the new controls in the demos (`Button.SetText` instead of a button with a label). The samples now run on .NET 10 and MonoGame 3.8.5.1, and the UWP head was removed.
- Keyboard and gamepad navigation between controls (focus order).
- Text input on mobile platforms (on-screen keyboard), key repetition and selection in `TextBox`.
- Tile maps: layers, animated tiles and import of Tiled (`.tmx`) maps; a "move and slide" helper for collisions with solid tiles.
