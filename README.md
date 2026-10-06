<p align="center">
  <img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Logo.png" alt="MonoGame.GameManager logo" width="120" height="120">
</p>

# MonoGame.GameManager

**MonoGame.GameManager** is a library for making 2D games with [MonoGame](https://monogame.net). It takes care of the parts that every game needs, so you can focus on the game itself:

- **Screens** with a virtual resolution, transitions and overlays (pause menus, dialogs).
- **Controls**: images, texts, buttons, panels, checkboxes, sliders, text boxes, scroll viewers, layout panels, sprite animations, tile maps and more, all with mouse and touch input.
- **Animations**: tweens with easing, sequences and groups, plus timers and a scheduler for every screen.
- **Game systems**: keyboard, gamepad, mouse and touch input with named actions, a 2D camera, collision helpers, simple physics, state machines, object pools, particles, audio and save games.

Version 2.0 is a complete review of the library. [CHANGES.md](https://github.com/DaniloPeres/MonoGame.GameManager/blob/main/CHANGES.md) lists what changed and how to migrate from 1.x, and [docs/PROJECT_REVIEW.md](https://github.com/DaniloPeres/MonoGame.GameManager/blob/main/docs/PROJECT_REVIEW.md) contains the review.

## Table of Contents

- [Installation](#installation)
- [Quick start](#quick-start)
- [Architecture](#architecture)
- [Screens](#screens)
- [Controls](#controls)
  - [Button](#button)
  - [Image](#image)
  - [Label](#label)
  - [Multi-line Labels](#multi-line-labels)
  - [Panel](#panel)
  - [Rectangle](#rectangle)
  - [Sprite Animation](#sprite-animation)
  - [Scroll Viewer](#scroll-viewer)
  - [Input controls](#input-controls)
  - [Layout panels](#layout-panels)
  - [Shapes, tiles and effects](#shapes-tiles-and-effects)
  - [Pointer events](#pointer-events)
- [Animations](#animations)
  - [Ease Animation](#ease-animation)
  - [Fade Animation](#fade-animation)
  - [Rotation Animation](#rotation-animation)
  - [Scale Animation](#scale-animation)
  - [Tweens, sequences and groups](#tweens-sequences-and-groups)
- [Timers and scheduler](#timers-and-scheduler)
- [Input](#input)
- [Camera](#camera)
- [Collision and math](#collision-and-math)
- [Physics, state machines, pools and particles](#physics-state-machines-pools-and-particles)
- [Audio](#audio)
- [Save games](#save-games)
- [Services](#services)
- [Screen](#screen)
  - [Screen Transition](#screen-transition)
  - [Screen Manager](#screen-manager)
- [Samples - Demos](#samples---demos)
- [Samples - Games](#samples---games)
- [Migrating from 1.x](#migrating-from-1x)
- [License](#license)

## Installation

The NuGet package is available at https://www.nuget.org/packages/MonoGame.GameManager/.

```
dotnet add package MonoGame.GameManager
```

The package targets `net8.0` and `net10.0` and is compiled against MonoGame 3.8.5.1. Games on .NET 8, .NET 9 and .NET 10 can use it. It does not bring a MonoGame platform package: your game references the one it runs on, for example `MonoGame.Framework.DesktopGL`, `MonoGame.Framework.WindowsDX` or `MonoGame.Framework.Android`.

## Quick start

A game is a `ScreenManager` (a MonoGame `Game`) that shows one screen at a time.

```csharp
public static class Program
{
    [STAThread]
    public static void Main()
    {
        var settings = new ScreenManagerSettings
        {
            VirtualResolution = new Point(1280, 720),
            Title = "My Game",
            AllowUserResizing = true,
            DefaultTransition = new FadeTransition()
        };

        using (var game = new ScreenManager(settings, new MenuScreen()))
            game.Run();
    }
}

public class MenuScreen : Screen
{
    private SpriteFont font;

    public override void LoadContent()
    {
        // Assets loaded with Content are released when the screen closes.
        font = Content.LoadSpriteFont("Fonts/Main");
    }

    public override void OnInit()
    {
        new Label(font, "My Game", new Vector2(0, 120), Color.White)
            .SetAnchor(Anchor.TopCenter)
            .AddToScreen();

        new Button(new Vector2(0, 40), new Vector2(240, 60), Color.SteelBlue)
            .SetText(font, "Play")
            .SetAnchor(Anchor.Center)
            .AddOnClick(args => ChangeScreen(new GameScreen()))
            .AddToScreen();
    }
}
```

Everything is drawn at the virtual resolution and scaled to fit the window, keeping the aspect ratio. A control is positioned relative to an anchor of its parent (`TopLeft` by default), and its fluent methods return its own type, so calls can be chained.

## Architecture

```
ScreenManager (the MonoGame Game)
 ├─ ServiceProvider ─► ServiceRegistry: IClock, IRandom, IScheduler (global), IGameWindowManager,
 │                     IContentLoader, IAssetCache, IInputManager, IAudioManager, ISaveGameService
 ├─ ControlManager ─► stage panel ─┬─ root panel of screen A ─► its controls
 │  (sprite batch states, clipping) └─ root panel of screen B (an overlay) ─► its controls
 ├─ InputManager ─► ControlMouseEventHandler ─► pointer events of the controls
 └─ every frame: clock ─► input ─► update events of the controls ─► screen schedulers and Screen.Update
                 ─► audio ─► global scheduler ─► screen changes requested during the frame
```

- **ServiceProvider** is a static facade over a `ServiceRegistry`. Every service is an interface, so a game can replace any of them, and a class that needs a service can receive it in its constructor instead.
- **Screens** own a root panel, a scheduler and a content loader. Closing a screen disposes all three, so controls, animations, timers and assets never outlive their screen.
- **Controls** form a tree (Composite pattern). Containers clip their children with the scissor rectangle; rotated containers use a render target of their own size.
- **Animations and timers** are `IUpdatable` objects updated by a scheduler. They implement `IPlayable` (`Play`, `Pause`, `Resume`, `Stop`, `Reset`).

## Screens

A screen has four hooks: `LoadContent` loads assets, `OnInit` creates the controls, `Update` runs every frame and `Dispose` releases what the screen created itself.

```csharp
ChangeScreen(new GameScreen());                                           // with the default transition
ChangeScreen(new GameScreen(), new SlideTransition(SlideDirection.Left)); // with a given transition
ChangeScreenWithNoTransition(new GameScreen());
PushScreen(new PauseScreen());                                            // opens an overlay over this screen
PopScreen();                                                              // closes the overlay
```

Screen changes are applied at the end of the frame, so they are safe inside click handlers and animation callbacks. By default a covered screen is still drawn but stops updating, and an overlay is modal: the screens below it do not receive pointer input. A screen can change this by overriding `DrawWhenCovered`, `UpdateWhenCovered` and `IsModal`.

```csharp
public class PauseScreen : Screen
{
    public override void OnInit()
    {
        var font = ServiceProvider.ContentLoader.LoadSpriteFont("Fonts/Main"); // kept for the whole game

        new RectangleControl(Vector2.Zero, ScreenManager.ScreenSize.ToVector2(), Color.Black * 0.6f)
            .AddToScreen();

        new Button(Vector2.Zero, new Vector2(220, 56), Color.SteelBlue)
            .SetText(font, "Resume")
            .SetAnchor(Anchor.Center)
            .AddOnClick(args => PopScreen())
            .AddToScreen();
    }
}
```

`Screen.Content` loads assets that are released with the screen. `ServiceProvider.ContentLoader` keeps assets for the whole game and caches the sprite animation files (`.sa`).

## Controls

### Button
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosButtons.gif" alt="MonoGame.GameManager samples demo - buttons" width="600" height="400">

A button is drawn with a texture for each state (normal, hover, pressed, disabled) or with solid colors, and can show a text. Other controls can be added to it.

```csharp
new Button(buttonTexture, new Vector2(20, 20))
    .SetHoverTexture(buttonHoverTexture)
    .SetMousePressedTexture(buttonPressedTexture)
    .SetText(font, "Options")
    .AddOnClick(args => PushScreen(new PauseScreen()))
    .AddToScreen();

var playButton = new Button(new Vector2(20, 100), new Vector2(200, 50), Color.SeaGreen)
    .SetText(font, "Play")
    .SetBorder(Color.White, 2)
    .AddToScreen();
playButton.SetIsEnabled(false); // drawn in gray, does not react
```

### Image
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosImages.gif" alt="MonoGame.GameManager samples demo - images" width="600" height="400">

```csharp
new Image(playerTexture)
    .SetSourceRectangle(new Rectangle(0, 0, 32, 32)) // a part of the texture (eg: an atlas)
    .SetOriginRate(0.5f)                             // rotates and scales around its center
    .SetScale(2f)
    .SetIgnoreIntersectionTransparentPixels(true)    // transparent pixels are not clickable
    .SetPosition(200, 200)
    .AddToScreen();
```

### Label
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosLabel.gif" alt="MonoGame.GameManager samples demo - labels" width="600" height="400">

```csharp
var scoreLabel = new Label(font, "Score: 0", new Vector2(20, 20), Color.Yellow)
    .SetAnchor(Anchor.TopRight) // the position of right and bottom anchors points inwards
    .AddToScreen();
scoreLabel.SetText("Score: 100");
```

### Multi-line Labels
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosMulti-lineLabel.gif" alt="MonoGame.GameManager samples demo - Multi-line labels" width="600" height="400">

```csharp
new MultiLineLabel(font, "A long text is wrapped to fit the width of the box.\nLine breaks are kept.", new Vector2(20, 20), Color.White, 300)
    .SetTextAlign(TextAlign.Center)
    .SetLineSpacing(4)
    .AddToScreen();
```

### Panel
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosPanel.gif" alt="MonoGame.GameManager samples demo - Panel" width="600" height="400">

```csharp
var panel = new Panel(new Vector2(40, 40), new Vector2(400, 300))
    .SetBackgroundColor(new Color(20, 20, 30))
    .SetBorder(Color.White, 2)
    .SetHideOverflow(true) // the children are clipped to the panel
    .AddToScreen();

new Label(font, "Inventory", new Vector2(0, 10), Color.White)
    .SetAnchor(Anchor.TopCenter)
    .AddToScreen(panel);
```

### Rectangle
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosRectangle.gif" alt="MonoGame.GameManager samples demo - Rectangle" width="600" height="400">

```csharp
new RectangleControl(new Rectangle(100, 100, 200, 40), Color.DarkBlue)
    .SetBorder(Color.White, 1)
    .AddToScreen();
```

### Sprite Animation
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosSpriteAnimation.gif" alt="MonoGame.GameManager samples demo - Sprite Animation" width="600" height="400">

Sprite animations are described in a `.sa` JSON file (cycles, frames and durations) or built in code with `SpriteAnimationCycleBuilder`.

```csharp
var dino = Content.LoadSpriteAnimationInfo("Images/Sprites/Dino/Dino.sa")
    .CreateSpriteAnimation("idle")
    .SetPosition(100, 100)
    .AddToScreen()
    .Play();

dino.Play("run");
dino.ChangeCycleOnAnimationEnd("idle"); // after the current cycle ends
```

### Scroll Viewer
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosScrollViewer.gif" alt="MonoGame.GameManager samples demo - Scroll Viewer" width="600" height="396">

The content scrolls by dragging (with inertia), with the mouse wheel or from code, and zooms with two fingers. A drag only starts after the pointer moves a few pixels, so the content still receives clicks.

```csharp
var levels = new ScrollViewer(new Vector2(20, 80), new Vector2(300, 400)).AddToScreen();
for (var i = 0; i < 50; i++)
{
    var level = i + 1;
    new Button(new Vector2(10, i * 50), new Vector2(280, 44), Color.DimGray)
        .SetText(font, "Level " + level)
        .AddOnClick(args => ChangeScreen(new GameScreen()))
        .AddToScreen(levels);
}

levels.ScrollToBottom(duration: 0.5f);
```

### Input controls

```csharp
new Checkbox(new Vector2(20, 20))
    .SetLabel(font, "Full screen")
    .AddOnCheckedChanged(isChecked => ServiceProvider.GameWindowManager.SetFullScreen(isChecked))
    .AddToScreen();

new Slider(new Vector2(20, 70), new Vector2(240, 24))
    .SetValue(Audio.MusicVolume, notify: false)
    .AddOnValueChanged(volume => Audio.MusicVolume = volume)
    .AddToScreen();

new ToggleButton(new Vector2(20, 110), new Vector2(120, 40), Color.DimGray)
    .SetToggledBackgroundColor(Color.SeaGreen)
    .SetText(font, "Music")
    .AddOnToggled(isOn => Audio.IsMuted = !isOn)
    .AddToScreen();

new TextBox(font, new Vector2(20, 170), new Vector2(260, 40))
    .SetPlaceholder("Your name")
    .SetMaxLength(16)
    .AddOnSubmit(name => StartGame(name))
    .AddToScreen();

var health = new ProgressBar(new Vector2(20, 230), new Vector2(240, 16))
    .SetColors(Color.DarkRed, Color.Red)
    .SetValue(1f)
    .AddToScreen();
health.AnimateTo(0.4f, 0.3f);

Tooltip.Attach(health, font, "Health");
```

The text box receives the characters typed on desktop platforms (it uses the text input of the game window, with the keyboard layout of the system).

### Layout panels

`StackPanel` places its children one after the other and `GridPanel` places them in cells of the same size. Both set the position of their children.

```csharp
var menu = new StackPanel(Vector2.Zero)
    .SetSpacing(12)
    .SetChildAlignment(ChildAlignment.Center)
    .SetAnchor(Anchor.Center)
    .AddToScreen();

foreach (var option in new[] { "Play", "Options", "Quit" })
    menu.AddChild(new Button(Vector2.Zero, new Vector2(240, 56), Color.SteelBlue).SetText(font, option));

var inventory = new GridPanel(new Vector2(40, 40), 6, 4, new Vector2(64))
    .SetSpacing(new Vector2(4))
    .AddToScreen();
inventory.SetChild(0, 0, new Image(swordTexture));
```

### Shapes, tiles and effects

```csharp
new NineSliceImage(windowTexture, new Thickness(16), new Vector2(100, 100), new Vector2(500, 300)).AddToScreen();
new TiledImage(grassTexture, Vector2.Zero, ScreenManager.ScreenSize.ToVector2()).AddToScreen();
new CircleControl(new Vector2(300, 300), 40, Color.Orange).AddToScreen();
new LineControl(new Vector2(0, 0), new Vector2(200, 120), Color.White, 2).AddToScreen();
new FpsCounter(font, new Vector2(10, 10), Color.Yellow).SetZIndex(ZIndexLayers.Overlay).AddToScreen();

var map = new TileMap(tileset, new Point(16, 16), 3, 2)
    .SetTiles(new[,] { { 0, 1, 2 }, { 3, -1, 3 } }) // [row, column], as written; -1 is empty
    .SetSolidTiles(3)
    .AddToScreen();
var touchesWall = map.CollidesWith(new RectangleF(10, 20, 16, 16));
```

### Pointer events

Controls receive `AddOnClick`, `AddOnMousePressed`, `AddOnMouseReleased`, `AddOnMouseMoved`, `AddOnMouseEnter`, `AddOnMouseLeave` and `AddOnMouseWheel`, for the mouse and for touch. The top-most control under the pointer receives an event first. A control that handles an event stops it, unless its handler calls `ContinuePropagation()`.

```csharp
var card = new Image(cardTexture)
    .SetPosition(300, 200)
    .AddOnMouseEnter(args => cardShadow.SetIsVisible(true))
    .AddOnMouseLeave(args => cardShadow.SetIsVisible(false))
    .AddOnClick(args => FlipCard())
    .SetAcceptedMouseButtons(MouseButtons.Left | MouseButtons.Right)
    .AddToScreen();

card.SetOpacity(0.5f);      // the control and its children
card.SetIsEnabled(false);   // no input
card.SetIsVisible(false);   // not drawn, no input
```

## Animations

Every animation has a duration, an easing function, a delay, repeats, ping-pong and reverse modes, and callbacks (`AddOnStarted`, `AddOnAnimationEnd` for every cycle, `AddOnCompleted` at the end). Animations run on the scheduler of the current screen and stop when their control is disposed.

### Ease Animation
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosEaseAnimation.gif" alt="MonoGame.GameManager samples demo - Ease Animation" width="600" height="400">

`MoveAnimation` moves a control. The 1.x name `EaseAnimation` still works.

```csharp
new MoveAnimation(player, 0.5f, new Vector2(400, 300))
    .SetEasing(Easing.BackOut)
    .AddOnCompleted(() => player.SetColor(Color.Gold))
    .Play();
```

### Fade Animation
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosFadeAnimation.gif" alt="MonoGame.GameManager samples demo - Fade Animation" width="600" height="400">

`OpacityAnimation` fades a control and its children without changing their colors.

```csharp
new OpacityAnimation(title, 1f, 0f)
    .SetDelay(2f)
    .SetShouldRemoveControlOnAnimationEnd(true)
    .Play();
```

### Rotation Animation
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosRotationAnimation.gif" alt="MonoGame.GameManager samples demo - Rotation Animation" width="600" height="400">

```csharp
new RotationAnimation(coin, 1f, 360f)
    .SetIsLooping(true)
    .Play();
```

### Scale Animation
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosScaleAnimation.gif" alt="MonoGame.GameManager samples demo - Scale Animation" width="600" height="400">

```csharp
new ScaleAnimation(coin, 0.3f, 1.3f)
    .SetEasing(EasingType.SineInOut)
    .SetIsPingPong(true)
    .SetRepeatCount(4)
    .Play();
```

### Tweens, sequences and groups

`ColorAnimation` and `BlinkAnimation` animate colors and visibility. A `Tween` animates any value, `AnimationSequence` plays animations one after the other and `AnimationGroup` plays them together.

```csharp
Tween.Int(value => scoreLabel.SetText("Score: " + value), 0, 1500, 1f)
    .SetEasing(Easing.CubicOut)
    .Play();

new AnimationSequence()
    .Add(new MoveAnimation(player, 0.3f, new Vector2(400, 200)).SetEasing(Easing.QuadOut))
    .AddDelay(0.2f)
    .Add(new AnimationGroup()
        .Add(new ScaleAnimation(player, 0.2f, 1.5f))
        .Add(new ColorAnimation(player, 0.2f, Color.Red)))
    .AddCallback(() => SpawnEnemy())
    .Play();

new BlinkAnimation(player, 1f, 5).Play();
```

## Timers and scheduler

Every screen has a `Scheduler`. Its timers stop when the screen closes, and its `TimeScale` slows down or speeds up the animations and timers of the screen.

```csharp
Scheduler.Delay(2f, () => SpawnEnemy());              // once, in 2 seconds
var spawner = Scheduler.Every(1.5f, SpawnEnemy);      // forever
Scheduler.Every(0.5f, SpawnEnemy, repeatCount: 6);    // 6 times
Scheduler.NextFrame(() => SpawnEnemy());
spawner.Cancel();
Scheduler.TimeScale = 0.5f;                           // slow motion for this screen

new DelayTime(3f, ShowHint).Play();                   // the timer of version 1.x still works
```

`ServiceProvider.GlobalScheduler` runs timers that survive screen changes. `ServiceProvider.Clock.TimeScale` changes the speed of the whole game.

## Input

`Input` (on every screen, or `ServiceProvider.Input`) gives access to the keyboard, the mouse, the touch panel and four gamepads, with the keys pressed and released in each frame. An `InputMap` gives names to actions, so the game does not depend on the device.

```csharp
Input.Map
    .Bind("jump", Keys.Space)
    .Bind("jump", Buttons.A)
    .BindAxis("move", Keys.Left, Keys.Right)
    .BindAxis("move", GamePadAxis.LeftThumbStickX);
```

```csharp
public override void Update(GameTime gameTime)
{
    var direction = Input.GetAxis("move");      // from -1 to 1
    if (Input.IsActionPressed("jump"))          // only in the frame it was pressed
        Jump();
    if (Input.Keyboard.WasKeyPressed(Keys.Escape))
        PushScreen(new PauseScreen());
    if (Input.GamePads.WasButtonPressed(Buttons.Start, PlayerIndex.Two))
        PushScreen(new PauseScreen());
}
```

## Camera

`Camera2D` follows a target, shakes, zooms, rotates and stays inside the bounds of the world. `CameraPanel` shows its children through a camera: pointer input is converted to world coordinates, and children outside of the view are not drawn.

```csharp
var world = new CameraPanel(Vector2.Zero, ScreenManager.ScreenSize.ToVector2()).AddToScreen();
world.AddChild(map);
world.AddChild(player);

world.Camera.Bounds = new RectangleF(0, 0, 4000, 2000);
world.Camera.Follow(() => player.PositionAnchor, lerpSpeed: 6f);
world.Camera.Zoom = 2f;
world.Camera.Shake(8f, 0.3f);

var pointerInWorld = world.ScreenToWorld(Input.Mouse.Position.ToVector2());
```

## Collision and math

```csharp
if (Collision.RectanglesIntersect(playerBounds, wallBounds, out var push))
    playerPosition += push; // the smallest move that separates the player from the wall

if (Collision.SweptRectangles(bulletBounds, bulletVelocity * deltaSeconds, targetBounds, out var time, out var normal))
    HitTarget(time, normal); // no tunneling through thin targets

var hit = Collision.CircleRectangleIntersect(new Circle(ballCenter, 8f), paddleBounds, out var bounce);
var aim = (targetPosition - playerPosition).NormalizedOrZero();
var angle = MathUtils.LerpAngle(currentAngle, targetAngle, 0.1f);
var spawnPoint = RandomGenerator.Default.NextPointInCircle(new Circle(playerPosition, 50f));
```

`RectangleF`, `Circle` and `LineSegment` are the shapes; `Collision` also tests points, polygons, segments and rays. `Primitives` draws lines, rectangles, circles and polygons with a `SpriteBatch`, and `TextureFactory` creates circle and rounded-rectangle textures.

## Physics, state machines, pools and particles

```csharp
var ballMover = new ControlMover(ball) { Velocity = new Vector2(300, -200), Gravity = new Vector2(0, 600), Drag = 0.1f }.Start();

var ai = new StateMachine<EnemyState>()
    .AddState(EnemyState.Patrol, onUpdate: deltaTime => Patrol(deltaTime))
    .AddState(EnemyState.Chase, onEnter: () => enemySpeed = 200f, onUpdate: deltaTime => Chase(deltaTime))
    .AddTransition(EnemyState.Patrol, EnemyState.Chase, () => DistanceToPlayer() < 150)
    .AddTransition(EnemyState.Chase, EnemyState.Patrol, () => DistanceToPlayer() > 300);
ai.SetInitialState(EnemyState.Patrol);
Scheduler.Add(ai); // updated every frame with the screen

var bullets = new ObjectPool<Bullet>(() => new Bullet(), initialSize: 32);
var bullet = bullets.Get();
bullets.Return(bullet);

var sparks = new ParticleEmitter(new ParticleSettings
{
    SpeedMin = 80, SpeedMax = 260,
    LifetimeMin = 0.3f, LifetimeMax = 0.8f,
    StartColor = Color.Orange, EndColor = Color.Transparent,
    Gravity = new Vector2(0, 400), Size = 6
}).SetPosition(400, 300).AddToScreen();
sparks.Burst(60);
```

## Audio

```csharp
Audio.PlayMusic("Music/Theme", fadeInSeconds: 1f);
Audio.PlaySound("Sounds/Jump", volume: 0.8f, pitch: RandomGenerator.Default.NextFloat(-0.1f, 0.1f));
Audio.MusicVolume = 0.5f;
Audio.SoundVolume = 0.8f;
Audio.StopMusic(fadeOutSeconds: 2f);
```

Sound effect instances are pooled and reused, up to `MaxInstancesPerSound` playing at the same time for each sound.

## Save games

`ServiceProvider.Storage` saves any object as JSON in the application data folder (`ScreenManagerSettings.ApplicationName`). Every save is written to a temporary file first, so a crash while saving does not corrupt the previous save. MonoGame types (`Vector2`, `Point`, `Rectangle`, `Color`) are supported.

```csharp
ServiceProvider.Storage.Save("slot1", new SaveData { Level = 3, Position = player.PositionAnchor });
var save = ServiceProvider.Storage.Load("slot1", new SaveData());
var slots = ServiceProvider.Storage.ListSlots();
```

## Services

The services can be read, registered and replaced through `ServiceProvider`. Services registered before the `ScreenManager` is created replace the default ones.

```csharp
ServiceProvider.Replace<ISaveGameService>(new JsonSaveGameService("Saves"));
ServiceProvider.Register(new Leaderboard());
var leaderboard = ServiceProvider.Get<Leaderboard>();
```

## Screen

### Screen Transition
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosScreenTransition.gif" alt="MonoGame.GameManager samples demo - Screen Transition" width="600" height="400">

`FadeTransition` fades to a color and back, and `SlideTransition` slides the screens. A transition implements `ITransition` (`TransitionOut` and `TransitionIn`).

```csharp
ScreenManager.DefaultTransition = new FadeTransition(0.3f, Color.Black);
ChangeScreen(new GameScreen(), new SlideTransition(SlideDirection.Up, 0.4f));
```

### Screen Manager

`ScreenManagerSettings` configures the virtual resolution, the window, full screen, pixel art (point sampling), the background colors, the default transition and the input when the window is not active. `ServiceProvider.GameWindowManager` changes the window at runtime and adds margins around the screen.

```csharp
ServiceProvider.GameWindowManager.SetWindowSize(new Point(1920, 1080));
ServiceProvider.GameWindowManager.ToggleFullScreen();
ServiceProvider.GameWindowManager.SetMarginTop(40); // in virtual pixels
```

## Samples - Demos
Repository: https://github.com/DaniloPeres/MonoGame.GameManager/tree/main/Samples/Demos

The samples need the .NET 10 SDK. The demos have a desktop head (`MonoGame.GameManager.Samples.WinExe`, DesktopGL, `net10.0`) and an Android head (`MonoGame.GameManager.Samples.Android`, `net10.0-android`, Android 10 or later, needs the `android` workload). The games are desktop only (DesktopGL, `net10.0`).

The content is built by the MonoGame content builder, a local .NET tool. Restore it once, from the repository root, before the first build:

```
dotnet tool restore
dotnet run --project Samples/Demos/MonoGame.GameManager.Samples.WinExe
```

<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosMainScreen.gif" alt="MonoGame.GameManager samples demo main screen" width="600" height="394">

## Samples - Games

### Ping-Pong
Repository: https://github.com/DaniloPeres/MonoGame.GameManager/tree/main/Samples/Games/Ping-Pong

<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Games/Ping-Pong/Sample-Ping-Pong.gif" alt="MonoGame.GameManager samples games - Ping-Pong" width="600" height="400">

### Snake
Repository: https://github.com/DaniloPeres/MonoGame.GameManager/tree/main/Samples/Games/Snake

<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Games/Snake/Sample-Snake.gif" alt="MonoGame.GameManager samples games - Snake" width="480" height="335">

### Tic-Tac-Toe
Repository: https://github.com/DaniloPeres/MonoGame.GameManager/tree/main/Samples/Games/Tic-Tac-Toe

<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Games/Tic-Tac-Toe/Sample-Tic-Tac-Toe.gif" alt="MonoGame.GameManager samples games - Tic-Tac-Toe" width="320" height="480">

## Migrating from 1.x

Most 1.x code keeps compiling: renamed members still exist and are marked `[Obsolete]` with the new name. The main changes are:

- Reference a MonoGame platform package in your game (the library no longer brings `MonoGame.Framework.DesktopGL`).
- `ServiceProvider.ContentLoaderManager` is now `ServiceProvider.ContentLoader`, `Screen.ContentLoader` is now `Screen.Content`, and the memory manager is gone (assets are released with the screen that loaded them).
- `FadeAnimation` is replaced by `OpacityAnimation`, `EaseAnimation` by `MoveAnimation`, and `ResetAnimation` by `Reset`.
- `ITransition` has two methods, `TransitionOut` and `TransitionIn`.

[CHANGES.md](https://github.com/DaniloPeres/MonoGame.GameManager/blob/main/CHANGES.md) has the complete list.

## License

MIT
