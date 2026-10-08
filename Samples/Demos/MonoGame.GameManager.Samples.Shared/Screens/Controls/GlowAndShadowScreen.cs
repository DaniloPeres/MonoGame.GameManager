using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Controls.Shading;
using MonoGame.GameManager.Controls.Sprites;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Layout;
using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Samples.Screens.Controls
{
    /// <summary>
    /// Shadows, glows and outlines that follow the silhouette of texts and images: a preview where the target (text,
    /// multi-line text, image, nine-slice image or sprite animation) and every effect can be changed live, and a
    /// scrolling gallery of ready-made looks (click one to edit it).
    /// </summary>
    public class GlowAndShadowScreen : Screen
    {
        private const int SectionTop = Config.ScreenContentMargin + 60;
        private const int SectionHeight = 690;
        private const int SectionDivisionLeft = 550;
        private const int OptionsWidth = SectionDivisionLeft - Config.ScreenContentMargin * 2;
        private const int PreviewLeft = SectionDivisionLeft + Config.ScreenContentMargin;
        private const int PreviewWidth = 600;
        private const int StageTop = 34;
        private const int StageHeight = 300;
        private const int GalleryTop = 366;
        private const int PagesTop = 78;
        private const int RowHeight = 36;
        private const int ActionsTop = 648;
        private const int TileWidth = 184;
        private const int TileHeight = 128;
        private const int TileGap = 10;

        private static readonly Color TabColor = new Color(60, 60, 60);
        private static readonly Color SelectedTabColor = new Color(80, 160, 230);
        private static readonly Color ActionColor = new Color(70, 110, 70);
        private static readonly Color SmallButtonBorder = new Color(110, 110, 110);
        private static readonly string[] TabNames = { "Target", "Effects" };
        private static readonly string[] KindNames = { "Text", "Multi-line text", "Image", "Nine-slice", "Sprite animation" };
        private static readonly string[] Texts = { "GLOW & SHADOW", "LEGENDARY", "Game Over", "POW!", "Level 7", "Victory!", "OPEN 24H", "Settings" };
        private static readonly string[] ImageNames = { "Star", "Orb", "Diamond", "Camera", "Calculator", "Picture", "Window" };
        private static readonly string[] SpriteNames = { "Coin", "Dino", "Torch" };
        private static readonly string[] CycleNames = { "None", "Rainbow", "Neon", "Fire", "Magic", "Ice" };
        private static readonly IList<Color>[] CyclePalettes =
        {
            null,
            ShadingPresets.RainbowColors,
            new[] { new Color(40, 240, 255), new Color(255, 60, 220) },
            ShadingPresets.FireColors,
            ShadingPresets.MagicColors,
            new[] { new Color(60, 140, 255), new Color(160, 230, 255), Color.White }
        };

        /// <summary>How many effects of each kind the add buttons can create.</summary>
        private static readonly Dictionary<Type, int> Limits = new Dictionary<Type, int>
        {
            { typeof(Shadow), 2 }, { typeof(Glow), 3 }, { typeof(Outline), 2 }, { typeof(Shine), 1 }
        };

        private static readonly string[] BlendNames = { "Light (added)", "Paint (over)" };
        private static readonly string[] LayerNames = { "Behind", "Front (bloom)" };
        private const string Paragraph = "Shadows, glows and outlines follow every glyph of the text, at any scale and rotation.";

        private static readonly List<Color> TargetColors = new List<Color>
        {
            Color.White, new Color(255, 220, 40), new Color(255, 225, 120), new Color(255, 90, 170),
            new Color(120, 220, 255), new Color(150, 255, 170), new Color(60, 60, 70), new Color(150, 150, 160)
        };

        private static readonly List<Color> ShadowColors = new List<Color>
        {
            Color.Black, new Color(50, 25, 0), new Color(0, 30, 80), new Color(90, 0, 60),
            Color.White, new Color(255, 60, 170), new Color(60, 220, 255), new Color(255, 200, 60)
        };

        private static readonly List<Color> LightColors = new List<Color>
        {
            Color.White, new Color(255, 190, 60), new Color(255, 110, 40), new Color(255, 60, 60),
            new Color(255, 60, 200), new Color(170, 90, 255), new Color(60, 200, 255), new Color(60, 230, 120)
        };

        private static readonly List<Color> OutlineColors = new List<Color>
        {
            Color.Black, Color.White, new Color(90, 45, 0), new Color(20, 40, 90),
            new Color(110, 20, 0), new Color(40, 0, 70), new Color(255, 220, 40), new Color(60, 200, 255)
        };

        private static readonly List<Color> StageColors = new List<Color>
        {
            new Color(12, 12, 22), new Color(40, 25, 60), new Color(20, 50, 90), new Color(40, 140, 230),
            new Color(60, 160, 90), new Color(230, 90, 70), new Color(150, 150, 160), new Color(230, 232, 240)
        };

        private readonly List<ShadingEffect> effects = new List<ShadingEffect>();
        private readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private readonly Dictionary<string, SpriteAnimationInfo> sprites = new Dictionary<string, SpriteAnimationInfo>();
        private Look look;
        private Panel stage;
        private RectangleControl stageBackground;
        private Target preview;
        private Label infoLabel;
        private Button[] tabButtons;
        private ScrollViewer[] pages;
        private int selectedTab;
        private int presetIndex = -1;
        private readonly Dictionary<Type, Button> addButtons = new Dictionary<Type, Button>();
        private int row;

        public static void OpenGlowAndShadowScreen() => ServiceProvider.ScreenManager.ChangeScreen(new GlowAndShadowScreen());

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Glow & Shadow", OpenGlowAndShadowScreen)
            });

            LoadResources();
            look = Gallery[12];
            effects.AddRange(look.CreateEffects());

            CreatePreviewSection();
            CreateOptionsSection();
            new RectangleControl(new Rectangle(SectionDivisionLeft, SectionTop, 2, SectionHeight), Color.White)
                .AddToScreen();

            RebuildPreview();
            RebuildPages();
            ShowTab(1);
            base.OnInit();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var enabled = 0;
            foreach (var effect in effects)
            {
                if (effect.IsEnabled)
                    enabled++;
            }

            infoLabel.Text = $"{enabled} of {effects.Count} effects on: {look.Caption}";
        }

        // ---- Resources

        private void LoadResources()
        {
            var device = ServiceProvider.GraphicsDevice;
            var content = ContentHandler.Instance;
            textures["Star"] = RegisterDisposable(TextureFactory.CreateStar(device, 128, Color.White, 5, 0.46f));
            textures["Orb"] = RegisterDisposable(TextureFactory.CreateCircle(device, 96, Color.White));
            textures["Diamond"] = RegisterDisposable(TextureFactory.CreateDiamond(device, 110, Color.White));
            textures["Card"] = RegisterDisposable(TextureFactory.CreateRoundedRectangle(device, 64, 64, 18, Color.White));
            textures["Camera"] = content.TextureCamera;
            textures["Calculator"] = content.TextureCalculator;
            textures["Picture"] = content.TextureImage;
            textures["Window"] = content.TextureWindowApplication;

            var loader = ServiceProvider.ContentLoader;
            sprites["Coin"] = loader.LoadSpriteAnimationInfo("Images/Sprites/Coin.sa");
            sprites["Dino"] = loader.LoadSpriteAnimationInfo("Images/Sprites/Dino/Dino.sa");
            sprites["Torch"] = loader.LoadSpriteAnimationInfo("Images/Sprites/TorchDrippingRed.sa");
        }

        // ---- Preview

        private void CreatePreviewSection()
        {
            var font = ContentHandler.Instance.Font;
            var container = new Panel(new Rectangle(PreviewLeft, SectionTop, PreviewWidth, SectionHeight))
                .AddToScreen();

            new Label(font, "Preview", Vector2.Zero, Color.Yellow)
                .SetAnchor(Anchor.TopCenter)
                .AddToScreen(container);

            stage = new Panel(new Rectangle(0, StageTop, PreviewWidth, StageHeight))
                .SetHideOverflow(true)
                .AddToScreen(container);
            stageBackground = new RectangleControl(Vector2.Zero, stage.Size, StageColors[0])
                .AddToScreen(stage);

            var infoTop = StageTop + StageHeight + 6;
            infoLabel = new Label(font, string.Empty, new Vector2(0, infoTop), Color.White)
                .SetScale(0.65f)
                .AddToScreen(container);
            new FpsCounter(font, new Vector2(PreviewWidth - 90, infoTop), Color.Yellow)
                .SetScale(0.65f)
                .AddToScreen(container);

            CreateGallery(container);
        }

        /// <summary>Creates the preview again from the current look, with the edited effects.</summary>
        private void RebuildPreview()
        {
            preview?.Control.Dispose();
            stageBackground.Color = look.Stage;
            preview = CreateTarget(look, stage, look.PreviewScale);
            preview.Control.SetRotationInDegree(look.Rotation);
            preview.Control.SetOpacity(look.Opacity);
            preview.SetShadings(effects);
        }

        /// <summary>Applies the shading list after effects were added or removed.</summary>
        private void ApplyEffects() => preview.SetShadings(effects);

        // ---- Gallery

        private void CreateGallery(Panel container)
        {
            var font = ContentHandler.Instance.Font;
            var frame = new Panel(new Rectangle(0, GalleryTop, PreviewWidth, SectionHeight - GalleryTop))
                .SetHideOverflow(true)
                .AddToScreen(container);
            new RectangleControl(Vector2.Zero, frame.Size, new Color(45, 40, 75))
                .AddToScreen(frame);
            new Label(font, "Gallery: scroll for more, click a look to edit it", new Vector2(12, 6), Color.Yellow)
                .SetScale(0.6f)
                .AddToScreen(frame);

            var gallery = new ScrollViewer(new Vector2(0, 28), new Vector2(PreviewWidth, frame.Size.Y - 28))
                .SetHideOverflow(true)
                .AddToScreen(frame);
            var content = gallery.ContentPanel;

            var left = (PreviewWidth - 3 * TileWidth - 2 * TileGap) / 2;
            var top = 4f;
            foreach (var section in GallerySections)
            {
                new Label(font, $"{section.Title} ({section.Items.Length})", new Vector2(left, top), new Color(200, 200, 235))
                    .SetScale(0.6f)
                    .AddToScreen(content);
                top += 24f;
                for (var i = 0; i < section.Items.Length; i++)
                    AddGalleryTile(content, section.Items[i], new Vector2(left + i % 3 * (TileWidth + TileGap), top + i / 3 * (TileHeight + TileGap)));
                top += (section.Items.Length + 2) / 3 * (TileHeight + TileGap) + 8f;
            }

            // Some space under the last row.
            new RectangleControl(new Rectangle(0, (int)top, 1, 6), Color.Transparent)
                .AddToScreen(content);
        }

        private void AddGalleryTile(Panel content, Look item, Vector2 position)
        {
            var font = ContentHandler.Instance.Font;
            {
                var tile = new Panel(new Rectangle(position.ToPoint(), new Point(TileWidth, TileHeight)))
                    .SetHideOverflow(true)
                    .AddOnClick(args => SelectLook(item))
                    .AddToScreen(content);
                new RectangleControl(Vector2.Zero, tile.Size, item.Stage)
                    .AddToScreen(tile);

                var target = CreateTarget(item, tile, item.TileScale);
                target.Control.SetPosition(0, -8);
                target.Control.SetRotationInDegree(item.Rotation);
                target.Control.SetOpacity(item.Opacity);
                target.SetShadings(item.CreateEffects());

                var caption = new Label(font, item.Caption, Vector2.Zero, IsLight(item.Stage) ? new Color(40, 40, 50) : new Color(220, 220, 235))
                    .SetScale(0.55f)
                    .SetAnchor(Anchor.BottomCenter)
                    .SetPosition(0, 4)
                    .AddToScreen(tile);
            }
        }

        private void SelectLook(Look item)
        {
            look = item;
            effects.Clear();
            effects.AddRange(item.CreateEffects());
            RebuildPreview();
            RebuildPages();
        }

        private static bool IsLight(Color color) => color.R + color.G + color.B > 450;

        // ---- Targets

        /// <summary>A control of the preview or of the gallery, with a way to set its shading effects.</summary>
        private sealed class Target
        {
            public IScalableControl Control;
            public Action<IEnumerable<ShadingEffect>> SetShadings;
        }

        private Target CreateTarget(Look item, Panel parent, float scale)
        {
            var font = ContentHandler.Instance.Font;
            switch (item.Kind)
            {
                case TargetKind.MultiLine:
                    return Setup(new MultiLineLabel(font, item.Text, Vector2.Zero, item.Color, (int)(300 * scale)).SetTextAlign(TextAlign.Center), parent, scale);
                case TargetKind.Image:
                    return Setup(new Image(textures[item.Image]).SetColor(item.Color), parent, scale);
                case TargetKind.NineSlice:
                    return Setup(new NineSliceImage(textures["Card"], new Thickness(20), Vector2.Zero, new Vector2(220, 120)).SetColor(item.Color), parent, scale);
                case TargetKind.Sprite:
                    var animation = sprites[item.Image].CreateSpriteAnimation();
                    animation.Play(item.Image == "Dino" ? 2 : 0);
                    return Setup(animation.SetColor(item.Color), parent, scale);
                default:
                    return Setup(new Label(font, item.Text, Vector2.Zero, item.Color), parent, scale);
            }
        }

        private static Target Setup<TControl>(TControl control, Panel parent, float scale) where TControl : ScalableControlAbstract<TControl>
        {
            control.SetScale(scale)
                .SetOriginRate(new Vector2(0.5f))
                .SetAnchor(Anchor.Center)
                .AddToScreen(parent);
            return new Target { Control = control, SetShadings = list => control.SetShadings(list) };
        }

        // ---- Options

        private void CreateOptionsSection()
        {
            var font = ContentHandler.Instance.Font;
            var container = new Panel(new Rectangle(Config.ScreenContentMargin, SectionTop, OptionsWidth, SectionHeight))
                .AddToScreen();

            new Label(font, "Options", Vector2.Zero, Color.Yellow)
                .SetAnchor(Anchor.TopCenter)
                .AddToScreen(container);

            tabButtons = new Button[TabNames.Length];
            pages = new ScrollViewer[TabNames.Length];
            for (var i = 0; i < TabNames.Length; i++)
            {
                var index = i;
                tabButtons[i] = CreateSmallButton(container, TabNames[i], new Vector2(i * 106, 36), new Vector2(100, 32), TabColor, 0.65f, () => ShowTab(index));
                pages[i] = new ScrollViewer(new Vector2(0, PagesTop), new Vector2(OptionsWidth, ActionsTop - PagesTop - 8))
                    .SetHideOverflow(true)
                    .SetIsVisible(false)
                    .AddToScreen(container);
            }

            var actionSize = new Vector2(80, 36);
            addButtons[typeof(Shadow)] = CreateSmallButton(container, "+ Shadow", new Vector2(0, ActionsTop), actionSize, ActionColor, 0.5f, () => AddEffect(CreateShadow()));
            addButtons[typeof(Glow)] = CreateSmallButton(container, "+ Glow", new Vector2(86, ActionsTop), actionSize, ActionColor, 0.5f, () => AddEffect(CreateGlow()));
            addButtons[typeof(Outline)] = CreateSmallButton(container, "+ Outline", new Vector2(172, ActionsTop), actionSize, ActionColor, 0.5f, () => AddEffect(CreateOutline()));
            addButtons[typeof(Shine)] = CreateSmallButton(container, "+ Shine", new Vector2(258, ActionsTop), actionSize, ActionColor, 0.5f, () => AddEffect(new Shine(Color.White)));
            CreateSmallButton(container, "Random", new Vector2(344, ActionsTop), actionSize, ActionColor, 0.55f, Randomize);
            CreateSmallButton(container, "Clear", new Vector2(430, ActionsTop), actionSize, new Color(120, 60, 60), 0.55f, () => SetEffects(new List<ShadingEffect>()));
        }

        private void ShowTab(int index)
        {
            selectedTab = index;
            for (var i = 0; i < pages.Length; i++)
            {
                pages[i].SetIsVisible(i == index);
                tabButtons[i].SetBackgroundColors(i == index ? SelectedTabColor : TabColor);
            }
        }

        private int CountOf(Type type)
        {
            var count = 0;
            foreach (var effect in effects)
            {
                if (effect.GetType() == type)
                    count++;
            }

            return count;
        }

        /// <summary>Shows how many effects of each kind there are on the add buttons, and turns off the full ones.</summary>
        private void RefreshAddButtons()
        {
            foreach (var pair in addButtons)
            {
                var count = CountOf(pair.Key);
                var limit = Limits[pair.Key];
                pair.Value.SetText($"+{pair.Key.Name} {count}/{limit}");
                pair.Value.SetIsEnabled(count < limit);
            }
        }

        // New effects get another look each time, so stacking them makes something nice at once.
        private Shadow CreateShadow() => CountOf(typeof(Shadow)) == 0
            ? new Shadow(new Vector2(0, 6), 8, Color.Black * 0.6f)
            : new Shadow(new Vector2(0, 3), 0, new Color(90, 0, 60)).SetSpread(2);

        private Glow CreateGlow()
        {
            switch (CountOf(typeof(Glow)))
            {
                case 0:
                    return new Glow(new Color(255, 120, 30), 28).SetIntensity(1.4f).SetBreathe(0.6f, 0.05f);
                case 1:
                    return new Glow(new Color(255, 200, 60), 12).SetIntensity(1.4f).SetPulse(1f, 0.35f);
                default:
                    return new Glow(Color.White, 3).SetIntensity(1.2f).SetFlicker(0.3f, 9f);
            }
        }

        private Outline CreateOutline() => CountOf(typeof(Outline)) == 0
            ? new Outline(Color.Black, 3)
            : new Outline(Color.White, 6);

        private void AddEffect(ShadingEffect effect)
        {
            if (CountOf(effect.GetType()) >= Limits[effect.GetType()])
                return;

            // Shadows go under the glows and the outlines, outlines over them, and the shine on top (added last).
            var index = effect is Shine ? effects.Count
                : effect is Outline ? FirstIndexOf<Shine>()
                : effect is Shadow ? FirstIndexOf<Glow>(FirstIndexOf<Outline>(FirstIndexOf<Shine>()))
                : FirstIndexOf<Outline>(FirstIndexOf<Shine>());
            effects.Insert(index, effect);
            ApplyEffects();
            RebuildPages();
            ShowTab(1);
        }

        /// <summary>The index of the first effect of a type, or <paramref name="otherwise"/> (the end of the list by default).</summary>
        private int FirstIndexOf<T>(int otherwise = -1) where T : ShadingEffect
        {
            for (var i = 0; i < effects.Count; i++)
            {
                if (effects[i] is T)
                    return i;
            }

            return otherwise >= 0 ? otherwise : effects.Count;
        }

        private void RemoveEffect(ShadingEffect effect)
        {
            effects.Remove(effect);
            ApplyEffects();
            RebuildPages();
        }

        private void SetEffects(List<ShadingEffect> newEffects)
        {
            effects.Clear();
            effects.AddRange(newEffects);
            ApplyEffects();
            RebuildPages();
        }

        private void Randomize()
        {
            var random = ServiceProvider.Random;
            SetEffects(ShadingPresets.Create(ShadingPresets.AnimatedNames[random.Next(0, ShadingPresets.AnimatedNames.Count - 1)]));
            look = look.With(stage: StageColors[random.Next(0, StageColors.Count - 1)], color: TargetColors[random.Next(0, TargetColors.Count - 1)]);
            RebuildPreview();
            RebuildPages();
        }

        private void RebuildPages()
        {
            if (pages == null)
                return;

            foreach (var page in pages)
                page.ClearChildren();

            BuildTargetPage(pages[0].ContentPanel);
            BuildEffectsPage(pages[1].ContentPanel);
            RefreshAddButtons();
            if (selectedTab < pages.Length)
                ShowTab(selectedTab);
        }

        private void BuildTargetPage(Panel page)
        {
            row = 0;
            Choice(page, "Preset", new List<string>(ShadingPresets.Names), presetIndex, index =>
            {
                presetIndex = index;
                SetEffects(ShadingPresets.Create(ShadingPresets.Names[index]));
            });
            Choice(page, "Target", KindNames, (int)look.Kind, index =>
            {
                var kind = (TargetKind)index;
                var image = kind == TargetKind.Sprite ? SpriteNames[0] : kind == TargetKind.Image ? ImageNames[0] : look.Image;
                var text = kind == TargetKind.MultiLine ? Paragraph : kind == TargetKind.Text && look.Kind != TargetKind.Text ? Texts[0] : look.Text;
                ChangeLook(look.With(kind: kind, image: image, text: text, previewScale: DefaultScale(kind)));
            });
            Choice(page, "Text", Texts, Math.Max(0, Array.IndexOf(Texts, look.Text)), index => ChangeLook(look.With(text: Texts[index])));
            Choice(page, "Image", ImageNames, Math.Max(0, Array.IndexOf(ImageNames, look.Image)), index => ChangeLook(look.With(kind: TargetKind.Image, image: ImageNames[index], previewScale: DefaultScale(TargetKind.Image))));
            Choice(page, "Sprite", SpriteNames, Math.Max(0, Array.IndexOf(SpriteNames, look.Image)), index => ChangeLook(look.With(kind: TargetKind.Sprite, image: SpriteNames[index], previewScale: DefaultScale(TargetKind.Sprite))));
            Colors(page, "Color", TargetColors, color => preview.Control.SetColor(color));
            Colors(page, "Stage", StageColors, color => stageBackground.Color = color);
            Slider(page, "Scale", 0.25f, 4f, look.PreviewScale, value => preview.Control.SetScale(value), 0.05f, "{0:0.00}");
            Slider(page, "Rotation", 0, 360, (look.Rotation + 360f) % 360f, value => preview.Control.SetRotationInDegree(value), 1, "{0:0}");
            Slider(page, "Opacity", 0, 1, look.Opacity, value => preview.Control.SetOpacity(value), 0.05f, "{0:0.00}");
            Slider(page, "Game speed", 0, 2, ServiceProvider.Clock.TimeScale, value => ServiceProvider.Clock.TimeScale = value, 0.1f, "{0:0.0}x");
        }

        private void ChangeLook(Look newLook)
        {
            look = newLook;
            RebuildPreview();
            RebuildPages();
        }

        private static float DefaultScale(TargetKind kind)
        {
            switch (kind)
            {
                case TargetKind.Text:
                    return 2f;
                case TargetKind.MultiLine:
                    return 1.3f;
                case TargetKind.Sprite:
                    return 1.6f;
                default:
                    return 1.4f;
            }
        }

        private void BuildEffectsPage(Panel page)
        {
            row = 0;
            if (effects.Count == 0)
            {
                new Label(ContentHandler.Instance.Font, "No effects: add a shadow, a glow, an outline or a shine below.", new Vector2(0, 6), Color.Gray)
                    .SetScale(0.7f)
                    .AddToScreen(page);
                return;
            }

            var counts = new Dictionary<Type, int>();
            foreach (var effect in effects)
            {
                counts.TryGetValue(effect.GetType(), out var count);
                counts[effect.GetType()] = ++count;
                var title = $"{effect.GetType().Name} {count}";
                Header(page, title, effect);

                switch (effect)
                {
                    case Shadow shadow:
                        Colors(page, "   Color", ShadowColors, color => shadow.Color = color);
                        Slider(page, "   Offset X", -20, 20, shadow.Offset.X, value => shadow.Offset = new Vector2(value, shadow.Offset.Y), 0.5f, "{0:0.0}");
                        Slider(page, "   Offset Y", -20, 20, shadow.Offset.Y, value => shadow.Offset = new Vector2(shadow.Offset.X, value), 0.5f, "{0:0.0}");
                        Slider(page, "   Blur", 0, 40, shadow.Blur, value => shadow.Blur = value, 0.5f, "{0:0.0}");
                        Slider(page, "   Spread", 0, 12, shadow.Spread, value => shadow.Spread = value, 0.5f, "{0:0.0}");
                        Slider(page, "   Intensity", 0, 3, shadow.Intensity, value => shadow.Intensity = value, 0.05f, "{0:0.00}");
                        OrbitRow(page, shadow);
                        FlickerRow(page, shadow);
                        break;
                    case Glow glow:
                        Colors(page, "   Color", LightColors, color => { glow.ColorCycle = null; glow.Color = color; });
                        CycleRow(page, glow);
                        Slider(page, "   Radius", 0, 60, glow.Radius, value => glow.Radius = value, 0.5f, "{0:0.0}");
                        Slider(page, "   Spread", 0, 12, glow.Spread, value => glow.Spread = value, 0.5f, "{0:0.0}");
                        Slider(page, "   Intensity", 0, 4, glow.Intensity, value => glow.Intensity = value, 0.05f, "{0:0.00}");
                        Slider(page, "   Offset Y", -20, 20, glow.Offset.Y, value => glow.Offset = new Vector2(glow.Offset.X, value), 0.5f, "{0:0.0}");
                        PulseRow(page, glow);
                        FlickerRow(page, glow);
                        BreatheRow(page, glow);
                        OrbitRow(page, glow);
                        Choice(page, "   Blend", BlendNames, glow.Blend == ShadingBlend.Light ? 0 : 1, index => glow.Blend = index == 0 ? ShadingBlend.Light : ShadingBlend.Normal);
                        Choice(page, "   Layer", LayerNames, (int)glow.Layer, index => glow.Layer = (ShadingLayer)index);
                        break;
                    case Outline outline:
                        Colors(page, "   Color", OutlineColors, color => { outline.ColorCycle = null; outline.Color = color; });
                        CycleRow(page, outline);
                        Slider(page, "   Thickness", 0, 16, outline.Thickness, value => outline.Thickness = value, 0.5f, "{0:0.0}");
                        Slider(page, "   Softness", 0, 12, outline.Softness, value => outline.Softness = value, 0.5f, "{0:0.0}");
                        Slider(page, "   Intensity", 0, 1, outline.Intensity, value => outline.Intensity = value, 0.05f, "{0:0.00}");
                        PulseRow(page, outline);
                        break;
                    case Shine shine:
                        Colors(page, "   Color", LightColors, color => { shine.ColorCycle = null; shine.Color = color; });
                        CycleRow(page, shine);
                        Slider(page, "   Width", 4, 80, shine.Width, value => shine.Width = value, 1, "{0:0}");
                        Slider(page, "   Angle", -60, 60, shine.Angle, value => shine.Angle = value, 1, "{0:0}");
                        Slider(page, "   Sweep time", 0.2f, 3, shine.Duration, value => shine.Duration = value, 0.05f, "{0:0.00}s");
                        Slider(page, "   Every", 0, 8, shine.Interval, value => shine.Interval = value, 0.1f, "{0:0.0}s");
                        Slider(page, "   Intensity", 0, 1, shine.Intensity, value => shine.Intensity = value, 0.05f, "{0:0.00}");
                        Check(page, "   Right to left", shine.Reverse, value => shine.Reverse = value);
                        break;
                }

                NextRow();
            }
        }

        private void PulseRow(Panel page, ShadingEffect effect)
            => Slider(page, "   Pulse", 0, 4, effect.PulseSpeed, value => { effect.PulseSpeed = value; effect.PulseAmount = Math.Max(effect.PulseAmount, 0.4f); }, 0.1f, "{0:0.0}/s");

        private void FlickerRow(Panel page, ShadingEffect effect)
            => Slider(page, "   Flicker", 0, 1, effect.FlickerAmount, value => effect.FlickerAmount = value, 0.05f, "{0:0.00}");

        private void BreatheRow(Panel page, ShadingEffect effect)
            => Slider(page, "   Breathe", 0, 0.2f, effect.BreatheAmount, value => { effect.BreatheAmount = value; effect.BreatheSpeed = effect.BreatheSpeed > 0f ? effect.BreatheSpeed : 0.6f; }, 0.01f, "{0:0.00}");

        private void OrbitRow(Panel page, ShadingEffect effect)
            => Slider(page, "   Orbit", 0, 12, effect.OrbitRadius, value => { effect.OrbitRadius = value; effect.OrbitSpeed = effect.OrbitSpeed != 0f ? effect.OrbitSpeed : 0.7f; }, 0.5f, "{0:0.0}");

        private void CycleRow(Panel page, ShadingEffect effect)
        {
            var selected = 0;
            for (var i = 1; i < CycleNames.Length; i++)
            {
                if (ReferenceEquals(effect.ColorCycle, CyclePalettes[i]))
                    selected = i;
            }

            Choice(page, "   Color cycle", CycleNames, selected, index =>
            {
                effect.ColorCycle = CyclePalettes[index];
                effect.ColorCyclePeriod = 3f;
            });
        }

        private void Check(Panel page, string text, bool value, Action<bool> onChanged)
            => CheckboxOption.CreateCheckboxControlOption(page, text, NextRow(), value, onChanged);

        /// <summary>The first row of an effect: its name, a checkbox to turn it on and off, and a remove button.</summary>
        private void Header(Panel page, string title, ShadingEffect effect)
        {
            var top = NextRow();
            new Label(ContentHandler.Instance.Font, title, new Vector2(0, top + 2), new Color(120, 220, 255))
                .SetScale(0.85f)
                .AddToScreen(page);
            new Checkbox(new Vector2(SliderOption.SliderLeft, top), 26)
                .SetIsChecked(effect.IsEnabled, notify: false)
                .AddOnCheckedChanged(value => effect.IsEnabled = value)
                .AddToScreen(page);
            CreateSmallButton(page, "Remove", new Vector2(SliderOption.SliderLeft + 40, top - 1), new Vector2(84, 28), new Color(120, 60, 60), 0.55f, () => RemoveEffect(effect));
        }

        private float NextRow() => row++ * RowHeight;

        private void Slider(Panel page, string text, float minimum, float maximum, float value, Action<float> onChanged, float step, string format)
            => SliderOption.CreateSliderOption(page, text, NextRow(), minimum, maximum, MathHelper.Clamp(value, minimum, maximum), onChanged, step, format);

        private void Choice(Panel page, string text, IList<string> choices, int selected, Action<int> onChanged)
            => ChoiceOption.CreateChoiceOption(page, text, NextRow(), choices, selected, onChanged);

        private void Colors(Panel page, string text, List<Color> colors, Action<Color> onSelected)
            => ColorOption.CreateColorOption(page, NextRow(), onSelected, text, colors, true, 0.75f, 26, SliderOption.SliderLeft);

        private static Button CreateSmallButton(Panel container, string text, Vector2 position, Vector2 size, Color color, float textScale, Action onClick)
        {
            var button = new Button(position, size, color)
                .SetBorder(SmallButtonBorder)
                .SetText(ContentHandler.Instance.Font, text, Color.White)
                .AddOnClick(args => onClick())
                .AddToScreen(container);
            button.TextLabel.SetScale(textScale);
            return button;
        }

        public override void Dispose()
        {
            ServiceProvider.Clock.TimeScale = 1f;
            base.Dispose();
        }

        // ---- Looks

        private enum TargetKind
        {
            Text,
            MultiLine,
            Image,
            NineSlice,
            Sprite
        }

        /// <summary>A look of the gallery: a target, its colors and its effects.</summary>
        private sealed class Look
        {
            private readonly Func<List<ShadingEffect>> createEffects;

            public Look(string caption, TargetKind kind, string content, Color color, Color stage, Func<List<ShadingEffect>> createEffects,
                float tileScale = 1f, float previewScale = 2f, float rotation = 0f, float opacity = 1f)
            {
                Caption = caption;
                Kind = kind;
                Text = kind == TargetKind.Text || kind == TargetKind.MultiLine ? content : Texts[0];
                Image = kind == TargetKind.Image || kind == TargetKind.Sprite ? content : ImageNames[0];
                Color = color;
                Stage = stage;
                this.createEffects = createEffects;
                TileScale = tileScale;
                PreviewScale = previewScale;
                Rotation = rotation;
                Opacity = opacity;
            }

            public string Caption { get; private set; }
            public TargetKind Kind { get; private set; }
            public string Text { get; private set; }
            public string Image { get; private set; }
            public Color Color { get; private set; }
            public Color Stage { get; private set; }
            public float TileScale { get; private set; }
            public float PreviewScale { get; private set; }
            public float Rotation { get; private set; }
            public float Opacity { get; private set; }

            public List<ShadingEffect> CreateEffects() => createEffects();

            /// <summary>A copy with some values changed (the effects are not copied: the preview keeps its own).</summary>
            public Look With(TargetKind? kind = null, string text = null, string image = null, Color? color = null, Color? stage = null, float? previewScale = null)
            {
                var copy = (Look)MemberwiseClone();
                copy.Caption = "your own look";
                copy.Kind = kind ?? Kind;
                copy.Text = text ?? Text;
                copy.Image = image ?? Image;
                copy.Color = color ?? Color;
                copy.Stage = stage ?? Stage;
                copy.PreviewScale = previewScale ?? PreviewScale;
                return copy;
            }
        }

        private static readonly Color Night = new Color(12, 10, 24);

        /// <summary>The gallery: animated looks first, then still ones, then images and sprites.</summary>
        private static readonly Look[] Gallery =
        {
            new Look("Neon sign (flickers)", TargetKind.Text, "OPEN", new Color(255, 225, 250), Night, () => ShadingPresets.Neon(ShadingPresets.NeonPink), 1.6f, 3f),
            new Look("Fire (3 glows)", TargetKind.Text, "HOT", new Color(255, 245, 210), new Color(25, 10, 5), ShadingPresets.Fire, 1.6f, 3f),
            new Look("Gold title (shine)", TargetKind.Text, "LEGENDARY", new Color(255, 225, 120), new Color(40, 25, 60), ShadingPresets.GoldTitle, 0.85f, 2.2f),
            new Look("Ice (breathes)", TargetKind.Text, "FROZEN", new Color(240, 250, 255), new Color(15, 30, 60), ShadingPresets.Ice, 1.3f, 2.6f),
            new Look("Rainbow (3 glows)", TargetKind.Text, "PARTY", Color.White, Night, ShadingPresets.Rainbow, 1.3f, 2.6f),
            new Look("Plasma (orbits)", TargetKind.Text, "ENERGY", Color.White, new Color(8, 6, 20), ShadingPresets.Plasma, 1.05f, 2.4f),
            new Look("Magic (orbits)", TargetKind.Text, "Spell", Color.White, new Color(15, 5, 30), ShadingPresets.Magic, 1.6f, 3f),
            new Look("Toxic (throbs)", TargetKind.Text, "DANGER", new Color(215, 255, 130), new Color(14, 20, 10), ShadingPresets.Toxic, 1.05f, 2.4f),
            new Look("Heartbeat", TargetKind.Text, "LOVE", new Color(255, 225, 232), new Color(35, 8, 18), ShadingPresets.Heartbeat, 1.5f, 3f),
            new Look("Lightning", TargetKind.Text, "STORM", new Color(235, 245, 255), new Color(10, 14, 30), Lightning, 1.2f, 2.6f),
            new Look("Sunset", TargetKind.Text, "Paradise", new Color(255, 240, 220), new Color(45, 15, 50), Sunset, 1.1f, 2.4f, -4f),
            new Look("Glitch", TargetKind.Text, "ERROR", Color.White, new Color(10, 10, 18), ShadingPresets.Glitch, 1.3f, 2.8f),
            new Look("All in one: 6 effects", TargetKind.Text, "Level 7", Color.White, new Color(40, 25, 60), CombinedText, 1.4f, 2.6f, -6f),
            new Look("Neon blue", TargetKind.Text, "BAR", new Color(225, 250, 255), Night, () => ShadingPresets.Neon(new Color(40, 190, 255)), 1.6f, 3f),
            new Look("Ghost (breathes)", TargetKind.Text, "Boo...", new Color(200, 215, 240), new Color(20, 25, 35), ShadingPresets.Ghost, 1.6f, 3f, 0f, 0.85f),
            new Look("Retro wave", TargetKind.Text, "1985", Color.White, new Color(30, 10, 50), RetroWave, 1.6f, 3f),
            new Look("Comic (shine)", TargetKind.Text, "POW!", new Color(255, 220, 40), new Color(40, 140, 230), ComicShine, 1.6f, 3f, -6f),
            new Look("Candy (shine)", TargetKind.Text, "Sweet!", new Color(255, 90, 170), new Color(255, 205, 230), CandyShine, 1.4f, 2.8f, -4f),
            new Look("Double outline", TargetKind.Text, "WIN", new Color(255, 220, 40), new Color(60, 160, 90), () => ShadingPresets.DoubleOutline(Color.White, new Color(20, 60, 30)), 1.6f, 3f),
            new Look("Emboss", TargetKind.Text, "Settings", new Color(150, 150, 160), new Color(150, 150, 160), ShadingPresets.Emboss, 1.4f, 2.8f),
            new Look("Long shadow", TargetKind.Text, "FLAT", Color.White, new Color(230, 90, 70), ShadingPresets.LongShadow, 1.6f, 3f),
            new Look("Multi-line text", TargetKind.MultiLine, Paragraph, Color.White, new Color(20, 50, 90), ParagraphEffects, 0.55f, 1.4f),
            new Look("Golden star (shine)", TargetKind.Image, "Star", new Color(255, 205, 50), new Color(30, 30, 50), GoldenStar, 0.6f, 1.4f),
            new Look("Shiny coin", TargetKind.Sprite, "Coin", Color.White, Night, ShinyCoin, 0.65f, 1.8f),
            new Look("Burning torch", TargetKind.Sprite, "Torch", Color.White, new Color(18, 10, 8), BurningTorch, 0.55f, 1.3f),
            new Look("Plasma orb", TargetKind.Image, "Orb", new Color(70, 35, 130), Night, PlasmaOrb, 0.5f, 1.2f),
            new Look("Gem (shine)", TargetKind.Image, "Diamond", new Color(80, 220, 160), new Color(20, 40, 35), ShadingPresets.Ice, 0.65f, 1.6f),
            new Look("Hologram (scan line)", TargetKind.Image, "Window", new Color(120, 220, 255) * 0.75f, Night, ShadingPresets.Hologram, 1f, 2.4f),
            new Look("Sticker (shine)", TargetKind.Image, "Picture", Color.White, new Color(90, 170, 220), StickerShine, 1f, 2.2f, -8f),
            new Look("Outlined sprite", TargetKind.Sprite, "Dino", Color.White, new Color(120, 200, 120), ShadingPresets.Sticker, 0.8f, 2f),
            new Look("Soft shadow", TargetKind.Image, "Calculator", Color.White, new Color(230, 232, 240), () => new List<ShadingEffect> { new Shadow(new Vector2(0, 6), 12, Color.Black * 0.45f) }, 0.85f, 1.8f),
            new Look("Card", TargetKind.NineSlice, string.Empty, Color.White, new Color(225, 228, 238), () => new List<ShadingEffect> { new Shadow(new Vector2(0, 8), 18, Color.Black * 0.35f), new Shadow(new Vector2(0, 1), 2, Color.Black * 0.25f) }, 0.55f, 1.4f),
            new Look("Selected", TargetKind.Image, "Camera", Color.White, new Color(35, 45, 70), ShadingPresets.Selected, 1f, 2.2f)
        };

        /// <summary>The sections of the gallery: parts of <see cref="Gallery"/> and the still shadows and outlines.</summary>
        private static readonly (string Title, Look[] Items)[] GallerySections =
        {
            ("Animated glows and shines", Slice(0, 18)),
            ("Still texts", Slice(18, 4)),
            ("Images and sprites", Slice(22, 11)),
            ("Shadows and outlines, no animation", new[]
            {
                new Look("Drop shadow", TargetKind.Text, "Title", Color.White, new Color(60, 120, 200), () => Effects(new Shadow(new Vector2(0, 6), 10, Color.Black * 0.55f)), 1.6f, 3f),
                new Look("Hard shadow", TargetKind.Text, "BOLD", new Color(255, 225, 60), new Color(230, 90, 70), () => Effects(new Shadow(new Vector2(4, 5), 0, new Color(60, 10, 10)).SetSpread(1), new Outline(new Color(60, 10, 10), 2)), 1.6f, 3f),
                new Look("Extruded 3D", TargetKind.Text, "3D", new Color(255, 200, 80), new Color(40, 30, 70), () => Extrude(new Color(150, 60, 20), 7, Color.Black * 0.5f), 1.8f, 3.4f, -4f),
                new Look("Thin outline", TargetKind.Text, "Outline", Color.White, new Color(90, 170, 220), () => Effects(new Outline(new Color(20, 40, 80), 1.5f)), 1.5f, 3f),
                new Look("Thick outline", TargetKind.Text, "GAME", Color.White, new Color(120, 70, 200), () => Effects(new Shadow(new Vector2(0, 5), 4, Color.Black * 0.45f).SetSpread(6), new Outline(new Color(40, 20, 80), 6)), 1.4f, 2.8f),
                new Look("Triple outline", TargetKind.Text, "WOW", new Color(255, 230, 60), new Color(40, 160, 200), () => Effects(new Shadow(new Vector2(0, 5), 0, Color.Black * 0.5f).SetSpread(10), new Outline(Color.Black, 10), new Outline(Color.White, 7), new Outline(new Color(220, 40, 60), 4)), 1.3f, 2.6f, -5f),
                new Look("Soft outline", TargetKind.Text, "Soft", Color.White, new Color(205, 210, 220), () => Effects(new Outline(new Color(40, 50, 80), 2).SetSoftness(5)), 1.6f, 3f),
                new Look("Dark aura", TargetKind.Text, "NIGHT", new Color(220, 230, 255), new Color(110, 125, 160), () => Effects(new Shadow(Vector2.Zero, 16, new Color(10, 10, 30)).SetSpread(2).SetIntensity(1.6f)), 1.4f, 2.8f),
                new Look("Letterpress", TargetKind.Text, "Press", new Color(205, 180, 140), new Color(205, 180, 140), () => Effects(new Shadow(new Vector2(0, 1.5f), 0, Color.White * 0.7f), new Shadow(new Vector2(0, -1), 0, Color.Black * 0.45f)), 1.6f, 3f),
                new Look("Pixel", TargetKind.Text, "PIXEL", new Color(130, 255, 130), new Color(70, 100, 170), () => Effects(new Shadow(new Vector2(3, 3), 0, new Color(10, 60, 20)).SetSpread(2), new Outline(new Color(10, 60, 20), 2)), 1.4f, 2.8f),
                new Look("Floating", TargetKind.Text, "Float", new Color(60, 90, 210), new Color(235, 236, 245), () => Effects(new Shadow(new Vector2(0, 16), 14, Color.Black * 0.3f)), 1.6f, 3f),
                new Look("Pop art", TargetKind.Text, "POP", Color.White, new Color(255, 220, 60), () => Effects(new Shadow(new Vector2(8, 8), 0, new Color(60, 200, 255)).SetSpread(2), new Shadow(new Vector2(4, 4), 0, new Color(255, 70, 130)).SetSpread(2), new Outline(Color.Black, 2)), 1.6f, 3f, -6f),
                new Look("Icon outline", TargetKind.Image, "Camera", Color.White, new Color(240, 140, 60), () => Effects(new Shadow(new Vector2(0, 4), 4, Color.Black * 0.4f).SetSpread(3), new Outline(new Color(40, 30, 40), 3)), 1f, 2.2f),
                new Look("Floating image", TargetKind.Image, "Picture", Color.White, new Color(235, 236, 245), () => Effects(new Shadow(new Vector2(0, 14), 14, Color.Black * 0.35f)), 1f, 2.2f, 6f),
                new Look("Sprite outline", TargetKind.Sprite, "Dino", Color.White, new Color(250, 210, 120), () => Effects(new Shadow(new Vector2(3, 3), 0, Color.Black * 0.6f).SetSpread(2), new Outline(Color.Black, 2)), 0.8f, 2f),
                new Look("Stamp", TargetKind.Image, "Star", new Color(230, 50, 60), new Color(245, 235, 215), () => Effects(new Shadow(new Vector2(0, 4), 6, Color.Black * 0.35f).SetSpread(8), new Outline(new Color(120, 0, 10), 8), new Outline(Color.White, 4)), 0.55f, 1.3f, -10f),
                new Look("Long shadow icon", TargetKind.Image, "Calculator", Color.White, new Color(60, 170, 120), ShadingPresets.LongShadow, 0.85f, 1.8f),
                new Look("Extruded gem", TargetKind.Image, "Diamond", new Color(90, 170, 255), new Color(230, 235, 245), () => Extrude(new Color(20, 50, 120), 6, Color.Black * 0.35f), 0.6f, 1.5f)
            })
        };

        private static Look[] Slice(int start, int count)
        {
            var items = new Look[count];
            Array.Copy(Gallery, start, items, 0, count);
            return items;
        }

        private static List<ShadingEffect> Effects(params ShadingEffect[] effects) => new List<ShadingEffect>(effects);

        /// <summary>A solid 3D side: sharp shadows one unit apart down to <paramref name="depth"/>, and a soft shadow under them.</summary>
        private static List<ShadingEffect> Extrude(Color side, int depth, Color shadow)
        {
            var effects = new List<ShadingEffect> { new Shadow(new Vector2(depth * 0.7f, depth + 4f), 8, shadow) };
            for (var i = depth; i >= 1; i--)
                effects.Add(new Shadow(new Vector2(i * 0.7f, i), 0, Color.Lerp(side, Color.Black, 0.25f * i / depth)));
            return effects;
        }

        /// <summary>Everything at once: two shadows, three glows (one breathing, one pulsing) and an outline.</summary>
        private static List<ShadingEffect> CombinedText() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(0, 10), 10, Color.Black * 0.6f),
            new Shadow(new Vector2(0, 4), 0, new Color(120, 30, 160)).SetSpread(3),
            new Glow(new Color(170, 60, 255), 30).SetIntensity(1.2f).SetBreathe(0.5f, 0.05f),
            new Glow(new Color(255, 110, 230), 14).SetIntensity(1.6f).SetPulse(0.8f, 0.4f),
            new Glow(new Color(255, 220, 250), 3).SetIntensity(1.1f),
            new Outline(new Color(60, 10, 90), 3)
        };

        /// <summary>Lightning: a blue haze and a white glow that flash quickly, like a storm.</summary>
        private static List<ShadingEffect> Lightning() => new List<ShadingEffect>
        {
            new Glow(new Color(60, 110, 255), 30).SetIntensity(1.4f).SetFlicker(0.85f, 17f),
            new Glow(new Color(170, 210, 255), 12).SetIntensity(1.5f).SetFlicker(0.95f, 23f).SetTimeOffset(0.7f),
            new Glow(Color.White, 3).SetIntensity(1.3f).SetFlicker(0.6f, 19f),
            new Outline(new Color(10, 20, 60), 2)
        };

        /// <summary>A sunset: a purple shadow, a pink glow that breathes, an orange glow, a dark outline and a warm shine.</summary>
        private static List<ShadingEffect> Sunset() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(0, 6), 8, new Color(40, 0, 60) * 0.8f),
            new Glow(new Color(255, 60, 140), 26).SetIntensity(1.4f).SetBreathe(0.4f, 0.06f),
            new Glow(new Color(255, 150, 40), 10).SetIntensity(1.6f).SetPulse(0.5f, 0.3f),
            new Outline(new Color(90, 10, 60), 2.5f),
            new Shine(new Color(255, 230, 170)).SetTiming(1.1f, 3.5f).SetWidth(30)
        };

        /// <summary>Retro wave: two sharp colored shadows that wobble and a pink glow.</summary>
        private static List<ShadingEffect> RetroWave() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(4, 4), 0, new Color(60, 220, 255)).SetOrbit(1.5f, 0.6f),
            new Shadow(new Vector2(2, 2), 0, new Color(255, 60, 170)).SetOrbit(1f, -0.9f),
            new Glow(new Color(255, 60, 200), 22).SetIntensity(1.2f).SetBreathe(0.5f, 0.05f)
        };

        private static List<ShadingEffect> ComicShine()
        {
            var effects = ShadingPresets.Comic();
            effects.Add(new Shine(new Color(255, 255, 220)).SetTiming(0.6f, 2.5f).SetWidth(26));
            return effects;
        }

        private static List<ShadingEffect> CandyShine()
        {
            var effects = ShadingPresets.Candy();
            effects.Add(new Shine(Color.White).SetTiming(0.7f, 2.8f).SetWidth(24).SetIntensity(0.9f));
            return effects;
        }

        private static List<ShadingEffect> StickerShine()
        {
            var effects = ShadingPresets.Sticker();
            effects.Add(new Shine(Color.White).SetTiming(0.7f, 3f).SetWidth(30).SetIntensity(0.8f));
            return effects;
        }

        /// <summary>A golden star: a warm halo that breathes, a white border and a shine.</summary>
        private static List<ShadingEffect> GoldenStar() => new List<ShadingEffect>
        {
            new Glow(new Color(255, 150, 20), 26).SetIntensity(1.6f).SetBreathe(0.8f, 0.08f),
            new Glow(new Color(255, 230, 120), 8).SetIntensity(1.8f).SetPulse(0.8f, 0.4f),
            new Outline(new Color(255, 250, 220), 2.5f),
            new Shine(Color.White).SetTiming(0.7f, 2.4f).SetWidth(36)
        };

        /// <summary>A coin: a golden halo that pulses and a shine that runs over each frame of the animation.</summary>
        private static List<ShadingEffect> ShinyCoin() => new List<ShadingEffect>
        {
            new Glow(new Color(255, 170, 30), 20).SetIntensity(1.6f).SetPulse(1f, 0.35f),
            new Glow(new Color(255, 230, 120), 6).SetIntensity(1.4f),
            new Shine(Color.White).SetTiming(0.5f, 1.6f).SetWidth(30)
        };

        /// <summary>A torch: the three flickering glows of the fire, rising above the flame.</summary>
        private static List<ShadingEffect> BurningTorch() => new List<ShadingEffect>
        {
            new Glow(ShadingPresets.FireColors[0], 34).SetOffset(0, -8).SetIntensity(1.6f).SetFlicker(0.55f, 6f).SetBreathe(1.4f, 0.08f),
            new Glow(ShadingPresets.FireColors[1], 16).SetOffset(0, -4).SetIntensity(1.8f).SetFlicker(0.45f, 9f).SetTimeOffset(1.1f),
            new Glow(ShadingPresets.FireColors[2], 6).SetIntensity(1.6f).SetFlicker(0.35f, 13f).SetTimeOffset(2.3f)
        };

        /// <summary>An orb of plasma: two glows turning in opposite directions, a white bloom over it that breathes.</summary>
        private static List<ShadingEffect> PlasmaOrb() => new List<ShadingEffect>
        {
            new Glow(new Color(40, 220, 255), 28).SetIntensity(1.6f).SetOrbit(9, 0.8f),
            new Glow(new Color(255, 40, 220), 28).SetIntensity(1.6f).SetOrbit(9, -0.8f),
            new Outline(new Color(200, 160, 255), 2).SetPulse(1.6f, 0.6f),
            new Glow(new Color(200, 170, 255), 16).SetLayer(ShadingLayer.Front).SetIntensity(0.7f).SetBreathe(0.9f, 0.1f).SetPulse(0.9f, 0.5f)
        };

        private static List<ShadingEffect> ParagraphEffects() => new List<ShadingEffect>
        {
            new Shadow(new Vector2(0, 3), 5, Color.Black * 0.7f),
            new Outline(new Color(10, 30, 70), 2)
        };
    }
}
