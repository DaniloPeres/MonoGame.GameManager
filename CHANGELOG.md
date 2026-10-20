# Changelog

## [3.1.0] (unreleased)

### Added
- A video player. `VideoScreen` plays a video of the content folder and goes on when it ends (`Skippable`, `IsMuted`, `ScaleMode` Fit/Fill/Stretch, `ContentOffset`, `BackgroundColor`; override `OnFinished` to open the next screen). The core library only has the abstractions (`IVideoPlayer`, `IVideoPlayerFactory`, `VideoLayout`, `ServiceProvider.Video`); a platform without a registered player plays nothing and the screen ends at once.
- `MonoGame.GameManager.DesktopGL`: the LibVLC player (`LibVlcVideoPlayerFactory`).
- `MonoGame.GameManager.Android`: the Android `MediaPlayer` player (`AndroidVideoPlayerFactory`), shown over the activity without detaching the game view.
- `ScreenManagerSettings.ContentStreamFilter`: wraps every stream of `IContentLoader.OpenStream` (global and screen loaders), eg to decrypt content files; `ContentLoader` takes it as `streamFilter`.
- Shading effects for casual game titles: `GradientFill` paints the silhouette of a control with a gradient of two or more colors (positions, angle, range, hardness, glyph or control bounds, scrolling), drawn in the new `ShadingLayer.Content` layer instead of the control; `InnerShadow` (a shadow inside the silhouette, an inner highlight with `ShadingBlend.Light`) and `InnerGlow` (a glow inside its edges). They are rendered with render targets and blend states, without shaders, so they work the same on DesktopGL and Android.
- `ShadingPresets.Bevel(highlight, shade, depth, softness)` (a light top edge and a dark bottom edge), and the fill presets Gold Map, Chrome, Ice Crystal, Candy Fill, Emerald, Sunset Fill, Lava and Holographic (`ShadingPresets.FillNames`).
- Effects painted inside the glyphs: `GradientOverlay` (a gradient of transparent colors: gloss bands with `GradientOverlay.Band` and `ShadingPresets.Gloss()`, darker bottoms, gem facets, stripes and scan lines with `SetRepeat()`, moving with `SetScroll`), `PatternOverlay` (tiled `ShadingPattern`s: noise, dots, checker, diagonal lines, grid, scales, scan lines and blotches, that can move) and `Sparkles` (star glints that twinkle, with rays that can spill a little outside the letters). `GradientFill` and `GradientOverlay` share the new `GradientEffect<TEffect>` base (with `Repeat`). Layered presets: Fantasy Logo, Arena Title and Hero Title.
- The shading captures the swashes of decorative fonts that go past the measured width of the text (labels report the box of their drawn glyphs).
- `Label.CharacterSpacing` / `SetCharacterSpacing` (letter spacing in font pixels, part of the size, kept when the text is rasterized at a bigger size) and `MultiLineLabel.CharacterSpacing` (the lines are wrapped with it). `TextOutline.Draw` and `TextOutlineEffect` follow the spacing.
- `MultiLineLabel.SetOutline(color, thickness)` (`OutlineColor`, `OutlineThickness`): the outlines of every line are drawn before the texts, so they never cover the line above.
- `ScalableControlAbstract.ShadingMargin` is public (how far the shading effects reach outside the control), and controls can override `GetContentBounds` (the box of what they draw; a label returns the box of its glyphs).
- `TextureFactory.CreateGradient` / `CreateGradientPixels`: a horizontal gradient of several colors.
- Shading shapes are rendered again one by one: when only an effect changed (or a fill scrolls), the other shapes of the control are kept. Very thick outlines are grown at a lower resolution instead of lowering the resolution of every shape of the control, so fills and glows stay sharp.
- Demos: the "Text Effects" page: a title whose text, font, size, rotation, opacity, letter spacing and stage can be changed live, with pages for the fill, three outlines, bevel, inner shadow and glow, shadows, glows, shine and motion, Reset and Random buttons, a Detail page (gradient overlays, patterns and sparkles), and a gallery of 205 styles clicked to edit them: 50 layered titles with many effects inside the letters (29 still, 21 animated), 40 static, 15 animated and 100 in nine themed sections (Fantasy & RPG, Arcade & Retro, Cartoon & Comic, Horror & Dark, Sci-fi & Neon, Materials & Nature, Western & Vintage, Elegant & Script, Seasons & Holidays). The tiles get their effects only near the visible part of the gallery. The Label page has a letter spacing option.
- Demos: 17 display fonts, loaded by `ContentHandler.Fonts` (licenses next to them): Alfa Slab One, Lilita One, Bangers, Press Start 2P, Audiowide, Creepster, Pacifico, Rye, Black Ops One, Monoton, Cinzel Decorative, Bungee, Kaushan Script and UnifrakturMaguntia, plus Luckiest Guy, Permanent Marker and Special Elite under the Apache 2.0 license.

### Performance
- `Control.SubpixelPositioning` / `SetSubpixelPositioning`: the control is drawn at its exact position instead of the whole
  pixel of its destination rectangle (images, circles, the local drawing helpers and the shading follow), so a slow float
  or an orbit glides instead of hopping. Off by default (still controls stay sharp).
- `ContainerAbstract.RenderCacheRate` / `RenderCacheMargin`: above twice that frame rate, the children of a container are
  drawn into a render target at most that many times per second and the container draws it in between (one draw call for
  a whole card or panel); slower (60 FPS on a phone) the children are drawn as usual. The target is rounded up, so a
  container that pops in keeps it.
- The children of a scroll viewer that are out of sight (with `ContainerAbstract.ScrolledChildCullMargin` around them) are
  not drawn: a long list no longer costs the draw calls of every card.
- `VisualQuality.ButtonEffectRefreshRate` (60): the animated inside layer of a button (sweeps, twinkling particle fields,
  ripples) is rendered again at most that many times per second, instead of at every frame of an unlocked frame rate.
- `VisualQuality` (static): knobs a game sets from its effects quality setting. `ParticleEmissionScale` (0 to 1) scales every particle emitted, the fractions carried over; `DecorativeButtonEffects` hides the animated lights that only decorate (`ButtonEffect.IsDecorativeAnimation`: sparkles, particle fields, running lights, glints, light rays, shine sweeps); `ButtonPulses` stops the pulses, so the inside layers of the buttons stay in their render targets.
- Far fewer render target switches (slow on phone GPUs): a typical screen goes from 15-45 per frame to 2-5, a busy one
  from about 115-150 to 4-13, with the same look.
  - The inside layer of a button (shine sweeps, ripples, shades...) is kept in its render targets while nothing it draws
    changes, instead of being rendered again every frame: `ButtonEffect.GetDrawKey` (null by default: rendered every frame;
    the effects of the library override it).
  - Containers (`Panel`, buttons without light effects) have a content signature built from their children, so their
    shading effects are no longer rendered again every frame (`ContainerAbstract.GetOwnContentSignature`).
  - Shading shapes are not rendered again when only the scale changes (breathing, pop-ins), when the intensity above 1 or
    the opacity moves (kept in steps of 0.25 and 1/16, the two nearest blended), or when a circle or a rectangle changes
    size a little (`ScalableControlAbstract.ShadingStretchesWithSize`: shapes stretched within 6%).
- Fewer sprite batches: shading shapes are premultiplied and drawn in the batch of the controls (lights with an alpha of 0),
  additive particles too, and `ControlManager.PushState` / `PopState` keep the batch when the state does not change.
- Button effects are drawn in the sprite batch of the screen when it uses the alpha blend and the button has the same scale
  on both axes: the `ButtonEffectContext` places the sprites (`Draw`, `DrawNineSlice`, `FillRectangle`) and draws the
  lights with an alpha of 0 (the same as the light blend). Effects that draw with the batch itself return false from
  `ButtonEffect.DrawsWithContext` and keep a batch of their own (`TextOutlineEffect`).
- The small generated images of the effects (light shapes, gradients, bands, frames up to 256 pixels) share an atlas, so
  the effects of many buttons are drawn without switching textures: `EffectSprite`, `ButtonEffectResources.GetShapeSprite`,
  `GetVerticalGradientSprite`, `SoftBandSprite`, `WhitePixel`; `LightFrame.Sprite`. A screen of shop cards goes from about
  640 draw calls to about 320.
- The inside layer of a button uses one render target instead of two (the lights add their color over the paint and keep
  its alpha), composited with one sprite.
- `ControlManager.CurrentState` returns a reference (no copy of the state each time it is read) and the `With...` methods of
  `SpriteBatchState` are `readonly`.
- After a batch with a custom `Effect`, `ControlManager` unbinds the textures it left in the slots above 0
  (`ControlManager.UnbindExtraTextures`): MonoGame sets the sampler state of a bound texture again at every draw call when
  it is drawn with another sampler state in slot 0 (eg: the white pixel used as a default mask).
- `Screen.IsOpaque`: an opaque screen hides the screens under it (they are not drawn).
- `ServiceProvider` lookups no longer take a lock.
- `ShadingEffect.Strength` (0 to 1): a multiplier applied when the effect is drawn, free to animate.
- `ScreenManager.Update` no longer copies the list of screens every frame.

### Breaking changes
- `ButtonEffectResources.GetBand` returns an `EffectSprite` (the band may be in the atlas): draw it with its `Source`.
- Code that draws a `LightFrame` by hand must use `LightFrame.Sprite.Source` (the frame may be a part of an atlas page).
- `ControlManager.CurrentState` is a `ref readonly` property.

### Changed
- `ScrollViewer` flings follow the speed of the pointer: a faster move scrolls faster and farther. `MaxInertiaSpeed` is a property (12000 by default instead of a fixed 3000) and `FlingBoost` (0.6 by default) raises the speed of fast flicks; `FlingBoost = 0` and `MaxInertiaSpeed = 3000` give the old feel.

### Fixed
- The glow of a circle or rectangle that grows (`ShadingStretchesWithSize`) was stretched from the size of the last shape
  rendered, not its own: while a ring grew and faded, the glows of its other opacity levels were drawn up to 6% too small,
  so the ring seemed to shake and shrink. Each shape now keeps the size it was rendered at.

## [3.0.0] (2026-10-07)

Text is rendered with FontStashSharp: TrueType/OpenType fonts at any size, sharp when scaled, without `.spritefont` files.

### Fixes
- `TextBox` receives typed text again on DesktopGL with MonoGame 3.8.1+: `GameWindow.TextInput` passes a struct (`TextInputEventArgs`), so the input bridge now builds its handler with an expression instead of binding a method that takes `EventArgs` (which failed silently).

### Breaking changes
- `SpriteFont` is no longer supported. `Label`, `MultiLineLabel`, `TextBox`, `FpsCounter`, `Tooltip.Attach`, `Button.SetText` and `Checkbox.SetLabel` take a `FontStashSharp.SpriteFontBase`; the `SpriteFont` properties and `SetSpriteFont` are renamed `Font` and `SetFont`.
- `IContentLoader.LoadSpriteFont` is removed.
- Text is no longer flipped by `SpriteEffects`.
- `TextBox` accepts every printable character (the characters a font does not have are drawn with `FontSystem.DefaultCharacter`).

### New features
- `IContentLoader.LoadFontSystem(params string[])` loads and caches a FontStashSharp font system from font files copied to the content folder (the next files are fallbacks), released with the loader; `LoadFont(path, size)` returns one size of it.
- `FontScaling`: scaled text is drawn with the font rasterized at its final size, so it stays sharp (`CrispScaling`, `MaxFontSize`).
- `Label.SetOutline(color, thickness)` (`OutlineColor`, `OutlineThickness`): an outline around the glyphs, in font pixels, that grows with the scale and the rotation of the label.
- `TextOutline`: round outlines of any color and thickness rasterized by a FontStashSharp glyph renderer (cached per size and thickness, one draw), used by the font systems of `LoadFontSystem` (`TextOutline.CreateFontSystemSettings()` for font systems created by hand; others repeat the text around its position). `TextOutlineEffect` uses it.
- Demos: the Label page has outline color and thickness options, and its scale goes up to 5.
- Demos: the Button Effects page has a Text tab (text, size, color, label border and the outline and shadow of `TextOutlineEffect`), and the buttons of its gallery have texts of many colors, many of them with a border.
- Demos: the "1250 Buttons" page shows 1250 numbered buttons in five color families of 250 (purple, blue, green, red and gold), each with its own combination of effects (the presets painted with the colors of the family, then auras, frames, inner lights, particles and animated lights) and a text with a shadow and a glow. Buttons jump to each family; a click shows the recipe of a button and marks it as a favorite; only the rows near the visible area are created.
- The samples use the Roboto font (Apache 2.0) copied to their content folders, instead of `.spritefont` files built from fonts installed on the machine.

## [2.2.0] (2026-10-07)

Effects for the buttons, static and animated, and shadows, glows and outlines for texts and images, with demo pages.

### Improvements
- `Button` and `ToggleButton` get a `CornerRadius`: the background color and the border are drawn rounded, at any size.
- `NineSliceImage` uses the new `NineSlice` helper; `ScalableControlAbstract` can draw nine-slice textures in local space (`DrawLocalNineSlice`).
- `TextureFactory.RoundedRectangleDistance` is public.
- Containers and camera panels draw their children with their shading effects; a camera panel keeps drawing a child while its shadow or glow can be seen. Controls can override `GetContentSignature` so their shading is rendered again only when what they draw changes (done for `Label`, `MultiLineLabel`, `Image`, `NineSliceImage`, `SpriteAnimation`, `TiledImage`, `RectangleControl` and `CircleControl`).

### New features
- Button effects (`MonoGame.GameManager.Controls.Effects`): `GlowEffect` (outer halo, inner glow and rim), `GlossEffect`, `ShineSweepEffect`, `LightFillEffect`, `ClickRippleEffect`, `CornerLightEffect`, `RunningLightEffect`, `SparkleEffect` and `LightRaysEffect`. They are added with `AddEffect`, `AddEffects` and `SetEffects`, drawn behind, inside (clipped to the shape of the button, `EffectMask`) or in front of the button, and follow its size, corner radius, scale and rotation. Every effect has a color or a `ColorCycle`, an intensity, a pulse, a time offset and an intensity per visual state (`VisibleOnHover`); custom effects derive from `ButtonEffect<TEffect>`.
- Static effects, without movement: `HighlightEffect` (band, strip, oval or bottom reflection), `GlintEffect` (reflection dots), `InnerShadeEffect` (bottom, top, edges or full), `OutlineEffect` (outside or inside, stackable), `DepthEffect` (3D lip), `DropShadowEffect` and `TextOutlineEffect` (outline and shadow under the text). An inner `GlowEffect` can light only the top or the bottom edge (`GlowEdges`).
- `ButtonEffectBlend`: lights are added (`Light`), shades, outlines, lips and shadows are painted (`Normal`); any effect can switch with `SetBlend`. `UseButtonColor(shade)` makes an effect use a darker or lighter version of the background color of the button.
- `FrameEffect`: borders made of several rings (levels) of colors and thicknesses, around the button or inside its edge, with the `Gold`, `Silver`, `Bronze`, `Gem`, `ButtonColor`, `Dark` and `Candy` styles. `ParticleFieldEffect`: still particles scattered inside the button, in its upper or lower half, above, under, around or on its edge, from a seed; they can twinkle or float up. `HighlightEffect` is painted by default and fades towards the middle of the button (`Fade`), with round ends that keep their shape at any size.
- `ButtonEffectPresets`: the rich static presets Ornate, Starlight, Treasure, Crystal, Galaxy, Fairy and Sunburst (a lot of glow and particles, nothing moves), the static presets Candy, Jelly, Glossy, Cartoon, Bubble, Pearl, Soft Glow, Inner Light, Framed and Minimal (`StaticNames`), and the animated presets Gold, Royal, Magic, Ice, Fire, Emerald, Neon, Rainbow, Legendary, Glass, Alert and Subtle.
- `ButtonEffectResources`: the light and mask blend states, the light shapes and the generated frames (glows, rims, rounded masks, borders, soft fills and glossy bands) cached per corner radius and resolution, the least recently used ones released when the cache is full. The effects of a button hidden by a parent that hides its overflow (eg: scrolled out of a scroll viewer) are not drawn.
- `RoundedRectanglePath` walks the outline of a rounded rectangle by distance; `NineSlice` draws any texture with nine-slice scaling.
- `TextureFactory` generates rounded rectangle glows, rims and soft fills, vertical gradients and soft bands.
- Shading effects for any control (`MonoGame.GameManager.Controls.Shading`): `Shadow` (offset, blur, spread), `Glow` (radius, spread, added light or painted, behind or in front as a bloom), `Outline` (thickness, softness) and `Shine` (a slanted band of light that sweeps across the silhouette). They follow the silhouette of the control (the glyphs of a text, the opaque pixels of an image, a nine-slice image or a sprite animation), are stacked with `AddShading`, `AddShadings` and `SetShadings`, and follow its scale, rotation and opacity. Every effect has a color or a `ColorCycle`, an intensity (above 1 makes soft edges denser), an offset that keeps its direction on the screen, and animations that cost nothing: `SetPulse`, `SetFlicker`, `SetBreathe`, `SetOrbit` and `SetTimeOffset`. The shapes are rendered once into render targets (downsampled for big blurs) and drawn every frame until the control changes.
- `ShadingPresets`: the animated presets Gold Title, Fire, Neon, Ice, Magic, Rainbow, Toxic, Plasma, Heartbeat, Ghost, Hologram, Selected, Shiny and Glitch (`AnimatedNames`), and the still presets Soft Shadow, Hard Shadow, Long Shadow, Sticker, Comic, Candy, Emboss, Retro and Double Outline. `ShadingResources` holds the blend states and the scratch render targets.
- Demos: a Glow & Shadow tile with a live preview of texts and images where up to 2 shadows, 3 glows, 2 outlines and a shine can be added, removed, changed and animated, the presets, and a gallery of 51 looks in four sections: animated glows and shines, still texts, images and sprites, and shadows and outlines without animation.
- Demos: a Button Effects tile with a live preview of every effect and option on six scrolling pages, a switch that turns the animations off, a light strength, animated and calm randomizers, and a scrolling gallery of 124 buttons (rich static, glossy, candy and cartoon, animated) of many sizes, shapes and colors.

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
