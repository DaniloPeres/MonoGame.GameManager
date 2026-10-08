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
  - [Button effects](#button-effects)
  - [Glow and shadow](#glow-and-shadow)
  - [Casual game titles](#casual-game-titles-gradients-bevels-stacked-outlines)
  - [Image](#image)
  - [Fonts](#fonts)
  - [Label](#label)
  - [Multi-line Labels](#multi-line-labels)
  - [Panel](#panel)
  - [Rectangle](#rectangle)
  - [Sprite Animation](#sprite-animation)
  - [Scroll Viewer](#scroll-viewer)
  - [Slider](#slider)
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
- [Physics, state machines and pools](#physics-state-machines-and-pools)
- [Particles](#particles)
- [Audio](#audio)
- [Save games](#save-games)
- [Services](#services)
- [Screen](#screen)
  - [Screen Transition](#screen-transition)
  - [Screen Manager](#screen-manager)
- [Samples - Demos](#samples---demos)
- [Samples - Games](#samples---games)
- [Migrating from 2.x](#migrating-from-2x)
- [Migrating from 1.x](#migrating-from-1x)
- [License](#license)

## Installation

The NuGet package is available at https://www.nuget.org/packages/MonoGame.GameManager/.

```
dotnet add package MonoGame.GameManager
```

The package targets `net8.0` and `net10.0` and is compiled against MonoGame 3.8.5.1. Games on .NET 8, .NET 9 and .NET 10 can use it. It does not bring a MonoGame platform package: your game references the one it runs on, for example `MonoGame.Framework.DesktopGL`, `MonoGame.Framework.WindowsDX` or `MonoGame.Framework.Android`. Text is rendered with [FontStashSharp](https://github.com/FontStashSharp/FontStashSharp) (`FontStashSharp.MonoGame`, brought by the package).

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
    private SpriteFontBase font;

    public override void LoadContent()
    {
        // Assets loaded with Content are released when the screen closes.
        font = Content.LoadFont("Fonts/Roboto-Regular.ttf", 24);
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
        var font = ServiceProvider.ContentLoader.LoadFont("Fonts/Roboto-Regular.ttf", 24); // kept for the whole game

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

### Button effects
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosButtonEffects.png" alt="MonoGame.GameManager samples demo - button effects" width="600" height="400">

Buttons (and toggle buttons) get effects. Some are static: glossy highlights, glints, inner light, shaded bottoms, outlines, 3D lips, drop shadows and outlined text. Others are animated lights: halos, lit rims, gems, lights running around the border, sweeps, sparkles, rays and ripples. They follow the size, the corner radius, the scale and the rotation of the button, so the same effects fit a small icon and a wide banner. `SetCornerRadius` also rounds the background color and the border of the button.

```csharp
// Rich static presets (a lot of glow and particles, nothing moves): Ornate, Starlight, Treasure, Crystal, Galaxy, Fairy, Sunburst
// Static presets (no movement, no blinking): Candy, Jelly, Glossy, Cartoon, Bubble, Pearl, Soft Glow, Inner Light, Framed, Minimal
// Animated presets: Gold, Royal, Magic, Ice, Fire, Emerald, Neon, Rainbow, Legendary, Glass, Alert, Subtle
new Button(new Vector2(100, 100), new Vector2(240, 64), new Color(255, 180, 20))
    .SetCornerRadius(32)
    .SetText(font, "PLAY")
    .AddEffects(ButtonEffectPresets.Candy())
    .AddToScreen();

// A calm glossy button made by hand: the borders and the lip use darker shades of the button color
nextButton
    .AddEffect(new DropShadowEffect().SetOffset(new Vector2(0, 5)).SetBlur(8))
    .AddEffect(new DepthEffect().SetDepth(6))
    .AddEffect(new OutlineEffect().UseButtonColor(0.6f).SetThickness(3))
    .AddEffect(new InnerShadeEffect().SetHeightRate(0.5f).SetIntensity(0.2f))
    .AddEffect(new HighlightEffect().SetInset(8, 4).SetHeightRate(0.4f))
    .AddEffect(new GlintEffect().SetCorners(Anchor.TopLeft).SetDots(2, 6))
    .AddEffect(new TextOutlineEffect().UseButtonColor(0.7f).SetShadow(new Vector2(0, 2), Color.Black * 0.3f));

// Effects made by hand, with fluent options
battleButton
    .AddEffect(new GlowEffect().SetColor(Color.DeepSkyBlue).SetRadius(18).SetPulse(1f, 0.4f))
    .AddEffect(new GlowEffect(GlowPlacement.Rim).SetColor(Color.Gold).SetThickness(2).SetOffset(-3))
    .AddEffect(new GlossEffect().SetHeightRate(0.45f))
    .AddEffect(new ShineSweepEffect().SetTiming(0.7f, 2.5f))
    .AddEffect(new CornerLightEffect().SetAnchors(Anchor.TopCenter).SetShape(LightShape.Diamond).SetStreak(110, 4))
    .AddEffect(new RunningLightEffect().SetCount(2).SetSpeed(240))
    .AddEffect(new SparkleEffect().SetArea(SparkleArea.Around).SetClickBurst(12))
    .AddEffect(new LightFillEffect().SetIntensity(0.2f).VisibleOnHover());
```

| Effect | Layer | What it does |
|---|---|---|
| `GlowEffect` | behind, inside or front | A halo around the button (`Outer`), light along the inside of the edge (`Inner`) or a lit border (`Rim`). Radius, falloff, thickness, softness, offset. |
| `GlossEffect` | inside | A glossy reflection at the top (or the bottom) of the button. |
| `ShineSweepEffect` | inside | A diagonal band of light that crosses the button every few seconds or when the pointer enters it. |
| `LightFillEffect` | inside | Lights the whole button: a hover highlight, a pulse, a flash. |
| `ClickRippleEffect` | inside | A ring of light that grows from where the button is pressed. |
| `CornerLightEffect` | front | Gems and sparkles on the corners and on the middle of the edges, with a twinkle and an optional streak. |
| `RunningLightEffect` | front | Lights with a fading tail that run around the border. |
| `SparkleEffect` | front | Sparkles on the border, around, inside or above the button, and bursts on click (built on the particle system). |
| `LightRaysEffect` | behind | Rays that turn slowly behind the button, from its center or from a point of its outline. |
| `HighlightEffect` | inside | A static soft reflection: a rounded band at the top, a thin strip, an oval, or a band at the bottom. |
| `GlintEffect` | front | Static reflection dots near a corner: an oval followed by smaller dots. |
| `InnerShadeEffect` | inside, painted | Darkens the bottom, the top, the edges or the whole button. |
| `OutlineEffect` | behind or inside, painted | A solid rounded border of any thickness around the button or inside its edge; stack several for double borders. |
| `DepthEffect` | behind, painted | A solid 3D lip under the button, thinner while it is pressed. |
| `DropShadowEffect` | behind, painted | A soft shadow under the button. |
| `TextOutlineEffect` | inside, painted | An outline and a shadow under the text of the button. |
| `FrameEffect` | behind or inside, painted | A border made of several rings (levels): `Gold`, `Silver`, `Bronze`, `Gem(color)`, `ButtonColor`, `Dark`, `Candy`, or your own with `AddLayer`. |
| `ParticleFieldEffect` | inside or front | Still particles (glows, sparkles, stars, dots, diamonds) inside the button, in its upper or lower half, above, under or around it; they can twinkle or float up. |

Lights are added to what is below them (`ButtonEffectBlend.Light`), so they can only brighten. Shades, outlines, lips, shadows and text outlines are painted over it (`ButtonEffectBlend.Normal`), so they can darken; `SetBlend` changes it for any effect (eg: a dark glow). An inner `GlowEffect` can light every edge or only the top or the bottom one (`SetEdges`). `UseButtonColor(shade)` makes any effect use a darker (0 to 1) or lighter (0 to -1) version of the background color of the button, so the static presets fit buttons of every color.

Every effect has a color (or a `ColorCycle`), an intensity, a pulse, a time offset and an intensity for each visual state: `VisibleOnHover()` shows an effect only under the pointer, and a disabled button turns its lights off. The effects drawn inside the button are clipped to its shape (`EffectMask`: a rectangle, the corner radius, or the transparency of the texture); `EffectPadding` shrinks the shape for textures with transparent margins, and `EffectsUseUnscaledTime` keeps the lights moving while the game is paused. Custom effects derive from `ButtonEffect<TEffect>`. A button scrolled out of a scroll viewer (or hidden by any parent that hides its overflow) skips its effects, so long lists of glowing buttons stay fast. `RoundedRectanglePath`, `NineSlice` and the new `TextureFactory` generators (rounded glows and rims, gradients, soft bands) can also be used on their own.

### Glow and shadow
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosGlowAndShadow.png" alt="MonoGame.GameManager samples demo - glow and shadow" width="600" height="400">

Any control can get shadows, glows, outlines and shines that follow its silhouette: the glyphs of a `Label` or a `MultiLineLabel`, the opaque pixels of an `Image`, a `NineSliceImage` or a `SpriteAnimation`. Effects are stacked, so a title can have two shadows, three glows, an outline and a shine at the same time. They follow the scale, the rotation and the opacity of the control, and they can be animated: flicker, pulse, breathe, orbit, cycle colors and shine.

```csharp
using MonoGame.GameManager.Controls.Shading;

// A title with two shadows, three glows, an outline and a shine. Shadows first, outlines last (drawn in this order).
new Label(font, "LEVEL 7", new Vector2(100, 100), Color.White)
    .SetScale(2f)
    .AddShading(new Shadow(new Vector2(0, 10), 10, Color.Black * 0.6f))                    // a soft shadow far below
    .AddShading(new Shadow(new Vector2(0, 4), 0, new Color(120, 30, 160)).SetSpread(3))   // a hard, bold shadow (a 3D lip)
    .AddShading(new Glow(new Color(170, 60, 255), 30).SetBreathe(0.5f, 0.05f))           // a wide halo that breathes
    .AddShading(new Glow(new Color(255, 110, 230), 14).SetPulse(0.8f, 0.4f))             // a pulsing glow
    .AddShading(new Glow(Color.White, 3).SetFlicker(0.3f, 9f))                           // a flickering core
    .AddShading(new Outline(new Color(60, 10, 90), 3))
    .AddShading(new Shine(new Color(255, 240, 255)).SetTiming(0.8f, 3f))                 // a band of light sweeping across
    .AddToScreen();

// Fire: three glows from red to yellow, each flickering at its own pace and rising like flames
new Label(font, "HOT", new Vector2(100, 220), Color.White)
    .AddShading(new Glow(new Color(255, 40, 10), 30).SetOffset(0, -6).SetFlicker(0.5f, 6f).SetBreathe(1.3f, 0.06f))
    .AddShading(new Glow(new Color(255, 120, 20), 15).SetOffset(0, -3).SetFlicker(0.4f, 9f).SetTimeOffset(1.3f))
    .AddShading(new Glow(new Color(255, 210, 90), 4).SetFlicker(0.3f, 13f).SetTimeOffset(2.1f))
    .AddToScreen();

// Animated presets: Gold Title, Fire, Neon, Ice, Magic, Rainbow, Toxic, Plasma, Heartbeat, Ghost, Hologram, Selected, Shiny, Glitch
// Still presets: Soft Shadow, Hard Shadow, Long Shadow, Sticker, Comic, Candy, Emboss, Retro, Double Outline
new Image(iconTexture).AddShadings(ShadingPresets.Sticker()).AddToScreen();
coinAnimation.AddShadings(ShadingPresets.Neon(Color.Gold));
title.SetShadings(ShadingPresets.Create("Gold Title"));
```

| Effect | Default | What it does |
|---|---|---|
| `Shadow` | behind, painted | A copy of the silhouette moved by an `Offset`, soft (`Blur`) or sharp, and grown by a `Spread` for bold shadows. Two shadows of different colors make 3D lips and retro titles. |
| `Glow` | behind, added light | A halo that fades over a `Radius` around the silhouette (`Spread` makes its core thicker). `SetLayer(ShadingLayer.Front)` also draws it over the control (a bloom); `SetBlend(ShadingBlend.Normal)` paints it (a dark aura on light backgrounds). |
| `Outline` | behind, painted | A border of a `Thickness` around the silhouette, crisp or soft (`Softness`). Stack two outlines for a double border. |
| `Shine` | front, added light | A slanted band of light (`Width`, `Angle`) that sweeps across the silhouette every few seconds (`SetTiming(duration, interval)`): gold titles, coins, gems, scan lines. |
| `GradientFill` | content (instead of the control), painted | Paints the silhouette with a gradient of two or more colors (`SetColors`, `SetPositions`), at an `Angle` (90 = top to bottom), over the box of the glyphs (`GradientBounds.Content`) or the whole control, between `SetRange(start, end)`. `SetHardness` makes hard edges (two-tone, stripes); `SetScroll` makes it flow. |
| `InnerShadow` | front, painted | A shadow inside the silhouette, on the edges its `Offset` moves away from (the default offset, down, shades the top of the letters). With a light color and `ShadingBlend.Light` it is an inner highlight. |
| `InnerGlow` | front, added light | A glow inside the silhouette, along all its edges, over a `Radius` (ice, glass, magic rims). |
| `GradientOverlay` | front, painted | A gradient of transparent colors over the silhouette: the glossy band of game titles (`GradientOverlay.Band`, `ShadingPresets.Gloss()`), a darker bottom, gem facets, and stripes or scan lines with `SetRepeat()` (and `SetScroll` to move them). |
| `PatternOverlay` | front, painted | A tiled pattern over the silhouette (`ShadingPattern`: `Noise`, `Dots`, `Checker`, `Diagonal`, `Grid`, `Scales`, `ScanLines`, `Blotches`), sized by `TileSize`, that can move (`SetScroll`): stone and metal grain, polka dots, halftone, dragon scales, sci-fi panels. |
| `Sparkles` | front, added light | Star glints scattered over the glyphs (`Count`, `Size`, `Seed`) that twinkle (`SetTwinkle`); their rays can go a little outside the letters (`Spill`). |

### Casual game titles: gradients, bevels, stacked outlines

`GradientFill`, `InnerShadow` and `InnerGlow` build the titles of casual games. `ShadingPresets.Bevel(highlight, shade, depth, softness)` returns two inner shadows: a light top edge and a dark bottom edge. Outlines stack: add the thickest first.

```csharp
// The golden "Map" of a casual game: a cream to gold to orange gradient, a light bevel, a thick dark brown outline,
// a dark shadow below and a warm glow around (this is ShadingPresets.GoldMap()).
new Label(alfaSlabOne, "Map", new Vector2(200, 100), Color.White)
    .SetScale(3f)
    .AddShading(new Shadow(new Vector2(0, 5), 3, Color.Black * 0.55f).SetSpread(3.5f))
    .AddShading(new Glow(new Color(255, 170, 40), 14).SetIntensity(0.8f))
    .AddShading(new Outline(new Color(75, 32, 6), 3.5f))
    .AddShading(new GradientFill(new Color(255, 252, 220), new Color(255, 222, 100), new Color(242, 150, 22)).SetPositions(0f, 0.45f, 1f))
    .AddShadings(ShadingPresets.Bevel(new Color(255, 255, 240) * 0.85f, new Color(150, 60, 0) * 0.55f, 1.5f, 1.2f))
    .AddToScreen();

// Ice: two stacked outlines, a gradient and an inner glow
new Label(font, "FROZEN", new Vector2(200, 250), Color.White)
    .AddShadings(
        new Outline(new Color(20, 60, 130), 3),
        new Outline(new Color(210, 245, 255), 1.5f),
        new GradientFill(Color.White, new Color(150, 230, 255), new Color(40, 130, 230)),
        new InnerGlow(new Color(200, 245, 255), 3))
    .AddToScreen();

// Many effects inside the letters: a metal fill, grain, a gloss band, a bevel, a light rim and twinkling stars.
new Label(cinzelDecorative, "Nimavora", new Vector2(200, 400), Color.White)
    .AddShadings(
        new Shadow(new Vector2(0, 5), 5, Color.Black * 0.6f).SetSpread(4),
        new Glow(new Color(60, 120, 255), 18),
        new Outline(new Color(12, 16, 50), 5),
        new Outline(new Color(120, 70, 20), 2.5f),
        new GradientFill(new Color(255, 252, 215), new Color(245, 190, 70), new Color(150, 80, 20), new Color(255, 225, 130), new Color(200, 120, 30))
            .SetPositions(0f, 0.42f, 0.5f, 0.62f, 1f).SetHardness(0.3f),
        new PatternOverlay(ShadingPattern.Noise, new Color(120, 60, 0) * 0.25f, 6),
        ShadingPresets.Gloss(0.4f, 0.45f),
        new InnerGlow(new Color(255, 250, 220), 1.5f).SetIntensity(0.7f),
        new Sparkles(Color.White, 12, 20).SetTwinkle(0.6f))
    .AddToScreen();

// Fill presets: Gold Map, Chrome, Ice Crystal, Candy Fill, Emerald, Sunset Fill, Lava (flowing), Holographic (flowing),
// and the layered Fantasy Logo, Arena Title and Hero Title
title.SetShadings(ShadingPresets.Create("Chrome"));
```

The fill is drawn in the `ShadingLayer.Content` layer, instead of the control: the colors of the control do not show, only its silhouette is used. It is rendered into a render target with the other shapes (the silhouette as alpha, the colors of a small gradient texture written over the whole target, so the edges of the glyphs never blend with another color) and kept until the text or the options change; its intensity, pulse and flicker are free, and a scrolling fill is rendered again in small steps. The angle of a fill and the offset of an inner shadow are in the local space of the control: they turn with it. The built-in outline of a label (`Label.SetOutline`) is part of the silhouette, so a fill covers it: use an `Outline` effect with a fill. `GradientOverlay`, `PatternOverlay` and `Sparkles` are rendered the same way and only change when their options change (a scrolling overlay or pattern and twinkling sparkles are rendered again in small steps). The silhouette is captured with the swashes of decorative fonts that go past the measured width of the text. No shader is used, so the effects work the same on DesktopGL and Android.

Every effect has a color (or a `ColorCycle`), an `Intensity` (above 1 makes soft glows and shadows denser) and an `Offset` that keeps its direction on the screen when the control rotates. Animations: `SetPulse` (dims and brightens), `SetFlicker` (smooth noise, like flames and old neon tubes), `SetBreathe` (grows and shrinks), `SetOrbit` (turns around a circle) and `SetColorCycle`; `SetTimeOffset` shifts them so stacked glows do not move in lockstep. The shape of each effect is rendered once into a render target (big blurs at a lower resolution) and drawn every frame until the control changes: its text, texture, frame, size, scale or alpha, or the shape options of the effect. The color, the animations and the rotation cost nothing. A custom control overrides `GetContentSignature` so its shading is not rendered every frame. The effects are drawn when the parent draws the control, and are skipped while a scroll viewer (or any parent that hides its overflow) hides it.

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

### Fonts

Text is drawn with [FontStashSharp](https://github.com/FontStashSharp/FontStashSharp): fonts are TrueType/OpenType files rasterized at runtime, at any size, so there are no `.spritefont` files. Copy the font file to the content folder (in the `.mgcb`: `/copy:Fonts/Roboto-Regular.ttf`) and load it with the content loader. Every text control (`Label`, `MultiLineLabel`, `Button.SetText`, `Checkbox.SetLabel`, `TextBox`, `Tooltip`, `FpsCounter`) takes a `SpriteFontBase`.

```csharp
// One font system per font file (cached and released with the loader); one font per size.
var fonts = Content.LoadFontSystem("Fonts/Roboto-Regular.ttf", "Fonts/NotoSansJP-Regular.ttf"); // the next files are fallbacks
var title = fonts.GetFont(48);
var body = fonts.GetFont(20);

// Or directly a size of a font file.
var small = Content.LoadFont("Fonts/Roboto-Regular.ttf", 14);
```

Scaled text stays sharp: a label scaled to 2 is drawn with the font rasterized at twice its size (the layout is still measured with the font of the control). Set `FontScaling.CrispScaling = false` to stretch the glyphs instead, and `FontScaling.MaxFontSize` to limit the sizes rasterized. Characters the fonts do not have are drawn with `FontSystem.DefaultCharacter`. `Label.SetOutline(color, thickness)` draws an outline around the text (see [Label](#label)).

### Label
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosLabel.gif" alt="MonoGame.GameManager samples demo - labels" width="600" height="400">

```csharp
var scoreLabel = new Label(font, "Score: 0", new Vector2(20, 20), Color.Yellow)
    .SetAnchor(Anchor.TopRight) // the position of right and bottom anchors points inwards
    .AddToScreen();
scoreLabel.SetText("Score: 100");

// An outline around the glyphs, in font pixels: it grows with the scale and takes any color.
new Label(font, "GAME OVER", new Vector2(0, 0), Color.Yellow)
    .SetOutline(Color.Black, 3)
    .SetCharacterSpacing(4) // letter spacing, in font pixels (negative values bring the letters closer)
    .SetScale(2f)
    .SetAnchor(Anchor.Center)
    .AddToScreen();
```

`CharacterSpacing` is part of the size of the label and grows with its scale. `MultiLineLabel` has the same `SetCharacterSpacing` (the lines are wrapped with it) and `SetOutline`. For gradients, bevels and stacked outlines, see [Casual game titles](#casual-game-titles-gradients-bevels-stacked-outlines).

The fonts loaded with `LoadFontSystem`/`LoadFont` rasterize the outline once per size and thickness (round, any color, one draw). For a `FontSystem` created by hand, pass `TextOutline.CreateFontSystemSettings()` to get the same; otherwise the outline is drawn by repeating the text around its position.

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

### Slider
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosSlider.gif" alt="MonoGame.GameManager samples demo - Slider: options, step, orientation, textures, rotation and examples" width="600" height="400">

A `Slider` chooses a value between a minimum and a maximum by dragging its thumb. Pressing anywhere on the track moves the thumb there, and the drag goes on when the pointer leaves the slider. It is horizontal or vertical (the minimum at the bottom), rounds the value to a step, keeps working when it is rotated or scaled, and is drawn with colors or textures.

```csharp
var volume = new Slider(new Vector2(20, 20), new Vector2(240, 24))   // from 0 to 1 by default
    .SetValue(Audio.MusicVolume, notify: false)                       // no callback for the initial value
    .AddOnValueChanged(value => Audio.MusicVolume = value)
    .AddToScreen();

var level = new Slider(new Vector2(300, 20), new Vector2(28, 200), 0, 100)
    .SetOrientation(Orientation.Vertical)
    .SetStep(10)                                                      // 0, 10, 20... 100
    .SetColors(track: Color.DimGray, fill: Color.Orange, thumb: Color.White)
    .SetThumbSize(new Vector2(32))
    .SetTrackThickness(10)
    .AddToScreen();

level.SetThumbTexture(starTexture).SetTrackTexture(trackTexture);     // textures instead of colors
level.IsEnabled = false;                                              // half transparent, ignores the pointer
```

`Value`, `NormalizedValue` (from 0 to 1), `Minimum`, `Maximum`, `Step` and `IsDragging` describe the state, and `SetRange` changes the limits. Without a texture the thumb is round or square (`IsThumbRound`). The Slider demo shows every option on a live preview with three examples: a color mixer, a stepped volume fader and a slider that resizes an image. The other demo pages use sliders for their numeric options.

### Input controls

```csharp
new Checkbox(new Vector2(20, 20))
    .SetLabel(font, "Full screen")
    .AddOnCheckedChanged(isChecked => ServiceProvider.GameWindowManager.SetFullScreen(isChecked))
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

## Physics, state machines and pools

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
```

## Particles
<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosParticles.gif" alt="MonoGame.GameManager particles playground: presets and live options" width="600" height="400">

A `ParticleEmitter` is a control that simulates and draws a `ParticleSystem`. Start from one of the `ParticlePresets` (Fire, Smoke, Explosion, Sparks, Rain, Snow, Confetti, Fireworks, Magic, Fountain, Bubbles, Fireflies, Vortex and Stars) or describe the effect with a `ParticleSettings`:

```csharp
var campfire = new ParticleEmitter(ParticlePresets.Fire()).SetPosition(400, 500).AddToScreen().Play();

ParticleEmitter.Spawn(ParticlePresets.Explosion(), enemy.PositionAnchor); // removed from the screen when it ends

var sparks = new ParticleEmitter(new ParticleSettings
{
    SpeedMin = 80, SpeedMax = 260,
    LifetimeMin = 0.3f, LifetimeMax = 0.8f,
    StartColor = Color.Orange, EndColor = Color.Transparent,
    Gravity = new Vector2(0, 400), Size = 6
}).SetPosition(400, 300).AddToScreen();
sparks.Burst(60);
```

The presets return new settings that can be changed before or after creating the emitter (`ParticlePresets.Create("snow")` creates them by name), and the settings are read every frame, so an effect can be tuned while it runs. Every value with Min and Max is chosen at random for each particle.

<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosParticlePresets.png" alt="The 14 particle presets: Fire, Smoke, Explosion, Sparks, Rain, Snow, Confetti, Fireworks, Magic, Fountain, Bubbles, Fireflies, Vortex and Stars" width="600" height="546">

```csharp
var magic = new ParticleSettings
{
    Shape = EmitterShape.Ring, SpawnInnerRadius = 4, SpawnRadius = 18, RadialVelocity = true,
    SpeedMin = 10, SpeedMax = 40, LifetimeMin = 0.8f, LifetimeMax = 1.6f,
    Appearance = ParticleShape.Star, Size = 12, ScaleVariation = 0.4f,
    BlendState = ParticleResources.AdditiveBlendState,                   // lights, fire and magic add up
    Colors = { Color.Violet, Color.Cyan, Color.White },                   // a palette: one color per particle
    StartColor = Color.White, EndColor = Color.White,
    ScaleOverLifetime = ParticleCurve.FadeInOut(0.2f, 0.5f),             // grows, then shrinks
    AlphaOverLifetime = ParticleCurve.Blink(2),                           // twinkles
    RotationSpeedMin = -180, RotationSpeedMax = 180,
    Turbulence = 40,                                                      // wanders
    EmissionPerDistance = 0.4f                                            // also emits while the emitter moves
};
var wand = new ParticleEmitter(magic).SetPosition(100, 100).AddToScreen().Play();
wand.SetPosition(pointerPosition); // the stars stay behind, a trail follows the pointer
```

The emission has a timeline: `Duration` (0 = forever), `Loop`, `StartDelay`, `PrewarmSeconds` (the effect is already running when it appears) and `Bursts`, groups of particles emitted at a given time. Sub-emitters emit particles from the particles: `OnDeath` when they die, `Trail` while they live.

```csharp
var rocket = new ParticleSettings
{
    Duration = 1f, Loop = true, EmissionRate = 0,                         // one rocket per second...
    Bursts = { new ParticleBurst(0f, 1) },
    AngleMin = -100, AngleMax = -80, SpeedMin = 450, SpeedMax = 550, Gravity = new Vector2(0, 250),
    LifetimeMin = 1.2f, LifetimeMax = 1.5f,
    Appearance = ParticleShape.Glow, Size = 10, BlendState = ParticleResources.AdditiveBlendState,
    Colors = { Color.Red, Color.Gold, Color.Cyan }, StartColor = Color.White, EndColor = Color.White,
    Trail = new ParticleSubEmitter(ParticlePresets.Smoke()) { Rate = 40 },                                   // ...with a smoke trail...
    OnDeath = new ParticleSubEmitter(ParticlePresets.Explosion()) { CountMin = 100, CountMax = 150, InheritColor = true } // ...that explodes in its color
};
new ParticleEmitter(rocket).SetPosition(640, 700).AddToScreen().Play();
```

An emitter is an `IPlayable`: `Play` starts the emission (and restarts a finished one), `Stop` stops it while the living particles finish their life, `Pause` and `Resume` freeze everything, `Reset` removes every particle and rewinds the timeline, `Restart` does both. `Burst(count)` emits at once at the control, `Burst(count, position)` anywhere (eg: a pointer position). When the emission ends and the last particle dies the emitter is `IsComplete`, raises `AddOnCompleted` and, with `SetRemoveWhenCompleted(true)`, leaves the screen by itself; `ParticleEmitter.Spawn` and `SpawnBurst` create such one-shot effects in one line.

| Group | Settings |
|---|---|
| Emission | `MaxParticles`, `EmissionRate`, `Duration`, `Loop`, `StartDelay`, `PrewarmSeconds`, `Bursts`, `EmissionPerDistance`, `InheritVelocity`, `SimulationSpace` (`World`: the particles stay where they were born; `Local`: they move, rotate and scale with the emitter) |
| Shape | `Shape` (`Point`, `Circle`, `Ring`, `Rectangle`, `Line`), `SpawnRadius`, `SpawnInnerRadius`, `SpawnSize`, `SpawnRotation`, `EmitFromEdge`, `RadialVelocity` |
| Initial values | `LifetimeMin/Max`, `SpeedMin/Max`, `AngleMin/Max` (degrees, 0 = right, 90 = down), `RotationMin/Max`, `RotationSpeedMin/Max`, `ScaleVariation`, `Colors`, `RandomFlip` |
| Over the lifetime | `StartColor`/`EndColor` or `ColorOverLifetime` (a `ParticleGradient`), `StartScale`/`EndScale` or `ScaleOverLifetime` (a `ParticleCurve`), `AlphaOverLifetime`, `ColorEasing`, `ScaleEasing` |
| Forces | `Gravity`, `Drag`, `Turbulence` and `TurbulenceFrequency`, `AttractionPoint` and `AttractionStrength` (negative repels), `VortexStrength`, `Floor`, `Bounds`, `BoundsMode` (`None`, `Kill`, `Bounce`), `Bounciness`, `Friction` |
| Drawing | `Texture` or `Textures` (one at random), `Frames` of a sprite sheet with `FrameMode` and `FrameRate`, `Appearance` (`Square`, `Circle`, `Glow`, `Ring`, `Star`, `Diamond`) and `Size` when there is no texture, `BlendState`, `AlignToVelocity`, `VelocityStretch` |
| Sub-emitters | `OnDeath`, `Trail` (`ParticleSubEmitter`: settings, count, probability, rate, inherited velocity and color) |

`ParticleCurve` (`Linear`, `Constant`, `FadeInOut`, `Peak`, `Blink`, `FromEasing` or `AddKey`) and `ParticleGradient` (`FromColors`, `Fade` or `AddStop`) describe values along the life of a particle, from 0 (born) to 1 (dead). The shape textures and `ParticleResources.AdditiveBlendState` are shared and released with the screen manager. A `ParticleSystem` can also be simulated without a control (`Scheduler.Add(system)`) and drawn by your own code with `Particles`, `ActiveCount`, `GetColor`, `GetDrawScale`, `GetRotation` and `GetSourceRectangle`.

Try everything live in the Particles demo: the playground above changes every setting of the presets, and its scenes screen combines emitters with other controls, animations and timers.

<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosParticleScenes.gif" alt="MonoGame.GameManager particle scenes: campfire, fireworks, rain, confetti, magic cursor, fountain, portal and spaceships" width="600" height="400">

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

The samples use the Roboto font (Apache 2.0), copied to the content folders. The demos also use display fonts (license files next to them) under the SIL Open Font License: Alfa Slab One, Lilita One, Bangers, Press Start 2P, Audiowide, Creepster, Pacifico, Rye, Black Ops One, Monoton, Cinzel Decorative, Bungee, Kaushan Script and UnifrakturMaguntia, plus Luckiest Guy, Permanent Marker and Special Elite under the Apache 2.0 license. The content is built by the MonoGame content builder, a local .NET tool. Restore it once, from the repository root, before the first build:

```
dotnet tool restore
dotnet run --project Samples/Demos/MonoGame.GameManager.Samples.WinExe
```

<img src="https://raw.githubusercontent.com/DaniloPeres/MonoGame.GameManager/main/Samples/Demos/SamplesDemosMainScreen.gif" alt="MonoGame.GameManager samples demo main screen" width="600" height="394">

The Particles tile opens a playground: 14 presets, every setting editable live (emission, shape, motion, look, color and timeline), click to burst, right click to move the emitter, a randomizer, and a scenes screen that combines emitters with other controls: a campfire, fireworks, a rainy day, a confetti cannon, a magic cursor, a fountain, a portal and spaceships.

The Slider tile shows every option of the slider on a live preview, with a color mixer, a stepped volume fader and a slider that resizes an image. The other pages use sliders for their numeric options (scale, rotation, opacity, durations, speed, zoom limits).

The Button Effects tile opens a preview button where every effect can be changed live on seven scrolling pages: Button, Text, Glow, Inner, Borders, Particles and Motion.
- The Button page has the 29 presets, a switch that turns the animations off, a light strength, the size, the corner radius, the colors of the button and of the stage, textures, scale, rotation, opacity, the enabled state and the game speed.
- The Text page has the text, its size and color, the border of the label (`Label.SetOutline`: thickness and color) and the outline and shadow of `TextOutlineEffect`.
- The other pages hold the options of each effect.
- Two randomizers make animated or calm combinations.

The Glow & Shadow tile opens a preview where shadows, glows and outlines can be added, removed and changed live on a text, a multi-line text, an image, a nine-slice image or a sprite animation.
- The Target page has the 23 presets, the target, its text, image, color, scale, rotation and opacity, the color of the stage and the game speed.
- The Effects page lists every effect of the target, each with its own options, animations (pulse, flicker, breathe, orbit, color cycle) and a switch. The add buttons create up to 2 shadows, 3 glows, 2 outlines and a shine, so a text can have all of them at the same time, or any of them alone.
- A scrolling gallery of 51 looks in four sections can be clicked to edit them: animated ones (a flickering neon sign, fire with three glows, a shining gold title, breathing ice, a rainbow, orbiting plasma, magic, toxic, a heartbeat, lightning, a sunset, a glitch), still ones (comic, candy, retro, emboss, long shadow), images and sprites (a shining golden star, a shiny coin, a burning torch, a plasma orb, a gem, a hologram with a scan line, stickers, a card), and a section of shadows and outlines without animation (drop, hard, floating and dark aura shadows, extruded 3D, letterpress, pop art, thin, thick, soft and triple outlines, outlined icons, sprites and stamps).

A scrolling gallery shows 124 buttons in three sections: 27 rich static buttons, 60 glossy, candy and cartoon buttons, and 37 animated ones. They use every preset and some hand-made combinations (VIP, Mythic, Frozen, Soft UI, Wood, Comic) on buttons of many sizes, shapes and colors, and a toggle button whose lights turn on with it.

The Text Effects tile opens a preview of a title whose text, font (Roboto and 17 display fonts: slab serif, rounded, comic, pixel, sci-fi, horror, script, western, stencil, neon tubes, marker, typewriter, blackletter...), size, rotation, opacity, letter spacing and stage color can be changed live, with a page per effect: Fill (gradient palettes, top, middle and bottom colors, angle, range, hardness, bounds and scroll), Outline (three stacked outlines), Inner (bevel, inner shadow and inner glow), Detail (three gradient overlays as gloss bands or stripes, a pattern and sparkles), Shadow (two), Glow (two), Shine and Motion (pulse, flicker, breathe and color cycles, an orbiting shadow, a moving bevel light, the game speed).
- Reset goes back to the selected style, Random picks a style, a text, a font and a stage, and Plain text removes every effect.
- A scrolling gallery of 205 styles in thirteen sections can be clicked to edit them. It opens with 50 layered titles that stack many effects inside the letters: 29 still ones (a fantasy gold logo with grain, rim and star glints, chunky arena and hero titles with gloss bands, a crown title, candy with polka dots, jelly, chrome with stripes, a racing checker, dragon scales, carved stone, wood grain, lava rock, a glacier, a sci-fi panel grid, a retro TV with scan lines, comic halftone, zebra stripes, marble, camouflage, cut gems, bronze, bubblegum, a golden plaque, spooky mist and toy plastic) and 21 animated ones (twinkling stars, moving casino lights, scrolling candy stripes, flowing magma, a hacker screen, a holographic grid, a cosmos, arcane runes, electric stripes, waves, treasure, a rainbow party, toxic bubbles, a royal shine, a disco checker, legendary loot and fairy dust). Then come 40 static ones (the golden "Map", silver, bronze, ruby, emerald, sapphire, ice, fire, poison, purple magic, candy, cartoon, comic pop, steel, chocolate, wood, stone, horror, retro 8-bit, sci-fi, rainbow, sunset, ocean, two-tone, embossed, pastel, VICTORY, DEFEAT, LEVEL UP, +100, COMBO x3, damage and critical hit numbers, heal, menu titles, a sticker and a boss fight) and 15 animated ones (a neon sign, blaze, lava, holographic, party, shiny gold, love, chill, toxic, spell, glitch, start, a moving light, a ghost and a victory parade), and 100 themed ones: Fantasy & RPG (dragon, elven, rune stone, legendary, epic, rare and common loot, quest complete, mana, health, kingdom, grimoire), Arcade & Retro (insert coin, high score, 1UP, neon marquee, synthwave, 80s chrome, vaporwave, pixel hero, press start, pinball, on air, 8-bit stripes), Cartoon & Comic (bubble, kaboom, zap, splat, wham, barber stripes, sugar rush, kids, toon shadow, doodle, marker note, smash), Horror & Dark (zombie, haunted, vampire, cursed, bones, hex, graveyard, nightmare, abyss, blood moon), Sci-fi & Neon (cyberpunk, laser, plasma core, warp, hologram, galaxy, neon signs, terminal, hazard stripes, mech, energy), Materials & Nature (marble, copper, platinum, obsidian, amber, jade, magma, waves, grass, snow, desert, forest, embers, crystal), Western & Vintage (wanted, saloon, sheriff, army, stencil stamp, typewriter, newspaper, telegram, motel sign, circus), Elegant & Script (wedding, royal, signature, luxury, calligraphy, brush, champagne, ink) and Seasons & Holidays (Christmas, candy cane, Halloween, spring, summer, autumn, winter, Valentine, new year, Easter).
- The tiles get their effects only near the visible part of the gallery and release them (and their render targets) when scrolled far away.

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

## Migrating from 2.x

Version 3 renders text with FontStashSharp instead of `SpriteFont`:

- Replace the `.spritefont` files with TrueType/OpenType files copied to the content folder (`/copy:Fonts/MyFont.ttf` in the `.mgcb`).
- `LoadSpriteFont("Fonts/Main")` is gone: use `LoadFont("Fonts/MyFont.ttf", size)` or `LoadFontSystem("Fonts/MyFont.ttf").GetFont(size)`.
- The text controls take a `FontStashSharp.SpriteFontBase`; their `SpriteFont` property and `SetSpriteFont` are now `Font` and `SetFont`.
- Text is no longer flipped by `SpriteEffects`.

## Migrating from 1.x

Most 1.x code keeps compiling: renamed members still exist and are marked `[Obsolete]` with the new name. The main changes are:

- Reference a MonoGame platform package in your game (the library no longer brings `MonoGame.Framework.DesktopGL`).
- `ServiceProvider.ContentLoaderManager` is now `ServiceProvider.ContentLoader`, `Screen.ContentLoader` is now `Screen.Content`, and the memory manager is gone (assets are released with the screen that loaded them).
- `FadeAnimation` is replaced by `OpacityAnimation`, `EaseAnimation` by `MoveAnimation`, and `ResetAnimation` by `Reset`.
- `ITransition` has two methods, `TransitionOut` and `TransitionIn`.

[CHANGES.md](https://github.com/DaniloPeres/MonoGame.GameManager/blob/main/CHANGES.md) has the complete list.

## License

MIT
