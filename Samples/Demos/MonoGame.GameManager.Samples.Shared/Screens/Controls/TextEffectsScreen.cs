using FontStashSharp;
using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Controls.Shading;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.GameManager.Samples.Screens.Controls
{
    /// <summary>
    /// Casual game titles: a label painted with a gradient, outlined several times, beveled, shaded and lit. A preview
    /// whose text, font and every effect can be changed live, and a gallery of ready-made styles (click one to edit it).
    /// </summary>
    public class TextEffectsScreen : Screen
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
        private const int TileHeight = 112;
        private const int TileGap = 10;
        private const int CaptionHeight = 22;

        private static readonly Color TabColor = new Color(60, 60, 60);
        private static readonly Color SelectedTabColor = new Color(80, 160, 230);
        private static readonly Color ActionColor = new Color(70, 110, 70);
        private static readonly Color SmallButtonBorder = new Color(110, 110, 110);
        private static readonly Color HeaderColor = new Color(120, 220, 255);
        private static readonly string[] TabNames = { "Text", "Fill", "Outline", "Inner", "Detail", "Shadow", "Glow", "Shine", "Motion" };
        private static readonly string[] OverlayKindNames = { "Band (gloss)", "Stripes" };
        private static readonly string[] PatternNames = Enum.GetNames(typeof(ShadingPattern));

        private static readonly List<Color> DetailColors = new List<Color>
        {
            Color.White, new Color(255, 245, 200), new Color(255, 200, 60), new Color(255, 110, 40), new Color(255, 60, 160),
            new Color(170, 90, 255), new Color(60, 200, 255), new Color(90, 40, 0), Color.Black
        };
        private static readonly string[] Texts =
        {
            "Map", "VICTORY", "DEFEAT", "LEVEL UP", "+100", "COMBO x3", "GAME OVER", "-250", "Play", "New Game", "Treasure", "Boss Fight", "READY?"
        };
        private static readonly string[] BoundsNames = { "Glyphs", "Line height" };
        private static readonly string[] CycleNames = { "None", "Rainbow", "Neon", "Fire", "Magic", "Ice", "Gold" };
        private static readonly IList<Color>[] CyclePalettes =
        {
            null,
            ShadingPresets.RainbowColors,
            new[] { new Color(40, 240, 255), new Color(255, 60, 220) },
            ShadingPresets.FireColors,
            ShadingPresets.MagicColors,
            new[] { new Color(60, 140, 255), new Color(160, 230, 255), Color.White },
            new[] { new Color(255, 200, 40), new Color(255, 245, 190), new Color(255, 150, 20) }
        };

        private static readonly List<Color> FillColors = new List<Color>
        {
            new Color(255, 252, 220), new Color(255, 220, 90), new Color(242, 150, 22), new Color(230, 40, 40), new Color(255, 100, 190),
            new Color(170, 80, 255), new Color(60, 160, 255), new Color(90, 220, 110), new Color(110, 60, 30)
        };

        private static readonly List<Color> TextColors = new List<Color>
        {
            Color.White, new Color(255, 220, 40), new Color(255, 225, 120), new Color(255, 90, 170), new Color(120, 220, 255),
            new Color(150, 255, 170), new Color(255, 120, 60), new Color(60, 60, 70), new Color(150, 150, 160)
        };

        private static readonly List<Color> DarkColors = new List<Color>
        {
            Color.Black, new Color(75, 32, 6), new Color(20, 40, 90), new Color(90, 10, 0), new Color(40, 0, 70),
            new Color(5, 50, 25), Color.White, new Color(255, 220, 40), new Color(60, 200, 255)
        };

        private static readonly List<Color> LightColors = new List<Color>
        {
            Color.White, new Color(255, 245, 200), new Color(255, 190, 60), new Color(255, 110, 40), new Color(255, 60, 60),
            new Color(255, 60, 200), new Color(170, 90, 255), new Color(60, 200, 255), new Color(60, 230, 120)
        };

        private static readonly List<Color> StageColors = new List<Color>
        {
            new Color(12, 12, 22), new Color(40, 25, 60), new Color(55, 105, 150), new Color(20, 50, 90), new Color(40, 140, 230),
            new Color(60, 140, 80), new Color(150, 60, 50), new Color(150, 150, 160), new Color(230, 232, 240)
        };

        /// <summary>Ready-made gradients of the Fill page.</summary>
        private static readonly (string Name, Color[] Colors)[] Palettes =
        {
            ("Gold", new[] { new Color(255, 252, 220), new Color(255, 222, 100), new Color(242, 150, 22) }),
            ("Silver", new[] { new Color(250, 252, 255), new Color(170, 180, 195), new Color(80, 88, 100) }),
            ("Fire", new[] { new Color(255, 250, 160), new Color(255, 140, 20), new Color(200, 20, 0) }),
            ("Ice", new[] { Color.White, new Color(150, 230, 255), new Color(40, 130, 230) }),
            ("Candy", new[] { new Color(255, 200, 235), new Color(255, 90, 180), new Color(220, 30, 140) }),
            ("Emerald", new[] { new Color(190, 255, 210), new Color(40, 200, 110), new Color(0, 110, 60) }),
            ("Ruby", new[] { new Color(255, 170, 190), new Color(225, 20, 60), new Color(110, 0, 30) }),
            ("Magic", new[] { new Color(255, 200, 255), new Color(190, 90, 255), new Color(90, 20, 170) }),
            ("Toxic", new[] { new Color(220, 255, 120), new Color(90, 200, 40), new Color(20, 90, 20) }),
            ("Sunset", new[] { new Color(255, 240, 120), new Color(255, 120, 60), new Color(150, 40, 150) }),
            ("Chocolate", new[] { new Color(190, 120, 70), new Color(110, 60, 30), new Color(70, 35, 15) }),
            ("Ocean", new[] { new Color(180, 250, 255), new Color(40, 170, 230), new Color(10, 60, 140) })
        };

        private TextStyle selected;
        private TextStyle style;
        private Panel stage;
        private RectangleControl stageBackground;
        private Label preview;
        private Label infoLabel;
        private Button[] tabButtons;
        private ScrollViewer[] pages;
        private int selectedTab;
        private int row;
        private ScrollViewer gallery;
        private readonly List<GalleryTile> galleryTiles = new List<GalleryTile>();
        private const float GalleryPreload = 260f;
        private const float GalleryRelease = 700f;

        public static void OpenTextEffectsScreen() => ServiceProvider.ScreenManager.ChangeScreen(new TextEffectsScreen());

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Text Effects", OpenTextEffectsScreen)
            });

            CreatePreviewSection();
            CreateOptionsSection();
            new RectangleControl(new Rectangle(SectionDivisionLeft, SectionTop, 2, SectionHeight), Color.White)
                .AddToScreen();

            SelectStyle(Gallery[0]);
            ShowTab(0);
            base.OnInit();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            infoLabel.Text = $"{preview.ShadingEffects.Count} effects, {FontName(style.FontIndex)}: {style.Caption}";
            UpdateGalleryEffects();
        }

        /// <summary>
        /// The tiles near the visible part of the gallery get their effects; the far ones release them (and their render
        /// targets), so a long gallery stays light.
        /// </summary>
        private void UpdateGalleryEffects()
        {
            var visibleTop = -gallery.ScrollPosition.Y;
            var visibleBottom = visibleTop + gallery.SizeWithoutScale.Y;
            foreach (var tile in galleryTiles)
            {
                var near = tile.Top + TileHeight > visibleTop - GalleryPreload && tile.Top < visibleBottom + GalleryPreload;
                if (near && !tile.HasEffects)
                {
                    tile.Label.SetShadings(tile.Item.CreateEffects());
                    tile.HasEffects = true;
                }
                else if (!near && tile.HasEffects && (tile.Top + TileHeight < visibleTop - GalleryRelease || tile.Top > visibleBottom + GalleryRelease))
                {
                    tile.Label.ClearShadings();
                    tile.HasEffects = false;
                }
            }
        }

        public override void Dispose()
        {
            ServiceProvider.Clock.TimeScale = 1f;
            base.Dispose();
        }

        private static IReadOnlyList<(string Name, SpriteFontBase Font)> Fonts => ContentHandler.Instance.Fonts;

        private static string FontName(int index) => Fonts[MathHelper.Clamp(index, 0, Fonts.Count - 1)].Name;

        private static SpriteFontBase FontOf(TextStyle item) => Fonts[MathHelper.Clamp(item.FontIndex, 0, Fonts.Count - 1)].Font;

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
            preview = new Label(font, Texts[0], Vector2.Zero, Color.White)
                .SetAnchor(Anchor.Center)
                .AddToScreen(stage);

            var infoTop = StageTop + StageHeight + 6;
            infoLabel = new Label(font, string.Empty, new Vector2(0, infoTop), Color.White)
                .SetScale(0.6f)
                .AddToScreen(container);
            new FpsCounter(font, new Vector2(PreviewWidth - 90, infoTop), Color.Yellow)
                .SetScale(0.6f)
                .AddToScreen(container);

            CreateGallery(container);
        }

        /// <summary>Applies the edited style to the preview (its text, its font and its effects).</summary>
        private void ApplyStyle()
        {
            stageBackground.Color = style.Stage;
            ApplyTo(preview, style, style.Scale);
        }

        private static void ApplyTo(Label label, TextStyle item, float scale, bool withEffects = true)
        {
            label.SetFont(FontOf(item))
                .SetText(item.Text)
                .SetCharacterSpacing(item.Spacing)
                .SetColor(item.TextColor)
                .SetOutline(item.BuiltInOutlineColor, item.Fill.On ? 0f : item.BuiltInOutline)
                .SetScale(scale)
                .SetOriginRate(new Vector2(0.5f))
                .SetRotationInDegree(item.Rotation)
                .SetOpacity(item.Opacity)
                .SetShadings(withEffects ? item.CreateEffects() : null);
        }

        private void SelectStyle(TextStyle item)
        {
            selected = item;
            style = item.Clone();
            style.Scale = Math.Min(style.PreviewScale, FitScale(style, new Vector2(PreviewWidth - 40, StageHeight - 30), 3.2f));
            ApplyStyle();
            RebuildPages();
        }

        /// <summary>The largest scale that fits a style, its effects included, in an area.</summary>
        private static float FitScale(TextStyle item, Vector2 room, float maximum)
        {
            var font = FontOf(item);
            var size = new Vector2(font.MeasureString(item.Text, characterSpacing: item.Spacing).X, font.LineHeight);

            // Decorative fonts draw swashes past the measured width: fit the glyphs as drawn, centered on the label.
            var left = 0f;
            var right = size.X;
            foreach (var glyph in font.GetGlyphs(item.Text, Vector2.Zero, Vector2.Zero, null, item.Spacing))
            {
                if (glyph.Bounds.Width <= 0)
                    continue;
                left = Math.Min(left, glyph.Bounds.X);
                right = Math.Max(right, glyph.Bounds.X + glyph.Bounds.Width);
            }

            size.X += 2f * Math.Max(-left, right - size.X);
            var extent = item.GetExtent() * 2f;
            var scale = Math.Min((room.X) / (size.X + extent), room.Y / (size.Y + extent));
            if (item.Rotation != 0f)
                scale *= 0.9f;
            return MathHelper.Clamp(scale, 0.2f, maximum);
        }

        // ---- Gallery

        private void CreateGallery(Panel container)
        {
            var font = ContentHandler.Instance.Font;
            var frame = new Panel(new Rectangle(0, GalleryTop, PreviewWidth, SectionHeight - GalleryTop))
                .SetHideOverflow(true)
                .AddToScreen(container);
            new RectangleControl(Vector2.Zero, frame.Size, new Color(45, 40, 75))
                .AddToScreen(frame);
            new Label(font, $"Gallery ({Gallery.Length} styles): scroll for more, click one to edit it", new Vector2(12, 6), Color.Yellow)
                .SetScale(0.6f)
                .AddToScreen(frame);

            gallery = new ScrollViewer(new Vector2(0, 28), new Vector2(PreviewWidth, frame.Size.Y - 28))
                .SetHideOverflow(true)
                .AddToScreen(frame);
            var content = gallery.ContentPanel;

            // The layered titles first, then the classic still and animated styles, then the themes.
            var sections = Gallery
                .GroupBy(item => item.Section ?? (item.IsAnimated ? "Animated" : "Static"))
                .Select(group => (group.Key, group.ToArray()))
                .OrderBy(section => section.Key.StartsWith("Layered", StringComparison.Ordinal) ? 0 : 1)
                .ToList();

            var left = (PreviewWidth - 3 * TileWidth - 2 * TileGap) / 2;
            var top = 4f;
            foreach (var section in sections)
            {
                new Label(font, $"{section.Item1} ({section.Item2.Length})", new Vector2(left, top), new Color(200, 200, 235))
                    .SetScale(0.6f)
                    .AddToScreen(content);
                top += 24f;
                for (var i = 0; i < section.Item2.Length; i++)
                    AddGalleryTile(content, section.Item2[i], new Vector2(left + i % 3 * (TileWidth + TileGap), top + i / 3 * (TileHeight + TileGap)));
                top += (section.Item2.Length + 2) / 3 * (TileHeight + TileGap) + 8f;
            }

            // Some space under the last row.
            new RectangleControl(new Rectangle(0, (int)top, 1, 6), Color.Transparent)
                .AddToScreen(content);
        }

        private void AddGalleryTile(Panel content, TextStyle item, Vector2 position)
        {
            var tile = new Panel(new Rectangle(position.ToPoint(), new Point(TileWidth, TileHeight)))
                .SetHideOverflow(true)
                .AddOnClick(args => SelectStyle(item))
                .AddToScreen(content);
            new RectangleControl(Vector2.Zero, tile.Size, item.Stage)
                .AddToScreen(tile);

            var room = new Vector2(TileWidth - 12, TileHeight - CaptionHeight - 8);
            var label = new Label(FontOf(item), item.Text, Vector2.Zero, Color.White)
                .SetAnchor(Anchor.Center)
                .SetPosition(0, -CaptionHeight / 2 + 2)
                .AddToScreen(tile);
            // The effects are added when the tile comes near the visible part of the gallery (UpdateGalleryEffects).
            ApplyTo(label, item, Math.Min(item.TileScale, FitScale(item, room, 1.6f)), withEffects: false);
            galleryTiles.Add(new GalleryTile { Label = label, Item = item, Top = position.Y });

            new Label(ContentHandler.Instance.Font, item.Caption, Vector2.Zero, IsLight(item.Stage) ? new Color(40, 40, 50) : new Color(220, 220, 235))
                .SetScale(0.5f)
                .SetAnchor(Anchor.BottomCenter)
                .SetPosition(0, 3)
                .AddToScreen(tile);
        }

        private static bool IsLight(Color color) => color.R + color.G + color.B > 450;

        /// <summary>A tile of the gallery: its label gets its effects only while it is near the visible area.</summary>
        private sealed class GalleryTile
        {
            public Label Label;
            public TextStyle Item;
            public float Top;
            public bool HasEffects;
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
            const float tabWidth = 54f;
            for (var i = 0; i < TabNames.Length; i++)
            {
                var index = i;
                var textScale = Math.Min(0.6f, (tabWidth - 10f) / font.MeasureString(TabNames[i]).X);
                tabButtons[i] = CreateSmallButton(container, TabNames[i], new Vector2(i * (tabWidth + 2f), 36), new Vector2(tabWidth, 32), TabColor, textScale, () => ShowTab(index));
                pages[i] = new ScrollViewer(new Vector2(0, PagesTop), new Vector2(OptionsWidth, ActionsTop - PagesTop - 8))
                    .SetHideOverflow(true)
                    .SetIsVisible(false)
                    .AddToScreen(container);
            }

            var actionSize = new Vector2(120, 36);
            CreateSmallButton(container, "Reset", new Vector2(0, ActionsTop), actionSize, ActionColor, 0.65f, () => SelectStyle(selected));
            CreateSmallButton(container, "Random", new Vector2(130, ActionsTop), actionSize, ActionColor, 0.65f, Randomize);
            CreateSmallButton(container, "Random font", new Vector2(260, ActionsTop), actionSize, ActionColor, 0.6f, () => Change(s => s.FontIndex = ServiceProvider.Random.Next(0, Fonts.Count - 1), true));
            CreateSmallButton(container, "Plain text", new Vector2(390, ActionsTop), actionSize, new Color(120, 60, 60), 0.6f, () =>
            {
                style = new TextStyle(style.Caption, style.Text, style.FontIndex) { Scale = style.Scale, Stage = style.Stage, TextColor = Color.White };
                ApplyStyle();
                RebuildPages();
            });
        }

        private void Randomize()
        {
            var random = ServiceProvider.Random;
            var item = Gallery[random.Next(0, Gallery.Length - 1)];
            SelectStyle(item);
            style.FontIndex = random.Next(1, Fonts.Count - 1);
            style.Text = Texts[random.Next(0, Texts.Length - 1)];
            style.Stage = StageColors[random.Next(0, StageColors.Count - 1)];
            style.Caption = $"random {item.Caption}";
            style.Scale = Math.Min(3f, FitScale(style, new Vector2(PreviewWidth - 40, StageHeight - 30), 3.2f));
            ApplyStyle();
            RebuildPages();
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

        /// <summary>Changes the edited style and applies it (and rebuilds the pages when the change shows in other rows).</summary>
        private void Change(Action<TextStyle> change, bool rebuildPages = false)
        {
            change(style);
            ApplyStyle();
            if (rebuildPages)
                RebuildPages();
        }

        private void RebuildPages()
        {
            if (pages == null)
                return;

            foreach (var page in pages)
                page.ClearChildren();

            BuildTextPage(pages[0].ContentPanel);
            BuildFillPage(pages[1].ContentPanel);
            BuildOutlinePage(pages[2].ContentPanel);
            BuildInnerPage(pages[3].ContentPanel);
            BuildDetailPage(pages[4].ContentPanel);
            BuildShadowPage(pages[5].ContentPanel);
            BuildGlowPage(pages[6].ContentPanel);
            BuildShinePage(pages[7].ContentPanel);
            BuildMotionPage(pages[8].ContentPanel);
            ShowTab(selectedTab);
        }

        private void BuildTextPage(Panel page)
        {
            row = 0;
            var texts = Texts.Contains(style.Text) ? Texts : new[] { style.Text }.Concat(Texts).ToArray();
            Choice(page, "Text", texts, Array.IndexOf(texts, style.Text), index => Change(s => s.Text = texts[index]));
            Choice(page, "Font", Fonts.Select(font => font.Name).ToList(), style.FontIndex, index => Change(s => s.FontIndex = index));
            Slider(page, "Size (scale)", 0.4f, 4f, style.Scale, value => Change(s => s.Scale = value), 0.05f, "{0:0.00}");
            Slider(page, "Rotation", -45f, 45f, style.Rotation, value => Change(s => s.Rotation = value), 1f, "{0:0}°");
            Slider(page, "Opacity", 0f, 1f, style.Opacity, value => Change(s => s.Opacity = value), 0.05f, "{0:0.00}");
            Slider(page, "Letter spacing", -6f, 20f, style.Spacing, value => Change(s => s.Spacing = value), 0.5f, "{0:0.#}");
            Colors(page, "Stage", StageColors, color => Change(s => s.Stage = color));
            Section(page, "Plain text (without a fill)");
            Colors(page, "   Text color", TextColors, color => Change(s => s.TextColor = color));
            Slider(page, "   Built-in outline", 0f, 8f, style.BuiltInOutline, value => Change(s => s.BuiltInOutline = value), 0.5f, "{0:0.#}");
            Colors(page, "   Outline color", DarkColors, color => Change(s => s.BuiltInOutlineColor = color));
            Note(page, "The built-in outline (Label.SetOutline) is part of the silhouette, so a fill would cover it: with a fill, use the Outline page.");
        }

        private void BuildFillPage(Panel page)
        {
            row = 0;
            var fill = style.Fill;
            Check(page, "Gradient fill", fill.On, value => Change(s => s.Fill.On = value));
            Choice(page, "Palette", Palettes.Select(palette => palette.Name).ToList(), Array.FindIndex(Palettes, palette => palette.Colors.SequenceEqual(fill.Colors)), index =>
                Change(s => s.Fill.Colors = (Color[])Palettes[index].Colors.Clone(), true));
            Colors(page, "Top color", FillColors, color => Change(s => s.Fill.Colors[0] = color));
            Colors(page, "Middle color", FillColors, color => Change(s => s.Fill.Colors[1] = color));
            Colors(page, "Bottom color", FillColors, color => Change(s => s.Fill.Colors[s.Fill.Colors.Length - 1] = color));
            Slider(page, "Middle at", 0.05f, 0.95f, fill.Middle, value => Change(s => s.Fill.Middle = value), 0.05f, "{0:0.00}");
            Slider(page, "Angle", 0f, 360f, fill.Angle, value => Change(s => s.Fill.Angle = value), 5f, "{0:0}°");
            Slider(page, "Start", -0.5f, 1f, fill.Start, value => Change(s => s.Fill.Start = value), 0.05f, "{0:0.00}");
            Slider(page, "End", 0f, 1.5f, fill.End, value => Change(s => s.Fill.End = value), 0.05f, "{0:0.00}");
            Slider(page, "Hardness", 0f, 1f, fill.Hardness, value => Change(s => s.Fill.Hardness = value), 0.05f, "{0:0.00}");
            Choice(page, "Bounds", BoundsNames, fill.LineBounds ? 1 : 0, index => Change(s => s.Fill.LineBounds = index == 1));
            Slider(page, "Scroll speed", -1f, 1f, fill.Scroll, value => Change(s => s.Fill.Scroll = value), 0.05f, "{0:0.00}");
            Note(page, "The fill replaces the colors of the label. Angle 90 goes from the top to the bottom; a scrolling fill repeats its colors.");
        }

        private void BuildOutlinePage(Panel page)
        {
            row = 0;
            for (var i = 0; i < style.Outlines.Length; i++)
            {
                var index = i;
                var outline = style.Outlines[i];
                Section(page, i == 0 ? "Outline 1 (outer)" : $"Outline {i + 1}");
                Check(page, "   On", outline.On, value => Change(s => s.Outlines[index].On = value));
                Colors(page, "   Color", DarkColors, color => Change(s => s.Outlines[index].Color = color));
                Slider(page, "   Thickness", 0.5f, 14f, outline.Thickness, value => Change(s => s.Outlines[index].Thickness = value), 0.5f, "{0:0.#}");
                Slider(page, "   Softness", 0f, 8f, outline.Softness, value => Change(s => s.Outlines[index].Softness = value), 0.5f, "{0:0.#}");
            }

            Note(page, "Outlines are drawn from the thickest to the thinnest, so stacked outlines show as rings.");
        }

        private void BuildInnerPage(Panel page)
        {
            row = 0;
            var bevel = style.Bevel;
            Section(page, "Bevel (light top, dark bottom)");
            Check(page, "   On", bevel.On, value => Change(s => s.Bevel.On = value));
            Colors(page, "   Highlight", LightColors, color => Change(s => s.Bevel.Highlight = color));
            Colors(page, "   Shade", DarkColors, color => Change(s => s.Bevel.Shade = color));
            Slider(page, "   Depth", 0.5f, 8f, bevel.Depth, value => Change(s => s.Bevel.Depth = value), 0.25f, "{0:0.##}");
            Slider(page, "   Softness", 0f, 6f, bevel.Softness, value => Change(s => s.Bevel.Softness = value), 0.25f, "{0:0.##}");
            Slider(page, "   Light amount", 0f, 1f, bevel.HighlightStrength, value => Change(s => s.Bevel.HighlightStrength = value), 0.05f, "{0:0.00}");
            Slider(page, "   Shade amount", 0f, 1f, bevel.ShadeStrength, value => Change(s => s.Bevel.ShadeStrength = value), 0.05f, "{0:0.00}");

            var inner = style.InnerShadow;
            Section(page, "Inner shadow");
            Check(page, "   On", inner.On, value => Change(s => s.InnerShadow.On = value));
            Colors(page, "   Color", DarkColors, color => Change(s => s.InnerShadow.Color = color));
            Slider(page, "   Offset Y", -8f, 8f, inner.OffsetY, value => Change(s => s.InnerShadow.OffsetY = value), 0.5f, "{0:0.#}");
            Slider(page, "   Blur", 0f, 8f, inner.Blur, value => Change(s => s.InnerShadow.Blur = value), 0.5f, "{0:0.#}");
            Slider(page, "   Strength", 0f, 1f, inner.Strength, value => Change(s => s.InnerShadow.Strength = value), 0.05f, "{0:0.00}");

            var glow = style.InnerGlow;
            Section(page, "Inner glow");
            Check(page, "   On", glow.On, value => Change(s => s.InnerGlow.On = value));
            Colors(page, "   Color", LightColors, color => Change(s => s.InnerGlow.Color = color));
            Slider(page, "   Radius", 0.5f, 12f, glow.Radius, value => Change(s => s.InnerGlow.Radius = value), 0.5f, "{0:0.#}");
            Slider(page, "   Intensity", 0f, 2f, glow.Intensity, value => Change(s => s.InnerGlow.Intensity = value), 0.05f, "{0:0.00}");
        }

        private void BuildShadowPage(Panel page)
        {
            row = 0;
            for (var i = 0; i < style.Shadows.Length; i++)
            {
                var index = i;
                var shadow = style.Shadows[i];
                Section(page, $"Shadow {i + 1}");
                Check(page, "   On", shadow.On, value => Change(s => s.Shadows[index].On = value));
                Colors(page, "   Color", DarkColors, color => Change(s => s.Shadows[index].Color = color));
                Slider(page, "   Opacity", 0f, 1f, shadow.Opacity, value => Change(s => s.Shadows[index].Opacity = value), 0.05f, "{0:0.00}");
                Slider(page, "   Offset X", -16f, 16f, shadow.Offset.X, value => Change(s => s.Shadows[index].Offset.X = value), 0.5f, "{0:0.#}");
                Slider(page, "   Offset Y", -16f, 16f, shadow.Offset.Y, value => Change(s => s.Shadows[index].Offset.Y = value), 0.5f, "{0:0.#}");
                Slider(page, "   Blur", 0f, 20f, shadow.Blur, value => Change(s => s.Shadows[index].Blur = value), 0.5f, "{0:0.#}");
                Slider(page, "   Spread", 0f, 10f, shadow.Spread, value => Change(s => s.Shadows[index].Spread = value), 0.5f, "{0:0.#}");
            }
        }

        private void BuildGlowPage(Panel page)
        {
            row = 0;
            for (var i = 0; i < style.Glows.Length; i++)
            {
                var index = i;
                var glow = style.Glows[i];
                Section(page, $"Glow {i + 1}");
                Check(page, "   On", glow.On, value => Change(s => s.Glows[index].On = value));
                Colors(page, "   Color", LightColors, color => Change(s => s.Glows[index].Color = color));
                Slider(page, "   Radius", 1f, 40f, glow.Radius, value => Change(s => s.Glows[index].Radius = value), 1f, "{0:0}");
                Slider(page, "   Spread", 0f, 8f, glow.Spread, value => Change(s => s.Glows[index].Spread = value), 0.5f, "{0:0.#}");
                Slider(page, "   Intensity", 0f, 3f, glow.Intensity, value => Change(s => s.Glows[index].Intensity = value), 0.05f, "{0:0.00}");
                Check(page, "   In front (bloom)", glow.Front, value => Change(s => s.Glows[index].Front = value));
            }
        }

        private void BuildShinePage(Panel page)
        {
            row = 0;
            var shine = style.Shine;
            Check(page, "Shine", shine.On, value => Change(s => s.Shine.On = value));
            Colors(page, "Color", LightColors, color => Change(s => s.Shine.Color = color));
            Slider(page, "Width", 6f, 80f, shine.Width, value => Change(s => s.Shine.Width = value), 1f, "{0:0}");
            Slider(page, "Angle", -60f, 60f, shine.Angle, value => Change(s => s.Shine.Angle = value), 1f, "{0:0}°");
            Slider(page, "Duration", 0.2f, 3f, shine.Duration, value => Change(s => s.Shine.Duration = value), 0.1f, "{0:0.0} s");
            Slider(page, "Interval", 0.5f, 8f, shine.Interval, value => Change(s => s.Shine.Interval = value), 0.1f, "{0:0.0} s");
            Slider(page, "Intensity", 0f, 2f, shine.Intensity, value => Change(s => s.Shine.Intensity = value), 0.05f, "{0:0.00}");
        }

        private void BuildDetailPage(Panel page)
        {
            row = 0;
            for (var i = 0; i < style.Overlays.Length; i++)
            {
                var index = i;
                var overlay = style.Overlays[i];
                Section(page, $"Overlay {i + 1} (gradient over the letters)");
                Check(page, "   On", overlay.On, value => Change(s => s.Overlays[index].On = value));
                Choice(page, "   Kind", OverlayKindNames, overlay.Stripes ? 1 : 0, choice => Change(s => s.Overlays[index].Stripes = choice == 1));
                Colors(page, "   Color", DetailColors, color => Change(s => s.Overlays[index].Color = color));
                Slider(page, "   Start strength", 0f, 1f, overlay.StartStrength, value => Change(s => s.Overlays[index].StartStrength = value), 0.05f, "{0:0.00}");
                Slider(page, "   End strength", 0f, 1f, overlay.EndStrength, value => Change(s => s.Overlays[index].EndStrength = value), 0.05f, "{0:0.00}");
                Slider(page, "   Start", -0.5f, 1f, overlay.Start, value => Change(s => s.Overlays[index].Start = value), 0.02f, "{0:0.00}");
                Slider(page, "   End", 0f, 1.5f, overlay.End, value => Change(s => s.Overlays[index].End = value), 0.02f, "{0:0.00}");
                Slider(page, "   Edge softness", 0f, 0.45f, overlay.Softness, value => Change(s => s.Overlays[index].Softness = value), 0.01f, "{0:0.00}");
                Slider(page, "   Angle", 0f, 360f, overlay.Angle, value => Change(s => s.Overlays[index].Angle = value), 5f, "{0:0}Â°");
                Check(page, "   Added as light", overlay.Light, value => Change(s => s.Overlays[index].Light = value));
                Slider(page, "   Scroll speed", -1f, 1f, overlay.Scroll, value => Change(s => s.Overlays[index].Scroll = value), 0.05f, "{0:0.00}");
            }

            var pattern = style.Pattern;
            Section(page, "Pattern (tiled over the letters)");
            Check(page, "   On", pattern.On, value => Change(s => s.Pattern.On = value));
            Choice(page, "   Pattern", PatternNames, (int)pattern.Kind, choice => Change(s => s.Pattern.Kind = (ShadingPattern)choice));
            Colors(page, "   Color", DetailColors, color => Change(s => s.Pattern.Color = color));
            Slider(page, "   Strength", 0f, 1f, pattern.Strength, value => Change(s => s.Pattern.Strength = value), 0.05f, "{0:0.00}");
            Slider(page, "   Tile size", 2f, 40f, pattern.TileSize, value => Change(s => s.Pattern.TileSize = value), 1f, "{0:0}");
            Check(page, "   Added as light", pattern.Light, value => Change(s => s.Pattern.Light = value));
            Slider(page, "   Scroll X", -30f, 30f, pattern.Scroll.X, value => Change(s => s.Pattern.Scroll.X = value), 1f, "{0:0}");
            Slider(page, "   Scroll Y", -30f, 30f, pattern.Scroll.Y, value => Change(s => s.Pattern.Scroll.Y = value), 1f, "{0:0}");

            var sparkles = style.Sparkles;
            Section(page, "Sparkles (star glints on the letters)");
            Check(page, "   On", sparkles.On, value => Change(s => s.Sparkles.On = value));
            Colors(page, "   Color", DetailColors, color => Change(s => s.Sparkles.Color = color));
            Slider(page, "   Count", 1f, 40f, sparkles.Count, value => Change(s => s.Sparkles.Count = (int)value), 1f, "{0:0}");
            Slider(page, "   Size", 4f, 48f, sparkles.Size, value => Change(s => s.Sparkles.Size = value), 0.5f, "{0:0.#}");
            Slider(page, "   Twinkle", 0f, 3f, sparkles.Twinkle, value => Change(s => s.Sparkles.Twinkle = value), 0.1f, "{0:0.0}/s");
            Slider(page, "   Seed", 1f, 50f, sparkles.Seed, value => Change(s => s.Sparkles.Seed = (int)value), 1f, "{0:0}");
        }

        private void BuildMotionPage(Panel page)
        {
            row = 0;
            for (var i = 0; i < style.Glows.Length; i++)
            {
                var index = i;
                var glow = style.Glows[i];
                Section(page, $"Glow {i + 1}" + (glow.On ? string.Empty : " (off)"));
                Slider(page, "   Pulse speed", 0f, 4f, glow.PulseSpeed, value => Change(s => s.Glows[index].PulseSpeed = value), 0.1f, "{0:0.0}/s");
                Slider(page, "   Pulse amount", 0f, 1f, glow.PulseAmount, value => Change(s => s.Glows[index].PulseAmount = value), 0.05f, "{0:0.00}");
                Slider(page, "   Flicker", 0f, 1f, glow.Flicker, value => Change(s => s.Glows[index].Flicker = value), 0.05f, "{0:0.00}");
                Slider(page, "   Breathe", 0f, 0.2f, glow.Breathe, value => Change(s => s.Glows[index].Breathe = value), 0.01f, "{0:0.00}");
                Choice(page, "   Color cycle", CycleNames, Array.IndexOf(CyclePalettes, glow.Cycle), choice => Change(s => s.Glows[index].Cycle = CyclePalettes[choice]));
            }

            Section(page, "Outline 1");
            Choice(page, "   Color cycle", CycleNames, Array.IndexOf(CyclePalettes, style.Outlines[0].Cycle), choice => Change(s => s.Outlines[0].Cycle = CyclePalettes[choice]));
            Section(page, "Shadow 1");
            Slider(page, "   Orbit radius", 0f, 10f, style.Shadows[0].Orbit, value => Change(s => s.Shadows[0].Orbit = value), 0.5f, "{0:0.#}");
            Section(page, "Bevel");
            Slider(page, "   Light orbit", 0f, 4f, style.Bevel.Orbit, value => Change(s => s.Bevel.Orbit = value), 0.25f, "{0:0.##}");
            Section(page, "Fill");
            Slider(page, "   Scroll speed", -1f, 1f, style.Fill.Scroll, value => Change(s => s.Fill.Scroll = value), 0.05f, "{0:0.00}");
            Slider(page, "Game speed", 0f, 2f, ServiceProvider.Clock.TimeScale, value => ServiceProvider.Clock.TimeScale = value, 0.05f, "{0:0.00}x");
        }

        // ---- Rows

        private float NextRow() => row++ * RowHeight;

        private void Slider(Panel page, string text, float minimum, float maximum, float value, Action<float> onChanged, float step, string format)
            => SliderOption.CreateSliderOption(page, text, NextRow(), minimum, maximum, MathHelper.Clamp(value, minimum, maximum), onChanged, step, format);

        private void Choice(Panel page, string text, IList<string> choices, int selected, Action<int> onChanged)
            => ChoiceOption.CreateChoiceOption(page, text, NextRow(), choices, Math.Max(0, selected), onChanged);

        private void Colors(Panel page, string text, List<Color> colors, Action<Color> onSelected)
            => ColorOption.CreateColorOption(page, NextRow(), onSelected, text, colors, true, 0.75f, 26, SliderOption.SliderLeft);

        private void Check(Panel page, string text, bool value, Action<bool> onChanged)
            => CheckboxOption.CreateCheckboxControlOption(page, text, NextRow(), value, onChanged);

        private void Section(Panel page, string title)
        {
            new Label(ContentHandler.Instance.Font, title, new Vector2(0, NextRow() + 4), HeaderColor)
                .SetScale(0.7f)
                .AddToScreen(page);
        }

        private void Note(Panel page, string text)
        {
            var top = NextRow() + 4;
            var note = new MultiLineLabel(ContentHandler.Instance.Font, text, new Vector2(0, top), new Color(170, 170, 190), OptionsWidth - 20)
                .SetScale(0.55f)
                .AddToScreen(page);
            row += (int)Math.Ceiling(note.Lines.Count * note.Font.LineHeight * 0.55f / RowHeight);
        }

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

        // ---- Styles

        private sealed class FillOptions
        {
            public FillOptions Clone() => (FillOptions)MemberwiseClone();

            public bool On;
            public Color[] Colors = { Color.White, Color.White, Color.White };
            public float Middle = 0.5f;
            public float Angle = 90f;
            public float Start;
            public float End = 1f;
            public float Hardness;
            public bool LineBounds;
            public float Scroll;
        }

        private sealed class OutlineOptions
        {
            public OutlineOptions Clone() => (OutlineOptions)MemberwiseClone();

            public bool On;
            public Color Color = Color.Black;
            public float Thickness = 3f;
            public float Softness;
            public IList<Color> Cycle;
        }

        private sealed class BevelOptions
        {
            public BevelOptions Clone() => (BevelOptions)MemberwiseClone();

            public bool On;
            public Color Highlight = Color.White;
            public Color Shade = Color.Black;
            public float Depth = 1.5f;
            public float Softness = 1.25f;
            public float HighlightStrength = 0.8f;
            public float ShadeStrength = 0.5f;
            public float Orbit;
        }

        private sealed class InnerShadowOptions
        {
            public InnerShadowOptions Clone() => (InnerShadowOptions)MemberwiseClone();

            public bool On;
            public Color Color = Color.Black;
            public float OffsetY = 2f;
            public float Blur = 2f;
            public float Strength = 0.6f;
        }

        private sealed class InnerGlowOptions
        {
            public InnerGlowOptions Clone() => (InnerGlowOptions)MemberwiseClone();

            public bool On;
            public Color Color = Color.White;
            public float Radius = 4f;
            public float Intensity = 1f;
            public float PulseSpeed;
        }

        private sealed class ShadowOptions
        {
            public ShadowOptions Clone() => (ShadowOptions)MemberwiseClone();

            public bool On;
            public Color Color = Color.Black;
            public float Opacity = 0.6f;
            public Vector2 Offset = new Vector2(0, 5);
            public float Blur = 4f;
            public float Spread;
            public float Orbit;
            public float Flicker;
        }

        private sealed class GlowOptions
        {
            public GlowOptions Clone() => (GlowOptions)MemberwiseClone();

            public bool On;
            public Color Color = Color.White;
            public float Radius = 12f;
            public float Spread;
            public float Intensity = 1f;
            public bool Front;
            public float OffsetY;
            public float PulseSpeed;
            public float PulseAmount;
            public float Flicker;
            public float Breathe;
            public IList<Color> Cycle;
        }

        private sealed class OverlayOptions
        {
            public OverlayOptions Clone() => (OverlayOptions)MemberwiseClone();

            public bool On;
            public bool Stripes;
            public Color Color = Color.White;
            public float StartStrength = 0.6f;
            public float EndStrength = 0.2f;
            public float Start;
            public float End = 0.48f;
            public float Softness;
            public float Angle = 90f;
            public bool Light = true;
            public float Scroll;

            public GradientOverlay Create()
            {
                GradientOverlay overlay;
                if (Stripes)
                {
                    // One stripe and one gap per period (from Start to End), repeated over the letters.
                    var clear = new Color(Color.R, Color.G, Color.B, (byte)0);
                    var soft = MathHelper.Clamp(Softness, 0f, 0.24f);
                    overlay = new GradientOverlay(Color * StartStrength, Color * EndStrength, clear, clear, Color * StartStrength)
                        .SetPositions(0f, 0.5f - soft, 0.5f + soft, 1f - soft, 1f)
                        .SetRange(Start, End)
                        .SetRepeat();
                }
                else
                {
                    overlay = GradientOverlay.Band(Color, StartStrength, EndStrength, Start, End, Softness).SetRepeat(Scroll != 0f);
                }

                return overlay.SetAngle(Angle)
                    .SetScroll(Scroll)
                    .SetBlend(Light ? ShadingBlend.Light : ShadingBlend.Normal);
            }
        }

        private sealed class PatternOptions
        {
            public PatternOptions Clone() => (PatternOptions)MemberwiseClone();

            public bool On;
            public ShadingPattern Kind;
            public Color Color = Color.Black;
            public float Strength = 0.3f;
            public float TileSize = 8f;
            public bool Light;
            public Vector2 Scroll;
        }

        private sealed class SparkleOptions
        {
            public SparkleOptions Clone() => (SparkleOptions)MemberwiseClone();

            public bool On;
            public Color Color = Color.White;
            public int Count = 10;
            public float Size = 18f;
            public float Twinkle;
            public int Seed = 1;
        }

        private sealed class ShineOptions
        {
            public ShineOptions Clone() => (ShineOptions)MemberwiseClone();

            public bool On;
            public Color Color = Color.White;
            public float Width = 30f;
            public float Angle = 25f;
            public float Duration = 0.9f;
            public float Interval = 3f;
            public float Intensity = 1f;
        }

        /// <summary>A style of the gallery, and the style edited in the preview: a text, a font and every effect.</summary>
        private sealed class TextStyle
        {
            public TextStyle(string caption, string text, int fontIndex)
            {
                Caption = caption;
                Text = text;
                FontIndex = fontIndex;
            }

            public string Caption;

            /// <summary>The section of the gallery (null: the classic still or animated sections).</summary>
            public string Section;
            public string Text;
            public int FontIndex;
            public float Scale = 2f;
            public float PreviewScale = 3f;
            public float TileScale = 1.6f;
            public float Rotation;
            public float Opacity = 1f;
            public float Spacing;
            public Color TextColor = Color.White;
            public Color Stage = new Color(12, 12, 22);
            public float BuiltInOutline;
            public Color BuiltInOutlineColor = Color.Black;
            public FillOptions Fill = new FillOptions();
            public OutlineOptions[] Outlines = { new OutlineOptions(), new OutlineOptions(), new OutlineOptions() };
            public BevelOptions Bevel = new BevelOptions();
            public InnerShadowOptions InnerShadow = new InnerShadowOptions();
            public InnerGlowOptions InnerGlow = new InnerGlowOptions();
            public ShadowOptions[] Shadows = { new ShadowOptions(), new ShadowOptions() };
            public GlowOptions[] Glows = { new GlowOptions(), new GlowOptions() };
            public ShineOptions Shine = new ShineOptions();
            public OverlayOptions[] Overlays = { new OverlayOptions(), new OverlayOptions(), new OverlayOptions() };
            public PatternOptions Pattern = new PatternOptions();
            public SparkleOptions Sparkles = new SparkleOptions();

            /// <summary>True when something of the style moves.</summary>
            public bool IsAnimated
                => (Fill.On && Fill.Scroll != 0f) || Shine.On || (Bevel.On && Bevel.Orbit > 0f) || (InnerGlow.On && InnerGlow.PulseSpeed > 0f)
                    || Glows.Any(glow => glow.On && (glow.PulseAmount > 0f || glow.Flicker > 0f || glow.Breathe > 0f || glow.Cycle != null))
                    || Shadows.Any(shadow => shadow.On && (shadow.Orbit > 0f || shadow.Flicker > 0f))
                    || Outlines.Any(outline => outline.On && outline.Cycle != null)
                    || Overlays.Any(overlay => overlay.On && overlay.Scroll != 0f)
                    || (Pattern.On && Pattern.Scroll != Vector2.Zero) || (Sparkles.On && Sparkles.Twinkle > 0f);

            public TextStyle Clone()
            {
                var copy = (TextStyle)MemberwiseClone();
                copy.Fill = Fill.Clone();
                copy.Fill.Colors = (Color[])Fill.Colors.Clone();
                copy.Outlines = Outlines.Select(item => item.Clone()).ToArray();
                copy.Bevel = Bevel.Clone();
                copy.InnerShadow = InnerShadow.Clone();
                copy.InnerGlow = InnerGlow.Clone();
                copy.Shadows = Shadows.Select(item => item.Clone()).ToArray();
                copy.Glows = Glows.Select(item => item.Clone()).ToArray();
                copy.Shine = Shine.Clone();
                copy.Overlays = Overlays.Select(item => item.Clone()).ToArray();
                copy.Pattern = Pattern.Clone();
                copy.Sparkles = Sparkles.Clone();
                return copy;
            }


            /// <summary>How far the effects reach outside the glyphs, in local units (to fit the text in a tile).</summary>
            public float GetExtent()
            {
                var extent = Fill.On ? 0f : BuiltInOutline;
                foreach (var outline in Outlines.Where(item => item.On))
                    extent = Math.Max(extent, outline.Thickness + outline.Softness);
                foreach (var shadow in Shadows.Where(item => item.On))
                    extent = Math.Max(extent, shadow.Offset.Length() + shadow.Blur + shadow.Spread + shadow.Orbit);
                foreach (var glow in Glows.Where(item => item.On))
                    extent = Math.Max(extent, (glow.Radius + glow.Spread) * 0.6f + Math.Abs(glow.OffsetY));
                return extent;
            }

            /// <summary>Creates the shading effects: shadows, glows, outlines (thickest first), fill, bevel, inner effects, shine.</summary>
            public List<ShadingEffect> CreateEffects()
            {
                var effects = new List<ShadingEffect>();
                foreach (var shadow in Shadows.Where(item => item.On))
                {
                    effects.Add(new Shadow(shadow.Offset, shadow.Blur, shadow.Color * shadow.Opacity)
                        .SetSpread(shadow.Spread)
                        .SetOrbit(shadow.Orbit, shadow.Orbit > 0f ? 0.4f : 0f)
                        .SetFlicker(shadow.Flicker, 9f));
                }

                foreach (var glow in Glows.Where(item => item.On))
                {
                    effects.Add(new Glow(glow.Color, glow.Radius)
                        .SetSpread(glow.Spread)
                        .SetIntensity(glow.Intensity)
                        .SetOffset(0f, glow.OffsetY)
                        .SetLayer(glow.Front ? ShadingLayer.Front : ShadingLayer.Behind)
                        .SetPulse(glow.PulseSpeed, glow.PulseAmount)
                        .SetFlicker(glow.Flicker, 6f)
                        .SetBreathe(glow.Breathe > 0f ? 0.5f : 0f, glow.Breathe)
                        .SetColorCycle(glow.Cycle, 3f));
                }

                foreach (var outline in Outlines.Where(item => item.On).OrderByDescending(item => item.Thickness))
                    effects.Add(new Outline(outline.Color, outline.Thickness).SetSoftness(outline.Softness).SetColorCycle(outline.Cycle, 2.5f));

                if (Fill.On)
                {
                    var colors = Fill.Colors.Length == 3 && Fill.Colors[1] == Fill.Colors[0] && Fill.Colors[1] == Fill.Colors[2]
                        ? new[] { Fill.Colors[0], Fill.Colors[0] }
                        : Fill.Colors;
                    var fill = new GradientFill(colors)
                        .SetAngle(Fill.Angle)
                        .SetRange(Fill.Start, Fill.End)
                        .SetHardness(Fill.Hardness)
                        .SetBounds(Fill.LineBounds ? GradientBounds.Control : GradientBounds.Content)
                        .SetScroll(Fill.Scroll);
                    if (colors.Length == 3)
                        fill.SetPositions(0f, Fill.Middle, 1f);
                    effects.Add(fill);
                }

                if (Pattern.On)
                {
                    effects.Add(new PatternOverlay(Pattern.Kind, Pattern.Color * Pattern.Strength, Pattern.TileSize)
                        .SetBlend(Pattern.Light ? ShadingBlend.Light : ShadingBlend.Normal)
                        .SetScroll(Pattern.Scroll.X, Pattern.Scroll.Y));
                }

                foreach (var overlay in Overlays.Where(item => item.On))
                    effects.Add(overlay.Create());

                if (Bevel.On)
                {
                    effects.Add(new InnerShadow(new Vector2(0f, Bevel.Depth), Bevel.Softness, Bevel.Highlight * Bevel.HighlightStrength)
                        .SetBlend(ShadingBlend.Light)
                        .SetOrbit(Bevel.Orbit, Bevel.Orbit > 0f ? 0.35f : 0f));
                    effects.Add(new InnerShadow(new Vector2(0f, -Bevel.Depth), Bevel.Softness, Bevel.Shade * Bevel.ShadeStrength)
                        .SetOrbit(Bevel.Orbit, Bevel.Orbit > 0f ? 0.35f : 0f));
                }

                if (InnerShadow.On)
                    effects.Add(new InnerShadow(new Vector2(0f, InnerShadow.OffsetY), InnerShadow.Blur, InnerShadow.Color * InnerShadow.Strength));

                if (InnerGlow.On)
                {
                    effects.Add(new InnerGlow(InnerGlow.Color, InnerGlow.Radius)
                        .SetIntensity(InnerGlow.Intensity)
                        .SetPulse(InnerGlow.PulseSpeed, InnerGlow.PulseSpeed > 0f ? 0.5f : 0f));
                }

                if (Sparkles.On)
                {
                    effects.Add(new Sparkles(Sparkles.Color, Sparkles.Count, Sparkles.Size)
                        .SetSeed(Sparkles.Seed)
                        .SetTwinkle(Sparkles.Twinkle));
                }

                if (Shine.On)
                {
                    effects.Add(new Shine(Shine.Color)
                        .SetWidth(Shine.Width)
                        .SetAngle(Shine.Angle)
                        .SetTiming(Shine.Duration, Shine.Interval)
                        .SetIntensity(Shine.Intensity));
                }

                return effects;
            }

            // ---- Fluent builders of the gallery

            public TextStyle In(string section)
            {
                Section = section;
                return this;
            }

            /// <summary>A gradient of any number of colors, evenly spread (with hardness 1: stripes).</summary>
            public TextStyle WithStops(float angle, float hardness, params Color[] colors)
            {
                Fill.On = true;
                Fill.Colors = colors;
                Fill.Angle = angle;
                Fill.Hardness = hardness;
                return this;
            }

            public TextStyle Faded(float opacity)
            {
                Opacity = opacity;
                return this;
            }

            public TextStyle OnStage(Color stage)
            {
                Stage = stage;
                return this;
            }

            public TextStyle Colored(Color color)
            {
                TextColor = color;
                return this;
            }

            public TextStyle Spaced(float spacing)
            {
                Spacing = spacing;
                return this;
            }

            public TextStyle Rotated(float degrees)
            {
                Rotation = degrees;
                return this;
            }

            public TextStyle WithFill(Color top, Color middle, Color bottom, float middleAt = 0.5f, float hardness = 0f, float angle = 90f)
            {
                Fill.On = true;
                Fill.Colors = new[] { top, middle, bottom };
                Fill.Middle = middleAt;
                Fill.Hardness = hardness;
                Fill.Angle = angle;
                return this;
            }

            public TextStyle WithFill(Color top, Color bottom, float hardness = 0f, float angle = 90f)
                => WithFill(top, Color.Lerp(top, bottom, 0.5f), bottom, 0.5f, hardness, angle);

            public TextStyle Scrolling(float speed, float end = 1f)
            {
                Fill.Scroll = speed;
                Fill.End = end;
                return this;
            }

            public TextStyle WithOutline(Color color, float thickness, float softness = 0f, IList<Color> cycle = null)
            {
                var outline = Outlines.First(item => !item.On);
                outline.On = true;
                outline.Color = color;
                outline.Thickness = thickness;
                outline.Softness = softness;
                outline.Cycle = cycle;
                return this;
            }

            public TextStyle WithBuiltInOutline(Color color, float thickness)
            {
                BuiltInOutline = thickness;
                BuiltInOutlineColor = color;
                return this;
            }

            public TextStyle WithBevel(Color highlight, Color shade, float depth = 1.5f, float softness = 1.25f, float highlightStrength = 0.8f, float shadeStrength = 0.5f, float orbit = 0f)
            {
                Bevel.On = true;
                Bevel.Highlight = highlight;
                Bevel.Shade = shade;
                Bevel.Depth = depth;
                Bevel.Softness = softness;
                Bevel.HighlightStrength = highlightStrength;
                Bevel.ShadeStrength = shadeStrength;
                Bevel.Orbit = orbit;
                return this;
            }

            public TextStyle WithInnerShadow(Color color, float offsetY, float blur, float strength = 0.6f)
            {
                InnerShadow.On = true;
                InnerShadow.Color = color;
                InnerShadow.OffsetY = offsetY;
                InnerShadow.Blur = blur;
                InnerShadow.Strength = strength;
                return this;
            }

            public TextStyle WithInnerGlow(Color color, float radius, float intensity = 1f, float pulseSpeed = 0f)
            {
                InnerGlow.On = true;
                InnerGlow.Color = color;
                InnerGlow.Radius = radius;
                InnerGlow.Intensity = intensity;
                InnerGlow.PulseSpeed = pulseSpeed;
                return this;
            }

            public TextStyle WithShadow(Color color, float opacity, float x, float y, float blur, float spread = 0f, float orbit = 0f, float flicker = 0f)
            {
                var shadow = Shadows.First(item => !item.On);
                shadow.On = true;
                shadow.Color = color;
                shadow.Opacity = opacity;
                shadow.Offset = new Vector2(x, y);
                shadow.Blur = blur;
                shadow.Spread = spread;
                shadow.Orbit = orbit;
                shadow.Flicker = flicker;
                return this;
            }

            public TextStyle WithGlow(Color color, float radius, float intensity = 1f, float spread = 0f, bool front = false, float offsetY = 0f)
            {
                var glow = Glows.First(item => !item.On);
                glow.On = true;
                glow.Color = color;
                glow.Radius = radius;
                glow.Intensity = intensity;
                glow.Spread = spread;
                glow.Front = front;
                glow.OffsetY = offsetY;
                return this;
            }

            /// <summary>Animates the last glow added.</summary>
            public TextStyle Animated(float pulseSpeed = 0f, float pulseAmount = 0f, float flicker = 0f, float breathe = 0f, IList<Color> cycle = null)
            {
                var glow = Glows.Last(item => item.On);
                glow.PulseSpeed = pulseSpeed;
                glow.PulseAmount = pulseAmount;
                glow.Flicker = flicker;
                glow.Breathe = breathe;
                glow.Cycle = cycle;
                return this;
            }

            /// <summary>A glossy band of light over the top of the letters (the next free overlay).</summary>
            public TextStyle WithGloss(float strength = 0.6f, float bottom = 0.48f, Color? color = null, float softness = 0f)
                => WithBand(color ?? Color.White, strength, strength / 3f, 0f, bottom, softness);

            /// <summary>A band of a color over the letters (the next free overlay).</summary>
            public TextStyle WithBand(Color color, float startStrength, float endStrength, float start, float end, float softness = 0f,
                bool light = true, float angle = 90f, float scroll = 0f)
            {
                var overlay = Overlays.First(item => !item.On);
                overlay.On = true;
                overlay.Stripes = false;
                overlay.Color = color;
                overlay.StartStrength = startStrength;
                overlay.EndStrength = endStrength;
                overlay.Start = start;
                overlay.End = end;
                overlay.Softness = softness;
                overlay.Light = light;
                overlay.Angle = angle;
                overlay.Scroll = scroll;
                return this;
            }

            /// <summary>Repeated stripes over the letters (the next free overlay); the period is a fraction of the letters.</summary>
            public TextStyle WithStripes(Color color, float strength, float angle, float period, float scroll = 0f, bool light = true, float softness = 0.02f)
            {
                var overlay = Overlays.First(item => !item.On);
                overlay.On = true;
                overlay.Stripes = true;
                overlay.Color = color;
                overlay.StartStrength = strength;
                overlay.EndStrength = strength;
                overlay.Start = 0f;
                overlay.End = period;
                overlay.Softness = softness;
                overlay.Light = light;
                overlay.Angle = angle;
                overlay.Scroll = scroll;
                return this;
            }

            public TextStyle WithPattern(ShadingPattern kind, Color color, float strength, float tileSize, bool light = false, float scrollX = 0f, float scrollY = 0f)
            {
                Pattern.On = true;
                Pattern.Kind = kind;
                Pattern.Color = color;
                Pattern.Strength = strength;
                Pattern.TileSize = tileSize;
                Pattern.Light = light;
                Pattern.Scroll = new Vector2(scrollX, scrollY);
                return this;
            }

            public TextStyle WithSparkles(Color color, int count, float size, float twinkle = 0f, int seed = 1)
            {
                Sparkles.On = true;
                Sparkles.Color = color;
                Sparkles.Count = count;
                Sparkles.Size = size;
                Sparkles.Twinkle = twinkle;
                Sparkles.Seed = seed;
                return this;
            }

            public TextStyle WithShine(Color color, float width = 30f, float angle = 25f, float duration = 0.9f, float interval = 3f)
            {
                Shine.On = true;
                Shine.Color = color;
                Shine.Width = width;
                Shine.Angle = angle;
                Shine.Duration = duration;
                Shine.Interval = interval;
                return this;
            }
        }

        // ---- Gallery styles

        private const int Roboto = 0;
        private const int AlfaSlab = 1;
        private const int Lilita = 2;
        private const int Bangers = 3;
        private const int PressStart = 4;
        private const int Audiowide = 5;
        private const int Creepster = 6;
        private const int Pacifico = 7;
        private const int Luckiest = 8;
        private const int Rye = 9;
        private const int BlackOps = 10;
        private const int Monoton = 11;
        private const int Marker = 12;
        private const int Cinzel = 13;
        private const int Bungee = 14;
        private const int Typewriter = 15;
        private const int Kaushan = 16;
        private const int Fraktur = 17;

        private static Color C(int r, int g, int b) => new Color(r, g, b);

        private static TextStyle S(string caption, string text, int font) => new TextStyle(caption, text, font);

        private static readonly Color Night = C(12, 12, 22);
        private static readonly Color Sky = C(55, 105, 150);
        private static readonly Color Brown = C(75, 32, 6);
        private static readonly IList<Color> Rainbow = ShadingPresets.RainbowColors;

        private static readonly TextStyle[] Gallery =
        {
            // The reference: bold serif letters, a cream to gold to orange gradient, a light bevel, a thick dark brown
            // outline, a dark shadow below and a warm glow.
            S("Gold Map", "Map", AlfaSlab).OnStage(Sky)
                .WithShadow(Color.Black, 0.55f, 0, 5, 3, 3.5f)
                .WithGlow(C(255, 170, 40), 14, 0.8f)
                .WithOutline(Brown, 3.5f)
                .WithFill(C(255, 252, 220), C(255, 222, 100), C(242, 150, 22), 0.45f)
                .WithBevel(C(255, 255, 240), C(150, 60, 0), 1.5f, 1.2f, 0.85f, 0.55f),
            S("Silver", "SILVER", AlfaSlab).OnStage(C(30, 34, 48))
                .WithShadow(Color.Black, 0.6f, 0, 4, 4, 2.5f)
                .WithOutline(C(30, 35, 45), 2.5f)
                .WithFill(C(250, 252, 255), C(150, 160, 175), C(215, 222, 235), 0.5f, 0.85f)
                .WithBevel(Color.White, Color.Black, 1.2f, 1f, 0.9f, 0.4f),
            S("Bronze", "BRONZE", AlfaSlab).OnStage(C(40, 30, 25))
                .WithShadow(Color.Black, 0.6f, 0, 4, 3, 2.5f)
                .WithOutline(C(50, 25, 5), 3)
                .WithFill(C(255, 210, 160), C(205, 127, 50), C(120, 60, 20))
                .WithBevel(C(255, 235, 200), Color.Black, 1.5f, 1f, 0.7f, 0.45f),
            S("Ruby", "RUBY", Lilita).OnStage(C(30, 10, 20))
                .WithGlow(C(255, 40, 80), 12, 0.7f)
                .WithOutline(C(60, 0, 15), 3)
                .WithFill(C(255, 170, 190), C(225, 20, 60), C(110, 0, 30), 0.5f, 0.3f)
                .WithBevel(Color.White, Color.Black, 1.5f, 1f, 0.85f, 0.45f),
            S("Emerald", "EMERALD", Lilita).OnStage(C(10, 30, 25))
                .WithShadow(Color.Black, 0.5f, 0, 4, 4, 3)
                .WithGlow(C(40, 255, 140), 12, 0.6f)
                .WithOutline(C(5, 50, 25), 3)
                .WithFill(C(190, 255, 210), C(40, 200, 110), C(0, 110, 60), 0.5f, 0.35f)
                .WithBevel(Color.White, Color.Black, 1.5f, 1f, 0.75f, 0.45f),
            S("Sapphire", "SAPPHIRE", AlfaSlab).OnStage(C(15, 20, 45))
                .WithGlow(C(60, 120, 255), 12, 0.7f)
                .WithOutline(C(5, 15, 60), 3)
                .WithFill(C(200, 220, 255), C(50, 100, 240), C(15, 30, 140), 0.5f, 0.3f)
                .WithBevel(Color.White, Color.Black, 1.5f, 1f, 0.8f, 0.45f),
            S("Ice", "FROZEN", Audiowide).OnStage(C(20, 40, 70))
                .WithGlow(C(60, 160, 255), 16, 0.9f)
                .WithOutline(C(20, 60, 130), 3)
                .WithOutline(C(210, 245, 255), 1.5f)
                .WithFill(Color.White, C(150, 230, 255), C(40, 130, 230))
                .WithInnerGlow(C(200, 245, 255), 3, 0.8f),
            S("Fire", "FIRE", Bangers).OnStage(C(25, 10, 5)).Spaced(2)
                .WithGlow(C(255, 110, 20), 16, 1.1f, 0, false, -3)
                .WithOutline(C(90, 10, 0), 3)
                .WithFill(C(255, 250, 160), C(255, 140, 20), C(200, 20, 0), 0.45f),
            S("Poison", "POISON", Creepster).OnStage(C(15, 25, 15)).Spaced(2)
                .WithGlow(C(120, 255, 60), 14, 0.9f)
                .WithOutline(C(10, 40, 0), 3)
                .WithFill(C(220, 255, 120), C(90, 200, 40), C(20, 90, 20))
                .WithInnerShadow(C(0, 40, 0), -2, 1.5f, 0.6f),
            S("Purple magic", "Magic", Pacifico).OnStage(C(25, 10, 40))
                .WithGlow(C(190, 90, 255), 18, 1f)
                .WithOutline(C(40, 0, 70), 3)
                .WithFill(C(255, 200, 255), C(190, 90, 255), C(90, 20, 170))
                .WithInnerGlow(C(255, 220, 255), 2.5f, 0.6f),
            S("Candy", "SWEET", Lilita).OnStage(C(255, 200, 225))
                .WithShadow(Color.Black, 0.3f, 0, 7, 5, 4)
                .WithShadow(C(150, 20, 90), 1f, 0, 4, 0, 4)
                .WithOutline(Color.White, 4)
                .WithFill(C(255, 200, 235), C(255, 90, 180), C(220, 30, 140))
                .WithBevel(Color.White, C(120, 0, 60), 2f, 1.5f, 0.9f, 0.4f),
            S("Cartoon", "BOOM!", Bangers).OnStage(C(60, 170, 230)).Spaced(1).Rotated(-6)
                .WithShadow(Color.Black, 1f, 4, 5, 0, 5)
                .WithOutline(Color.Black, 5)
                .WithFill(C(255, 245, 120), C(255, 170, 0), 0.6f),
            S("Comic pop", "POW!", Bangers).OnStage(C(255, 220, 40)).Spaced(2)
                .WithShadow(C(255, 40, 80), 1f, 5, 5, 0, 3)
                .WithOutline(Color.Black, 3)
                .Colored(Color.White),
            S("Steel", "STEEL", Audiowide).OnStage(C(45, 50, 60))
                .WithShadow(Color.Black, 0.6f, 0, 4, 3, 2)
                .WithOutline(C(20, 20, 30), 2)
                .WithFill(C(235, 240, 245), C(120, 130, 140), 0.8f)
                .WithBevel(Color.White, Color.Black, 1f, 0.75f, 0.8f, 0.4f),
            S("Chocolate", "CHOCO", Lilita).OnStage(C(250, 230, 200))
                .WithShadow(C(60, 30, 10), 0.5f, 0, 5, 3, 3)
                .WithOutline(C(40, 20, 5), 3)
                .WithFill(C(190, 120, 70), C(110, 60, 30), C(70, 35, 15))
                .WithBevel(C(255, 220, 180), Color.Black, 2f, 1.5f, 0.6f, 0.4f),
            S("Wood", "WOOD", AlfaSlab).OnStage(C(60, 110, 60))
                .WithShadow(Color.Black, 0.55f, 0, 5, 3, 3)
                .WithOutline(C(60, 30, 10), 3)
                .WithFill(C(215, 155, 90), C(150, 90, 45), C(200, 140, 80), 0.5f, 0.6f, 75f)
                .WithBevel(C(255, 225, 180), C(60, 30, 10), 1.5f, 1f, 0.6f, 0.5f),
            S("Stone", "STONE", AlfaSlab).OnStage(C(90, 120, 90))
                .WithShadow(Color.Black, 0.6f, 0, 5, 2, 3)
                .WithOutline(C(30, 30, 30), 3)
                .WithFill(C(200, 200, 195), C(130, 130, 125), C(90, 90, 88))
                .WithBevel(Color.White, Color.Black, 2f, 1f, 0.5f, 0.6f),
            S("Horror", "HORROR", Creepster).OnStage(C(10, 5, 5)).Spaced(2)
                .WithShadow(Color.Black, 0.9f, 0, 4, 6, 2)
                .WithGlow(C(160, 0, 0), 12, 0.8f)
                .WithOutline(Color.Black, 2)
                .WithFill(C(255, 60, 60), C(170, 0, 0), C(70, 0, 0)),
            S("Retro 8-bit", "GAME OVER", PressStart).OnStage(C(20, 10, 40))
                .WithShadow(C(200, 0, 80), 1f, 4, 4, 0)
                .WithOutline(Color.Black, 2)
                .WithFill(C(255, 240, 0), C(255, 120, 0), 1f),
            S("Sci-fi", "SCI-FI", Audiowide).OnStage(C(5, 15, 30)).Spaced(3)
                .WithGlow(C(0, 200, 255), 12, 0.9f)
                .WithOutline(C(0, 30, 60), 2)
                .WithFill(C(220, 255, 255), C(60, 200, 255), C(0, 80, 200), 0.5f, 0.6f)
                .WithInnerGlow(Color.White, 2, 0.5f),
            S("Rainbow", "RAINBOW", Lilita).OnStage(C(30, 30, 50))
                .WithOutline(C(30, 20, 60), 5)
                .WithOutline(Color.White, 2.5f)
                .WithFill(C(255, 80, 80), C(250, 230, 70), C(60, 170, 255), 0.5f, 0f, 0f)
                .WithBevel(Color.White, Color.Black, 1.5f, 1f, 0.6f, 0.3f),
            S("Sunset", "Sunset", Pacifico).OnStage(C(40, 20, 50))
                .WithShadow(C(40, 0, 50), 0.6f, 0, 5, 6, 2)
                .WithOutline(C(50, 10, 60), 3)
                .WithFill(C(255, 240, 120), C(255, 120, 60), C(150, 40, 150)),
            S("Ocean", "OCEAN", Lilita).OnStage(C(230, 240, 250))
                .WithShadow(C(0, 30, 70), 0.5f, 0, 5, 3, 3)
                .WithOutline(C(0, 30, 70), 3)
                .WithFill(C(180, 250, 255), C(40, 170, 230), C(10, 60, 140))
                .WithBevel(Color.White, Color.Black, 1.5f, 1f, 0.8f, 0.35f),
            S("Two-tone", "SPLIT", Bangers).OnStage(C(230, 60, 60)).Spaced(2)
                .WithShadow(Color.Black, 0.8f, 3, 4, 0, 3)
                .WithOutline(Color.Black, 3)
                .WithFill(Color.White, C(255, 200, 0), 1f),
            S("Embossed", "EMBOSS", AlfaSlab).OnStage(C(150, 150, 160)).Colored(C(150, 150, 160))
                .WithShadow(Color.White, 0.5f, 0, 2, 1)
                .WithBevel(Color.White, Color.Black, 2f, 1.5f, 0.7f, 0.6f),
            S("Pastel", "Hello!", Pacifico).OnStage(C(120, 100, 170))
                .WithShadow(C(255, 120, 180), 0.4f, 0, 4, 6, 2)
                .WithOutline(Color.White, 3)
                .WithFill(C(255, 200, 230), C(180, 200, 255)),
            S("Victory", "VICTORY", AlfaSlab).OnStage(C(30, 20, 50))
                .WithShadow(Color.Black, 0.6f, 0, 5, 4, 3)
                .WithGlow(C(255, 200, 60), 18, 0.9f)
                .WithOutline(C(80, 35, 0), 3.5f)
                .WithOutline(C(255, 240, 180), 1.5f)
                .WithFill(C(255, 255, 230), C(255, 210, 60), C(230, 120, 10), 0.5f)
                .WithBevel(Color.White, C(120, 50, 0), 1.5f, 1f, 0.85f, 0.5f),
            S("Defeat", "DEFEAT", AlfaSlab).OnStage(C(60, 15, 15))
                .WithShadow(Color.Black, 0.8f, 0, 5, 6, 2)
                .WithOutline(C(20, 20, 30), 3)
                .WithFill(C(180, 190, 200), C(90, 95, 110))
                .WithInnerShadow(Color.Black, 2.5f, 2f, 0.6f),
            S("Level up", "LEVEL UP", Lilita).OnStage(C(20, 40, 80))
                .WithShadow(C(10, 70, 20), 1f, 0, 4, 0, 3.5f)
                .WithGlow(C(120, 255, 120), 12, 0.6f)
                .WithOutline(C(10, 70, 20), 3.5f)
                .WithFill(C(220, 255, 140), C(60, 210, 70))
                .WithBevel(Color.White, Color.Black, 1.5f, 1f, 0.8f, 0.35f),
            S("+100 coins", "+100", Lilita).OnStage(C(40, 120, 200))
                .WithShadow(Color.Black, 0.5f, 0, 4, 2, 3)
                .WithOutline(C(120, 60, 0), 3)
                .WithFill(C(255, 250, 180), C(255, 190, 0))
                .WithBevel(Color.White, C(120, 60, 0), 1.5f, 1f, 0.8f, 0.4f),
            S("Combo", "COMBO x3", Bangers).OnStage(C(25, 15, 40)).Spaced(1).Rotated(-6)
                .WithShadow(Color.Black, 0.9f, 3, 4, 0, 3.5f)
                .WithOutline(C(60, 0, 60), 3.5f)
                .WithFill(C(255, 230, 90), C(255, 90, 200), 0f, 0f),
            S("Damage", "-250", Bangers).OnStage(C(50, 60, 70)).Spaced(1)
                .WithShadow(Color.Black, 0.7f, 2, 3, 0, 3)
                .WithOutline(C(60, 0, 0), 3)
                .WithFill(C(255, 160, 120), C(220, 20, 20)),
            S("Critical hit", "999!", AlfaSlab).OnStage(C(40, 20, 20))
                .WithGlow(C(255, 140, 0), 14, 1f)
                .WithOutline(C(140, 40, 0), 3)
                .WithFill(Color.White, C(255, 230, 60), C(255, 140, 0)),
            S("Heal", "+75", Lilita).OnStage(C(25, 45, 35))
                .WithGlow(C(80, 255, 140), 10, 0.7f)
                .WithOutline(C(0, 60, 30), 3)
                .WithFill(C(210, 255, 220), C(40, 210, 110)),
            S("Menu button", "Play", Lilita).OnStage(C(255, 190, 60))
                .WithShadow(C(10, 40, 100), 1f, 0, 4, 0, 4)
                .WithOutline(C(20, 60, 140), 4)
                .WithFill(Color.White, C(200, 230, 255))
                .WithBevel(Color.White, C(20, 60, 140), 1.5f, 1f, 0.6f, 0.3f),
            S("Plain Roboto", "Settings", Roboto).OnStage(C(40, 60, 90))
                .WithBuiltInOutline(C(20, 25, 40), 2)
                .WithShadow(Color.Black, 0.5f, 0, 3, 4),
            S("New record", "NEW RECORD", Audiowide).OnStage(C(20, 10, 30))
                .WithGlow(C(255, 180, 40), 12, 0.8f)
                .WithOutline(C(60, 20, 0), 2.5f)
                .WithFill(C(255, 255, 200), C(255, 180, 40), 0.5f),
            S("Paused", "Paused", Pacifico).OnStage(C(30, 30, 40)).Colored(Color.White)
                .WithShadow(Color.Black, 0.6f, 0, 4, 6)
                .WithGlow(Color.White, 10, 0.4f),
            S("Sticker", "HELLO", Lilita).OnStage(C(120, 200, 140)).Colored(C(255, 220, 40))
                .WithShadow(Color.Black, 0.4f, 0, 5, 5, 6)
                .WithOutline(Color.Black, 4)
                .WithOutline(Color.White, 7),
            S("Boss fight", "BOSS", Creepster).OnStage(C(20, 10, 30)).Spaced(3)
                .WithShadow(Color.Black, 0.8f, 0, 4, 4, 2)
                .WithGlow(C(170, 0, 255), 14, 0.8f)
                .WithOutline(Color.Black, 2.5f)
                .WithFill(C(255, 120, 120), C(140, 0, 60))
                .WithInnerShadow(Color.Black, -2.5f, 2f, 0.5f),

            // ---- Animated
            S("Neon sign", "OPEN", Audiowide).OnStage(C(10, 8, 20)).Colored(C(255, 220, 250)).Spaced(3)
                .WithGlow(ShadingPresets.NeonPink, 30, 0.9f).Animated(flicker: 0.35f, breathe: 0.03f)
                .WithGlow(ShadingPresets.NeonPink, 10, 1.3f).Animated(flicker: 0.3f),
            S("Blaze", "BLAZE", Bangers).OnStage(C(20, 8, 4)).Spaced(2)
                .WithGlow(C(255, 60, 10), 24, 1.5f, 0, false, -5).Animated(flicker: 0.5f, breathe: 0.05f)
                .WithOutline(C(80, 10, 0), 3)
                .WithFill(C(255, 240, 120), C(255, 120, 0), C(255, 240, 120), 0.5f, 0f, -90f).Scrolling(0.4f),
            S("Lava", "LAVA", AlfaSlab).OnStage(C(25, 10, 8))
                .WithGlow(C(255, 60, 0), 18, 1.2f).Animated(flicker: 0.35f)
                .WithOutline(C(50, 8, 0), 3)
                .WithFill(C(255, 230, 90), C(170, 20, 0), C(255, 230, 90), 0.5f, 0f, -90f).Scrolling(0.2f)
                .WithInnerGlow(C(255, 210, 80), 3, 0.9f, 0.8f),
            S("Holographic", "HOLO", Audiowide).OnStage(C(20, 20, 35)).Spaced(2)
                .WithShadow(Color.Black, 0.5f, 0, 4, 5, 2)
                .WithOutline(C(20, 20, 40), 2.5f)
                .WithFill(C(255, 120, 200), C(120, 220, 255), C(255, 120, 200), 0.5f, 0f, 30f).Scrolling(0.3f)
                .WithShine(Color.White),
            S("Party", "PARTY", Lilita).OnStage(C(25, 25, 40))
                .WithGlow(Color.White, 14, 0.8f).Animated(cycle: Rainbow)
                .WithOutline(C(30, 20, 60), 4)
                .WithFill(C(255, 80, 80), C(80, 160, 255), C(255, 80, 80), 0.5f, 0f, 0f).Scrolling(0.25f),
            S("Shiny gold", "GOLD", AlfaSlab).OnStage(C(40, 25, 60))
                .WithShadow(Color.Black, 0.55f, 0, 5, 3, 3)
                .WithGlow(C(255, 170, 40), 16, 0.9f).Animated(0.6f, 0.35f)
                .WithOutline(Brown, 3.5f)
                .WithFill(C(255, 252, 220), C(255, 222, 100), C(242, 150, 22), 0.45f)
                .WithBevel(C(255, 255, 240), C(150, 60, 0), 1.5f, 1.2f, 0.85f, 0.55f)
                .WithShine(C(255, 250, 220), 34),
            S("Love", "Love", Pacifico).OnStage(C(40, 10, 25))
                .WithGlow(C(255, 40, 100), 16, 1.2f).Animated(1.2f, 0.85f, breathe: 0.05f)
                .WithOutline(C(80, 0, 30), 3)
                .WithFill(C(255, 190, 210), C(240, 40, 100)),
            S("Chill", "CHILL", Audiowide).OnStage(C(15, 30, 55)).Spaced(2)
                .WithGlow(C(40, 140, 255), 22, 1f).Animated(breathe: 0.05f)
                .WithOutline(C(20, 60, 130), 2.5f)
                .WithFill(Color.White, C(150, 230, 255), C(40, 130, 230))
                .WithShine(C(220, 245, 255), 24, 35, 1.2f, 4f),
            S("Toxic", "TOXIC", Creepster).OnStage(C(10, 20, 10)).Spaced(2)
                .WithGlow(C(120, 255, 40), 18, 1.2f).Animated(0.8f, 0.5f, breathe: 0.04f)
                .WithOutline(C(10, 40, 0), 2.5f)
                .WithFill(C(220, 255, 120), C(40, 160, 20))
                .WithInnerGlow(C(220, 255, 140), 3, 0.9f, 1.2f),
            S("Spell", "Spell", Pacifico).OnStage(C(20, 10, 35))
                .WithGlow(Color.White, 20, 1.1f).Animated(breathe: 0.04f, cycle: ShadingPresets.MagicColors)
                .WithOutline(C(40, 0, 70), 3)
                .WithFill(C(255, 220, 255), C(150, 80, 255)),
            S("Glitch", "ERROR", PressStart).OnStage(C(10, 10, 15)).Colored(Color.White)
                .WithShadow(C(255, 40, 60), 0.9f, -3, 0, 0, 0, 2, 0.5f)
                .WithShadow(C(40, 220, 255), 0.9f, 3, 0, 0, 0, 2, 0.5f),
            S("Start", "START", Lilita).OnStage(C(30, 60, 120))
                .WithGlow(C(255, 220, 80), 18, 1f).Animated(1.2f, 0.4f, breathe: 0.03f)
                .WithOutline(Color.White, 3)
                .WithFill(C(255, 240, 150), C(255, 160, 0)),
            S("Moving light", "SHINE", AlfaSlab).OnStage(C(30, 30, 45))
                .WithShadow(Color.Black, 0.6f, 0, 5, 4, 3)
                .WithOutline(C(20, 25, 40), 3)
                .WithFill(C(240, 245, 255), C(140, 150, 170), C(210, 220, 235), 0.5f, 0.7f)
                .WithBevel(Color.White, Color.Black, 2f, 1.5f, 1f, 0.6f, 2f),
            S("Ghost", "BOO!", Creepster).OnStage(C(15, 20, 35)).Spaced(3)
                .WithGlow(C(120, 170, 255), 24, 0.9f).Animated(0.4f, 0.3f, breathe: 0.06f)
                .WithFill(Color.White, C(170, 200, 255))
                .WithInnerGlow(C(150, 190, 255), 4, 0.8f),
            S("Victory parade", "VICTORY!", Lilita).OnStage(C(60, 20, 90)).Rotated(-4)
                .WithShadow(Color.Black, 0.5f, 0, 5, 4, 3)
                .WithGlow(C(255, 220, 60), 16, 1f).Animated(1f, 0.4f)
                .WithOutline(C(90, 30, 0), 3.5f, 0f, ShadingPresets.FireColors)
                .WithFill(C(255, 255, 200), C(255, 200, 0), C(255, 120, 0))
                .WithShine(Color.White, 26),
            // ---- Fantasy & RPG
            S("Dragon", "DRAGON", Cinzel).In("Fantasy & RPG").OnStage(C(30, 10, 10)).Spaced(2)
                .WithShadow(Color.Black, 0.7f, 0, 5, 4, 3)
                .WithGlow(C(255, 90, 20), 16, 0.9f)
                .WithOutline(C(40, 5, 0), 4.5f)
                .WithOutline(C(255, 200, 80), 1.5f)
                .WithFill(C(255, 120, 80), C(200, 20, 10), C(90, 0, 0))
                .WithBevel(C(255, 220, 180), Color.Black, 1.5f, 1f, 0.7f, 0.5f),
            S("Elven", "Elven", Kaushan).In("Fantasy & RPG").OnStage(C(15, 40, 35))
                .WithGlow(C(120, 255, 200), 16, 0.7f)
                .WithOutline(C(0, 50, 45), 3)
                .WithFill(C(220, 255, 240), C(60, 200, 170))
                .WithInnerGlow(Color.White, 2.5f, 0.6f),
            S("Rune stone", "RUNES", Cinzel).In("Fantasy & RPG").OnStage(C(25, 30, 35)).Spaced(3)
                .WithGlow(C(60, 220, 255), 18, 1f).Animated(0.7f, 0.6f)
                .WithOutline(C(20, 25, 30), 3)
                .WithFill(C(170, 175, 180), C(100, 105, 110), C(70, 72, 78))
                .WithInnerGlow(C(120, 240, 255), 3, 0.9f, 0.7f),
            S("Legendary", "LEGENDARY", Cinzel).In("Fantasy & RPG").OnStage(C(35, 20, 10)).Spaced(1)
                .WithShadow(Color.Black, 0.6f, 0, 4, 3, 2.5f)
                .WithGlow(C(255, 140, 0), 18, 1.1f).Animated(0.8f, 0.4f)
                .WithOutline(C(70, 25, 0), 3)
                .WithFill(C(255, 240, 160), C(255, 160, 20), C(200, 80, 0), 0.5f, 0.4f)
                .WithShine(C(255, 250, 220), 30),
            S("Epic loot", "EPIC", Luckiest).In("Fantasy & RPG").OnStage(C(25, 10, 40))
                .WithGlow(C(180, 80, 255), 16, 1f)
                .WithOutline(C(40, 0, 70), 6)
                .WithOutline(Color.White, 2.5f)
                .WithFill(C(230, 180, 255), C(160, 60, 240), C(90, 20, 170)),
            S("Rare loot", "RARE", Luckiest).In("Fantasy & RPG").OnStage(C(10, 20, 45))
                .WithGlow(C(60, 140, 255), 14, 0.9f)
                .WithOutline(C(5, 25, 70), 6)
                .WithOutline(Color.White, 2.5f)
                .WithFill(C(180, 220, 255), C(60, 140, 255), C(20, 60, 180)),
            S("Common loot", "COMMON", Luckiest).In("Fantasy & RPG").OnStage(C(45, 45, 50))
                .WithShadow(Color.Black, 0.5f, 0, 4, 2, 2)
                .WithOutline(C(30, 30, 35), 4)
                .WithFill(C(240, 240, 240), C(160, 160, 165)),
            S("Quest complete", "QUEST COMPLETE", Cinzel).In("Fantasy & RPG").OnStage(C(60, 40, 25)).Spaced(2)
                .WithShadow(Color.Black, 0.6f, 0, 4, 3, 2.5f)
                .WithOutline(C(60, 30, 5), 3)
                .WithFill(C(255, 245, 200), C(240, 190, 80), C(190, 120, 30))
                .WithBevel(Color.White, C(90, 40, 0), 1.2f, 1f, 0.7f, 0.4f),
            S("Mana", "MANA", Bungee).In("Fantasy & RPG").OnStage(C(10, 15, 40))
                .WithGlow(C(40, 120, 255), 18, 1f).Animated(breathe: 0.04f)
                .WithOutline(C(0, 20, 70), 3)
                .WithFill(C(120, 220, 255), C(30, 80, 230), C(120, 220, 255), 0.5f, 0f, -90f).Scrolling(0.2f)
                .WithInnerGlow(C(170, 240, 255), 3, 0.8f),
            S("Health", "HP 100", Bungee).In("Fantasy & RPG").OnStage(C(40, 15, 15))
                .WithShadow(Color.Black, 0.6f, 0, 4, 2, 2.5f)
                .WithOutline(C(70, 0, 0), 3)
                .WithFill(C(255, 140, 130), C(230, 30, 30), C(140, 0, 0))
                .WithBevel(Color.White, Color.Black, 1.5f, 1f, 0.7f, 0.4f),
            S("Kingdom", "Kingdom", Fraktur).In("Fantasy & RPG").OnStage(C(40, 25, 60))
                .WithShadow(Color.Black, 0.7f, 0, 4, 4, 2)
                .WithOutline(Color.Black, 3)
                .WithFill(C(255, 245, 190), C(255, 200, 60), C(190, 110, 10)),
            S("Grimoire", "Grimoire", Fraktur).In("Fantasy & RPG").OnStage(C(20, 8, 30))
                .WithGlow(C(220, 60, 255), 18, 1f).Animated(breathe: 0.05f, cycle: ShadingPresets.MagicColors)
                .WithOutline(C(30, 0, 50), 2.5f)
                .WithFill(C(255, 210, 255), C(170, 70, 230)),

            // ---- Arcade & Retro
            S("Insert coin", "INSERT COIN", PressStart).In("Arcade & Retro").OnStage(C(10, 10, 20)).Colored(C(255, 230, 60))
                .WithGlow(C(255, 200, 0), 10, 1f).Animated(2f, 1f),
            S("High score", "HI-SCORE", PressStart).In("Arcade & Retro").OnStage(C(15, 10, 35))
                .WithShadow(C(40, 80, 255), 1f, 3, 3, 0)
                .WithFill(C(255, 70, 70), C(255, 230, 60), 1f),
            S("Player one", "1UP", PressStart).In("Arcade & Retro").OnStage(C(20, 20, 30)).Colored(Color.White)
                .WithShadow(C(230, 30, 60), 1f, 3, 3, 0),
            S("Arcade marquee", "ARCADE", Monoton).In("Arcade & Retro").OnStage(C(8, 8, 20)).Colored(C(200, 255, 255))
                .WithGlow(C(0, 230, 255), 24, 0.9f)
                .WithGlow(C(0, 230, 255), 6, 1.2f),
            S("Synthwave", "SYNTH", Monoton).In("Arcade & Retro").OnStage(C(25, 5, 40))
                .WithGlow(C(255, 40, 200), 20, 1f)
                .WithFill(C(80, 240, 255), C(255, 80, 220)),
            S("80s chrome", "RADICAL", Bungee).In("Arcade & Retro").OnStage(C(20, 10, 40)).Rotated(-5)
                .WithGlow(C(255, 60, 200), 14, 0.9f)
                .WithOutline(C(255, 60, 200), 2)
                .WithStops(90f, 0.8f, Color.White, C(140, 200, 255), C(40, 40, 120), C(255, 170, 230)),
            S("Vaporwave", "VAPOR", Audiowide).In("Arcade & Retro").OnStage(C(255, 200, 230)).Spaced(6)
                .WithShadow(C(0, 200, 220), 1f, 4, 4, 0)
                .WithFill(C(255, 120, 210), C(120, 200, 255)),
            S("Pixel hero", "LEVEL 1", PressStart).In("Arcade & Retro").OnStage(C(30, 40, 30))
                .WithShadow(Color.Black, 1f, 3, 3, 0)
                .WithFill(C(180, 255, 120), C(40, 170, 40), 1f),
            S("Press start", "PRESS START", PressStart).In("Arcade & Retro").OnStage(C(5, 5, 15)).Colored(Color.White)
                .WithGlow(Color.White, 8, 0.8f).Animated(1.2f, 0.9f),
            S("Pinball", "TILT", Bungee).In("Arcade & Retro").OnStage(C(20, 0, 30))
                .WithOutline(Color.Black, 6)
                .WithOutline(C(255, 220, 0), 3)
                .WithFill(C(255, 120, 100), C(220, 0, 40))
                .WithShine(Color.White, 24, 25, 0.5f, 1.5f),
            S("On air", "ON AIR", Audiowide).In("Arcade & Retro").OnStage(C(20, 5, 5)).Colored(C(255, 220, 220)).Spaced(3)
                .WithGlow(C(255, 20, 30), 22, 1.2f).Animated(flicker: 0.4f)
                .WithGlow(C(255, 40, 40), 6, 1.2f),
            S("8-bit stripes", "8 BIT", PressStart).In("Arcade & Retro").OnStage(C(10, 10, 10))
                .WithShadow(C(80, 80, 80), 1f, 3, 3, 0)
                .WithStops(90f, 1f, C(255, 60, 60), C(255, 170, 40), C(250, 240, 70), C(70, 230, 110), C(60, 170, 255)),

            // ---- Cartoon & Comic
            S("Bubble", "BUBBLE", Luckiest).In("Cartoon & Comic").OnStage(C(255, 240, 200))
                .WithShadow(C(0, 40, 90), 0.4f, 0, 6, 4, 6)
                .WithOutline(C(10, 60, 140), 7)
                .WithOutline(Color.White, 3.5f)
                .WithFill(C(200, 240, 255), C(70, 180, 255))
                .WithBevel(Color.White, C(0, 60, 140), 2f, 1.5f, 0.9f, 0.3f),
            S("Kaboom", "KABOOM!", Bangers).In("Cartoon & Comic").OnStage(C(255, 80, 60)).Rotated(-8).Spaced(1)
                .WithShadow(Color.Black, 1f, 5, 6, 0, 6)
                .WithOutline(Color.Black, 6)
                .WithOutline(Color.White, 2.5f)
                .WithFill(C(255, 250, 120), C(255, 140, 0)),
            S("Zap", "ZAP!", Bangers).In("Cartoon & Comic").OnStage(C(40, 20, 80)).Rotated(6).Spaced(2)
                .WithShadow(C(255, 40, 160), 1f, 4, 4, 0, 4)
                .WithOutline(Color.Black, 4)
                .WithFill(C(200, 255, 255), C(0, 200, 255)),
            S("Splat", "SPLAT", Luckiest).In("Cartoon & Comic").OnStage(C(90, 40, 120))
                .WithShadow(C(10, 60, 0), 1f, 0, 5, 0, 4)
                .WithOutline(C(10, 60, 0), 4)
                .WithFill(C(220, 255, 120), C(90, 200, 20))
                .WithInnerGlow(C(240, 255, 200), 3, 0.7f),
            S("Wham", "WHAM!", Luckiest).In("Cartoon & Comic").OnStage(C(40, 170, 230)).Rotated(-4)
                .WithShadow(Color.Black, 1f, 4, 5, 0, 4)
                .WithOutline(Color.Black, 4)
                .WithFill(C(255, 200, 80), C(255, 100, 0)),
            S("Barber stripes", "BONUS", Luckiest).In("Cartoon & Comic").OnStage(C(40, 40, 60))
                .WithShadow(Color.Black, 0.6f, 0, 5, 2, 4)
                .WithOutline(C(120, 0, 20), 4)
                .WithStops(45f, 1f, C(230, 30, 50), Color.White, C(230, 30, 50)).Scrolling(0.4f, 0.25f),
            S("Sugar rush", "YUMMY", Lilita).In("Cartoon & Comic").OnStage(C(130, 220, 230))
                .WithShadow(Color.Black, 0.3f, 0, 6, 4, 7)
                .WithOutline(C(200, 20, 110), 7)
                .WithOutline(Color.White, 3.5f)
                .WithFill(C(255, 210, 240), C(255, 80, 170)),
            S("Kids", "FUN!", Luckiest).In("Cartoon & Comic").OnStage(C(255, 250, 230))
                .WithOutline(C(40, 30, 90), 7)
                .WithOutline(Color.White, 3.5f)
                .WithStops(0f, 0.6f, C(255, 70, 70), C(255, 180, 40), C(80, 210, 90), C(60, 150, 255)),
            S("Toon shadow", "TOON", Luckiest).In("Cartoon & Comic").OnStage(C(255, 120, 150)).Colored(C(255, 230, 50))
                .WithShadow(C(120, 30, 60), 1f, 8, 8, 0, 3)
                .WithShadow(Color.Black, 1f, 4, 4, 0, 3)
                .WithOutline(Color.Black, 3),
            S("Doodle", "Doodle", Marker).In("Cartoon & Comic").OnStage(C(250, 250, 245)).Colored(C(30, 30, 40)).Rotated(-3)
                .WithShadow(C(150, 150, 160), 0.6f, 3, 3, 1),
            S("Marker note", "TODO!", Marker).In("Cartoon & Comic").OnStage(C(255, 245, 150)).Colored(C(220, 30, 40)).Rotated(-6),
            S("Smash", "SMASH", Bangers).In("Cartoon & Comic").OnStage(C(30, 30, 30)).Spaced(2)
                .WithShadow(C(255, 0, 60), 1f, 5, 5, 0, 4)
                .WithOutline(Color.Black, 6)
                .WithOutline(C(255, 40, 70), 3)
                .WithFill(Color.White, C(190, 190, 200)),

            // ---- Horror & Dark
            S("Zombie", "ZOMBIES", Creepster).In("Horror & Dark").OnStage(C(25, 30, 20)).Spaced(2)
                .WithShadow(Color.Black, 0.8f, 0, 4, 4, 2)
                .WithGlow(C(120, 200, 40), 12, 0.6f)
                .WithOutline(C(20, 25, 10), 2.5f)
                .WithFill(C(190, 210, 140), C(90, 120, 60), C(60, 50, 40)),
            S("Haunted", "Haunted", Fraktur).In("Horror & Dark").OnStage(C(15, 10, 25))
                .WithGlow(C(160, 120, 255), 22, 1f).Animated(0.5f, 0.4f, breathe: 0.06f)
                .WithFill(C(240, 240, 255), C(170, 170, 210))
                .WithInnerGlow(C(200, 180, 255), 3, 0.6f),
            S("Vampire", "VAMPIRE", Cinzel).In("Horror & Dark").OnStage(C(10, 0, 5)).Spaced(2)
                .WithGlow(C(200, 0, 20), 16, 0.9f)
                .WithOutline(Color.Black, 3)
                .WithFill(C(255, 90, 90), C(160, 0, 10), C(60, 0, 0))
                .WithInnerShadow(Color.Black, -2, 2, 0.5f),
            S("Cursed", "CURSED", Creepster).In("Horror & Dark").OnStage(C(15, 5, 20)).Spaced(3)
                .WithGlow(C(170, 0, 255), 18, 1.1f).Animated(flicker: 0.5f)
                .WithOutline(Color.Black, 2)
                .WithFill(C(150, 60, 200), C(30, 0, 50))
                .WithInnerGlow(C(220, 120, 255), 3, 1f, 1.5f),
            S("Bones", "BONES", Luckiest).In("Horror & Dark").OnStage(C(30, 25, 35))
                .WithShadow(Color.Black, 0.7f, 0, 4, 3, 2)
                .WithOutline(C(50, 35, 20), 3.5f)
                .WithFill(C(255, 250, 230), C(220, 205, 170))
                .WithInnerShadow(C(90, 70, 40), -2, 2, 0.6f),
            S("Hex", "Hex", Fraktur).In("Horror & Dark").OnStage(C(10, 20, 10))
                .WithGlow(C(120, 255, 40), 20, 1f).Animated(breathe: 0.05f)
                .WithOutline(C(10, 30, 0), 2.5f)
                .WithFill(C(210, 255, 150), C(60, 160, 20)),
            S("Graveyard", "R.I.P.", Cinzel).In("Horror & Dark").OnStage(C(20, 25, 30)).Spaced(3)
                .WithShadow(Color.Black, 0.8f, 0, 4, 5, 2)
                .WithOutline(C(40, 70, 30), 3)
                .WithFill(C(170, 170, 165), C(100, 100, 98))
                .WithBevel(Color.White, Color.Black, 1.5f, 1f, 0.4f, 0.6f),
            S("Nightmare", "NIGHTMARE", Creepster).In("Horror & Dark").OnStage(C(5, 5, 10)).Spaced(2)
                .WithShadow(C(255, 0, 40), 0.9f, -3, 0, 0, 0, 2, 0.5f)
                .WithShadow(C(0, 200, 255), 0.9f, 3, 0, 0, 0, 2, 0.5f)
                .WithFill(C(255, 120, 120), C(180, 0, 0)),
            S("Abyss", "ABYSS", BlackOps).In("Horror & Dark").OnStage(C(5, 10, 25)).Spaced(2)
                .WithOutline(Color.Black, 3)
                .WithFill(C(40, 70, 140), C(5, 10, 40))
                .WithInnerGlow(C(60, 220, 255), 4, 1f),
            S("Blood moon", "BLOOD MOON", Creepster).In("Horror & Dark").OnStage(C(25, 0, 5)).Spaced(2)
                .WithGlow(C(255, 20, 20), 20, 1.1f).Animated(0.6f, 0.6f)
                .WithOutline(Color.Black, 2)
                .WithFill(C(255, 150, 120), C(220, 20, 10), C(110, 0, 0)),

            // ---- Sci-fi & Neon
            S("Cyberpunk", "CYBER", BlackOps).In("Sci-fi & Neon").OnStage(C(20, 10, 30)).Spaced(2)
                .WithShadow(C(0, 240, 255), 1f, 4, 4, 0, 1)
                .WithOutline(Color.Black, 2.5f)
                .WithFill(C(255, 250, 100), C(255, 210, 0), 0.9f),
            S("Laser", "LASER", Audiowide).In("Sci-fi & Neon").OnStage(C(10, 0, 5)).Colored(Color.White).Spaced(4)
                .WithGlow(C(255, 20, 40), 22, 1.2f).Animated(flicker: 0.3f)
                .WithGlow(C(255, 60, 60), 5, 1.4f),
            S("Plasma core", "CORE", Bungee).In("Sci-fi & Neon").OnStage(C(5, 15, 25))
                .WithGlow(C(0, 230, 255), 20, 1.1f).Animated(breathe: 0.05f)
                .WithOutline(C(0, 30, 50), 2.5f)
                .WithFill(C(220, 255, 255), C(0, 170, 255), C(220, 255, 255), 0.5f, 0f, -90f).Scrolling(0.5f),
            S("Warp", "WARP", Audiowide).In("Sci-fi & Neon").OnStage(C(5, 5, 20)).Spaced(4)
                .WithGlow(C(80, 120, 255), 14, 0.9f)
                .WithFill(C(40, 60, 200), Color.White, C(40, 60, 200), 0.5f, 0f, 0f).Scrolling(0.8f),
            S("Hologram", "HOLOGRAM", Audiowide).In("Sci-fi & Neon").OnStage(C(5, 15, 25)).Colored(C(150, 240, 255)).Faded(0.85f)
                .WithGlow(C(0, 200, 255), 12, 0.9f).Animated(flicker: 0.35f)
                .WithShine(C(200, 250, 255), 10, 0, 1.2f, 1.6f),
            S("Galaxy", "GALAXY", Monoton).In("Sci-fi & Neon").OnStage(C(10, 5, 25))
                .WithGlow(C(150, 60, 255), 20, 1f)
                .WithFill(C(255, 150, 240), C(130, 90, 255), C(60, 200, 255)),
            S("Neon green", "OPEN 24/7", Monoton).In("Sci-fi & Neon").OnStage(C(5, 15, 10)).Colored(C(210, 255, 210))
                .WithGlow(C(40, 255, 80), 22, 0.9f).Animated(flicker: 0.4f)
                .WithGlow(C(40, 255, 80), 6, 1.2f).Animated(flicker: 0.3f),
            S("Neon blue", "BAR", Monoton).In("Sci-fi & Neon").OnStage(C(5, 5, 15)).Colored(C(210, 230, 255)).Spaced(4)
                .WithGlow(C(40, 100, 255), 26, 1f)
                .WithGlow(C(80, 160, 255), 6, 1.3f),
            S("Terminal", "ACCESS GRANTED", PressStart).In("Sci-fi & Neon").OnStage(C(0, 10, 0)).Colored(C(80, 255, 100))
                .WithGlow(C(40, 255, 60), 8, 0.8f),
            S("Hazard", "WARNING", BlackOps).In("Sci-fi & Neon").OnStage(C(40, 40, 45)).Spaced(2)
                .WithOutline(Color.Black, 3)
                .WithStops(45f, 1f, C(255, 210, 0), C(20, 20, 20), C(255, 210, 0)).Scrolling(0.3f, 0.2f),
            S("Mech", "MECH", BlackOps).In("Sci-fi & Neon").OnStage(C(35, 40, 45)).Spaced(3)
                .WithShadow(Color.Black, 0.7f, 0, 4, 3, 2)
                .WithOutline(C(15, 18, 22), 3)
                .WithStops(90f, 0.7f, C(220, 225, 230), C(120, 125, 135), C(80, 85, 95), C(170, 175, 185))
                .WithBevel(Color.White, Color.Black, 1f, 0.75f, 0.7f, 0.5f),
            S("Energy", "ENERGY", Bungee).In("Sci-fi & Neon").OnStage(C(10, 20, 10))
                .WithGlow(C(180, 255, 0), 16, 1.1f).Animated(3f, 0.5f)
                .WithOutline(C(20, 50, 0), 2.5f)
                .WithFill(C(255, 255, 120), C(120, 230, 0)),

            // ---- Materials & Nature
            S("Marble", "MARBLE", Cinzel).In("Materials & Nature").OnStage(C(40, 50, 60)).Spaced(2)
                .WithShadow(Color.Black, 0.6f, 0, 4, 3, 2)
                .WithOutline(C(60, 60, 70), 2)
                .WithStops(30f, 0.3f, Color.White, C(200, 200, 210), Color.White, C(170, 170, 185), C(235, 235, 240))
                .WithBevel(Color.White, Color.Black, 1.2f, 1f, 0.6f, 0.4f),
            S("Copper", "COPPER", Bungee).In("Materials & Nature").OnStage(C(30, 40, 40))
                .WithShadow(Color.Black, 0.6f, 0, 4, 3, 2.5f)
                .WithOutline(C(60, 25, 10), 3)
                .WithFill(C(255, 200, 160), C(190, 95, 45), C(110, 50, 20))
                .WithBevel(C(255, 230, 200), Color.Black, 1.5f, 1f, 0.7f, 0.5f),
            S("Platinum", "PLATINUM", Cinzel).In("Materials & Nature").OnStage(C(20, 25, 35)).Spaced(2)
                .WithShadow(Color.Black, 0.6f, 0, 4, 3, 2)
                .WithOutline(C(40, 45, 60), 2.5f)
                .WithStops(90f, 0.85f, Color.White, C(200, 215, 230), C(120, 135, 160), C(220, 230, 245))
                .WithShine(Color.White, 26, 25, 0.9f, 3.5f),
            S("Obsidian", "OBSIDIAN", BlackOps).In("Materials & Nature").OnStage(C(150, 140, 175)).Spaced(1)
                .WithShadow(Color.Black, 0.6f, 0, 4, 3, 2)
                .WithFill(C(60, 40, 80), C(15, 10, 25))
                .WithBevel(C(210, 170, 255), Color.Black, 1.5f, 1f, 0.9f, 0.6f),
            S("Amber", "AMBER", Luckiest).In("Materials & Nature").OnStage(C(40, 25, 10))
                .WithGlow(C(255, 160, 0), 12, 0.7f)
                .WithOutline(C(90, 40, 0), 3.5f)
                .WithFill(C(255, 220, 100), C(230, 130, 0), C(160, 70, 0))
                .WithInnerGlow(C(255, 240, 160), 3, 0.7f),
            S("Jade", "JADE", Cinzel).In("Materials & Nature").OnStage(C(240, 235, 220)).Spaced(3)
                .WithShadow(C(0, 40, 20), 0.4f, 0, 4, 3, 2)
                .WithOutline(C(0, 60, 40), 2.5f)
                .WithFill(C(180, 255, 210), C(40, 160, 110), C(10, 90, 60))
                .WithBevel(Color.White, Color.Black, 1.2f, 1f, 0.7f, 0.4f),
            S("Magma", "MAGMA", Bungee).In("Materials & Nature").OnStage(C(15, 5, 5))
                .WithGlow(C(255, 80, 0), 16, 1.1f)
                .WithOutline(Color.Black, 2.5f)
                .WithFill(C(80, 40, 30), C(30, 15, 10))
                .WithInnerGlow(C(255, 120, 0), 5, 1.3f),
            S("Waves", "WAVES", Luckiest).In("Materials & Nature").OnStage(C(240, 230, 200))
                .WithShadow(C(0, 40, 90), 0.5f, 0, 5, 3, 4)
                .WithOutline(C(0, 40, 90), 4)
                .WithFill(C(150, 230, 255), C(30, 120, 220), C(150, 230, 255), 0.5f, 0f, 0f).Scrolling(0.25f),
            S("Grass", "GRASS", Luckiest).In("Materials & Nature").OnStage(C(130, 200, 255))
                .WithShadow(Color.Black, 0.4f, 0, 5, 3, 3)
                .WithOutline(C(40, 30, 10), 4)
                .WithFill(C(140, 230, 60), C(60, 160, 30), C(130, 80, 40), 0.42f, 0.9f),
            S("Snow", "SNOW", Luckiest).In("Materials & Nature").OnStage(C(60, 110, 170))
                .WithShadow(C(0, 30, 80), 0.5f, 0, 5, 3, 3)
                .WithOutline(C(30, 70, 150), 4)
                .WithFill(Color.White, C(240, 248, 255), C(150, 200, 255), 0.5f, 0.6f)
                .WithBevel(Color.White, C(30, 70, 150), 1.5f, 1f, 0.9f, 0.4f),
            S("Desert", "DESERT", Rye).In("Materials & Nature").OnStage(C(120, 180, 230))
                .WithShadow(C(90, 50, 10), 0.6f, 0, 4, 2, 2.5f)
                .WithOutline(C(100, 55, 15), 3)
                .WithFill(C(255, 235, 170), C(225, 175, 90), C(180, 120, 50)),
            S("Forest", "Forest", Kaushan).In("Materials & Nature").OnStage(C(230, 240, 220))
                .WithShadow(C(0, 40, 0), 0.4f, 0, 4, 3, 2)
                .WithOutline(C(10, 60, 20), 3)
                .WithFill(C(160, 230, 90), C(30, 120, 40)),
            S("Embers", "COAL", BlackOps).In("Materials & Nature").OnStage(C(90, 80, 75)).Spaced(3)
                .WithGlow(C(255, 90, 0), 10, 0.6f)
                .WithFill(C(70, 65, 65), C(25, 22, 22))
                .WithInnerGlow(C(255, 100, 0), 4, 1.2f, 1.2f),
            S("Crystal", "CRYSTAL", Cinzel).In("Materials & Nature").OnStage(C(40, 20, 60)).Spaced(2)
                .WithGlow(C(255, 150, 255), 14, 0.8f)
                .WithOutline(C(80, 30, 110), 2.5f)
                .WithStops(60f, 0.7f, C(255, 220, 255), C(200, 140, 255), C(255, 200, 240), C(150, 120, 255))
                .WithInnerGlow(Color.White, 2.5f, 0.7f)
                .WithShine(Color.White, 22, 30, 0.7f, 2.5f),

            // ---- Western, Military & Vintage
            S("Wanted", "WANTED", Rye).In("Western & Vintage").OnStage(C(230, 205, 150)).Colored(C(70, 40, 15)).Spaced(2)
                .WithInnerShadow(Color.Black, 2, 1.5f, 0.5f),
            S("Saloon", "SALOON", Rye).In("Western & Vintage").OnStage(C(90, 50, 25))
                .WithShadow(Color.Black, 0.6f, 0, 4, 2, 2.5f)
                .WithOutline(C(50, 25, 5), 3)
                .WithFill(C(255, 235, 170), C(230, 170, 60), C(170, 100, 20))
                .WithBevel(Color.White, Color.Black, 1.2f, 1f, 0.6f, 0.4f),
            S("Sheriff", "SHERIFF", Rye).In("Western & Vintage").OnStage(C(130, 80, 40))
                .WithShadow(Color.Black, 0.6f, 0, 4, 2, 2.5f)
                .WithOutline(C(40, 40, 50), 3)
                .WithStops(90f, 0.8f, Color.White, C(190, 195, 205), C(110, 115, 130), C(200, 205, 215)),
            S("Army", "ARMY", BlackOps).In("Western & Vintage").OnStage(C(200, 190, 150)).Spaced(4)
                .WithShadow(Color.Black, 0.5f, 3, 3, 1)
                .WithFill(C(120, 130, 70), C(70, 80, 40)),
            S("Stencil stamp", "CLASSIFIED", BlackOps).In("Western & Vintage").OnStage(C(235, 225, 200)).Colored(C(200, 30, 30)).Rotated(-8).Faded(0.85f)
                .WithBuiltInOutline(C(200, 30, 30), 1),
            S("Typewriter", "Top Secret", Typewriter).In("Western & Vintage").OnStage(C(240, 235, 220)).Colored(C(30, 30, 35)),
            S("Old newspaper", "EXTRA!", Typewriter).In("Western & Vintage").OnStage(C(225, 220, 205)).Colored(C(40, 40, 40)).Spaced(3)
                .WithShadow(Color.Black, 0.25f, 1, 1, 1),
            S("Telegram", "STOP", Typewriter).In("Western & Vintage").OnStage(C(250, 230, 180)).Colored(C(110, 60, 20)).Spaced(6),
            S("Motel sign", "MOTEL", Bungee).In("Western & Vintage").OnStage(C(30, 50, 70))
                .WithShadow(Color.Black, 0.6f, 0, 5, 3, 6)
                .WithOutline(C(60, 10, 10), 6.5f)
                .WithOutline(C(255, 240, 200), 3)
                .WithFill(C(255, 110, 90), C(210, 20, 30)),
            S("Circus", "CIRCUS", Rye).In("Western & Vintage").OnStage(C(30, 40, 90))
                .WithShadow(Color.Black, 0.6f, 0, 4, 2, 3)
                .WithOutline(C(255, 210, 60), 3)
                .WithStops(0f, 1f, C(220, 20, 40), Color.White, C(220, 20, 40), Color.White, C(220, 20, 40), Color.White, C(220, 20, 40)),

            // ---- Elegant & Script
            S("Wedding", "Forever", Pacifico).In("Elegant & Script").OnStage(C(250, 240, 235))
                .WithShadow(C(150, 110, 60), 0.4f, 0, 3, 4)
                .WithOutline(C(190, 150, 80), 1.5f)
                .WithFill(C(255, 245, 220), C(230, 200, 140)),
            S("Royal", "ROYAL", Cinzel).In("Elegant & Script").OnStage(C(20, 10, 40)).Spaced(3)
                .WithShadow(Color.Black, 0.6f, 0, 4, 3, 2)
                .WithOutline(C(30, 0, 50), 4.5f)
                .WithOutline(C(255, 210, 90), 2)
                .WithFill(C(220, 160, 255), C(120, 40, 200), C(60, 10, 120)),
            S("Signature", "Bravo!", Kaushan).In("Elegant & Script").OnStage(C(30, 40, 60)).Colored(Color.White).Rotated(-6)
                .WithShadow(Color.Black, 0.6f, 0, 4, 4),
            S("Luxury", "LUXURY", Cinzel).In("Elegant & Script").OnStage(C(8, 8, 10)).Spaced(6)
                .WithStops(90f, 0.6f, C(255, 245, 200), C(220, 180, 90), C(150, 105, 40), C(240, 210, 130))
                .WithShine(C(255, 250, 230), 20, 30, 1f, 4f),
            S("Calligraphy", "Chapter I", Fraktur).In("Elegant & Script").OnStage(C(240, 225, 190)).Colored(C(40, 25, 15)),
            S("Brush", "Adventure", Kaushan).In("Elegant & Script").OnStage(C(40, 90, 120))
                .WithShadow(Color.Black, 0.6f, 0, 4, 3, 2)
                .WithOutline(C(60, 20, 0), 3)
                .WithFill(C(255, 220, 100), C(255, 120, 20)),
            S("Champagne", "Cheers", Pacifico).In("Elegant & Script").OnStage(C(30, 20, 30))
                .WithGlow(C(255, 210, 120), 14, 0.8f)
                .WithFill(C(255, 250, 220), C(230, 200, 130))
                .WithShine(Color.White, 20, 25, 0.8f, 2.5f),
            S("Ink", "Legend", Kaushan).In("Elegant & Script").OnStage(C(245, 240, 225)).Colored(C(20, 30, 70))
                .WithInnerShadow(Color.Black, 1.5f, 1f, 0.5f),

            // ---- Seasons & Holidays
            S("Christmas", "MERRY XMAS", Luckiest).In("Seasons & Holidays").OnStage(C(20, 60, 40))
                .WithShadow(Color.Black, 0.5f, 0, 5, 3, 6)
                .WithOutline(C(20, 110, 50), 6)
                .WithOutline(Color.White, 3)
                .WithFill(C(255, 120, 120), C(210, 20, 30)),
            S("Candy cane", "CANDY", Luckiest).In("Seasons & Holidays").OnStage(C(230, 245, 240))
                .WithShadow(Color.Black, 0.35f, 0, 5, 3, 4)
                .WithOutline(C(130, 0, 20), 3.5f)
                .WithStops(45f, 1f, C(230, 20, 40), Color.White, C(230, 20, 40), Color.White, C(230, 20, 40), Color.White, C(230, 20, 40), Color.White)
                .WithBevel(Color.White, Color.Black, 1.5f, 1f, 0.6f, 0.3f),
            S("Halloween", "BOO!", Luckiest).In("Seasons & Holidays").OnStage(C(25, 10, 35)).Rotated(-4)
                .WithGlow(C(160, 60, 255), 16, 0.9f)
                .WithOutline(Color.Black, 4)
                .WithFill(C(255, 200, 80), C(255, 110, 0)),
            S("Spring", "Spring", Pacifico).In("Seasons & Holidays").OnStage(C(240, 255, 240))
                .WithShadow(C(80, 160, 80), 0.4f, 0, 3, 3)
                .WithOutline(C(60, 140, 70), 2)
                .WithFill(C(180, 240, 140), C(255, 170, 210)),
            S("Summer", "SUMMER", Luckiest).In("Seasons & Holidays").OnStage(C(60, 200, 220))
                .WithShadow(C(0, 90, 110), 0.7f, 0, 5, 2, 4)
                .WithOutline(C(0, 110, 130), 4)
                .WithFill(C(255, 245, 100), C(255, 150, 30)),
            S("Autumn", "AUTUMN", Rye).In("Seasons & Holidays").OnStage(C(250, 235, 210))
                .WithShadow(C(80, 30, 0), 0.5f, 0, 4, 3, 2)
                .WithOutline(C(80, 30, 0), 3)
                .WithFill(C(255, 190, 60), C(220, 90, 20), C(130, 50, 20)),
            S("Winter", "WINTER", Cinzel).In("Seasons & Holidays").OnStage(C(20, 40, 80)).Spaced(3)
                .WithGlow(C(120, 190, 255), 16, 0.9f)
                .WithOutline(C(10, 40, 100), 3)
                .WithOutline(Color.White, 1.2f)
                .WithFill(Color.White, C(170, 220, 255)),
            S("Valentine", "Be Mine", Pacifico).In("Seasons & Holidays").OnStage(C(255, 220, 230))
                .WithGlow(C(255, 60, 120), 14, 0.8f).Animated(1.1f, 0.7f, breathe: 0.04f)
                .WithOutline(C(170, 0, 60), 2.5f)
                .WithFill(C(255, 160, 200), C(240, 40, 110)),
            S("New year", "2027", Bungee).In("Seasons & Holidays").OnStage(C(10, 10, 30))
                .WithGlow(C(255, 200, 60), 18, 1f).Animated(0.8f, 0.4f, cycle: CyclePalettes[6])
                .WithOutline(C(60, 30, 0), 3)
                .WithFill(C(255, 250, 200), C(255, 200, 50), C(220, 130, 0))
                .WithShine(Color.White, 24, 25, 0.8f, 2f),
            S("Easter", "EASTER", Luckiest).In("Seasons & Holidays").OnStage(C(255, 250, 235))
                .WithShadow(C(120, 80, 160), 0.4f, 0, 5, 3, 4)
                .WithOutline(C(120, 80, 160), 4)
                .WithStops(90f, 0.9f, C(255, 200, 220), C(200, 230, 255), C(210, 255, 200), C(255, 240, 180)),
            // ---- Layered titles: many effects inside the letters (fills, gloss bands, patterns, sparkles, bevels, rims)
            S("Fantasy gold", "Nimavora", Cinzel).In("Layered titles").OnStage(C(20, 30, 70))
                .WithShadow(Color.Black, 0.6f, 0, 5, 5, 4)
                .WithGlow(C(60, 120, 255), 18, 0.9f)
                .WithOutline(C(12, 16, 50), 5)
                .WithOutline(C(120, 70, 20), 2.5f)
                .WithStops(90f, 0.3f, C(255, 252, 215), C(245, 190, 70), C(150, 80, 20), C(255, 225, 130), C(200, 120, 30))
                .WithPattern(ShadingPattern.Noise, C(120, 60, 0), 0.25f, 6)
                .WithBevel(Color.White, C(90, 40, 0), 1.5f, 1f, 0.9f, 0.6f)
                .WithInnerGlow(C(255, 250, 220), 1.5f, 0.7f)
                .WithSparkles(Color.White, 12, 20),
            S("Arena", "ARENA", Luckiest).In("Layered titles").OnStage(C(70, 110, 170))
                .WithShadow(C(20, 10, 5), 0.8f, 0, 6, 2, 5)
                .WithOutline(C(30, 18, 10), 5)
                .WithFill(C(255, 225, 100), C(255, 160, 25), C(225, 95, 0))
                .WithGloss(0.55f, 0.45f)
                .WithInnerShadow(C(120, 40, 0), -2.5f, 1.5f, 0.7f)
                .WithBevel(Color.White, C(80, 25, 0), 1.2f, 1f, 0.6f, 0.3f),
            S("Heroes", "HEROES", AlfaSlab).In("Layered titles").OnStage(C(60, 120, 100))
                .WithShadow(Color.Black, 0.5f, 0, 5, 4, 7)
                .WithGlow(C(255, 170, 40), 14, 0.6f)
                .WithOutline(C(255, 205, 90), 7.5f)
                .WithOutline(C(95, 30, 0), 4.5f)
                .WithFill(C(255, 245, 200), C(255, 175, 45), C(240, 110, 10))
                .WithGloss(0.7f, 0.42f)
                .WithBevel(Color.White, C(140, 40, 0), 1.5f, 1.2f, 0.7f, 0.5f),
            S("Kingdom crown", "KINGDOM", Cinzel).In("Layered titles").OnStage(C(40, 20, 60)).Spaced(2)
                .WithShadow(Color.Black, 0.6f, 0, 5, 4, 4)
                .WithOutline(C(50, 10, 80), 5)
                .WithOutline(C(255, 220, 120), 2)
                .WithStops(90f, 0.4f, C(255, 250, 220), C(240, 190, 80), C(160, 90, 20), C(250, 215, 120))
                .WithPattern(ShadingPattern.Noise, C(100, 50, 0), 0.2f, 5)
                .WithGloss(0.4f, 0.4f)
                .WithBevel(Color.White, Color.Black, 1.5f, 1f, 0.8f, 0.4f),
            S("Lime fortune", "FORTUNE", Bungee).In("Layered titles").OnStage(C(30, 70, 180))
                .WithShadow(C(0, 30, 0), 0.7f, 0, 5, 2, 4)
                .WithOutline(C(10, 50, 0), 4)
                .WithFill(C(220, 255, 110), C(100, 205, 10), C(35, 120, 0))
                .WithGloss(0.6f, 0.45f)
                .WithInnerShadow(C(0, 60, 0), -2, 1.5f, 0.6f)
                .WithInnerGlow(C(240, 255, 200), 1.5f, 0.6f),
            S("Titans", "Titans", Kaushan).In("Layered titles").OnStage(C(25, 20, 30))
                .WithShadow(Color.Black, 0.7f, 2, 5, 4, 3)
                .WithGlow(C(255, 160, 40), 14, 0.7f)
                .WithOutline(C(70, 30, 0), 3.5f)
                .WithFill(C(255, 240, 170), C(250, 180, 40), C(190, 100, 10))
                .WithGloss(0.5f, 0.45f)
                .WithBevel(Color.White, C(90, 40, 0), 1.2f, 1f, 0.7f, 0.5f),
            S("Candy gloss", "SUGAR", Luckiest).In("Layered titles").OnStage(C(140, 220, 240))
                .WithShadow(Color.Black, 0.3f, 0, 7, 4, 7)
                .WithOutline(C(190, 20, 110), 7)
                .WithOutline(Color.White, 3.5f)
                .WithFill(C(255, 200, 235), C(255, 90, 180), C(220, 30, 140))
                .WithPattern(ShadingPattern.Dots, Color.White, 0.3f, 7, true)
                .WithGloss(0.55f, 0.45f)
                .WithBevel(Color.White, C(120, 0, 60), 2f, 1.5f, 0.6f, 0.3f),
            S("Jelly", "JELLY", Lilita).In("Layered titles").OnStage(C(250, 240, 210))
                .WithShadow(C(0, 80, 20), 0.4f, 0, 6, 5, 4)
                .WithOutline(C(20, 110, 40), 4)
                .WithFill(C(200, 255, 170), C(90, 220, 90), C(30, 160, 60))
                .WithPattern(ShadingPattern.Blotches, Color.White, 0.25f, 22, true)
                .WithGloss(0.75f, 0.4f, null, 0.05f)
                .WithInnerGlow(Color.White, 4, 0.8f),
            S("Turbo chrome", "TURBO", BlackOps).In("Layered titles").OnStage(C(25, 25, 35)).Spaced(2)
                .WithShadow(Color.Black, 0.7f, 0, 5, 3, 3)
                .WithOutline(C(200, 20, 30), 4.5f)
                .WithOutline(C(15, 15, 20), 2)
                .WithStops(90f, 0.85f, Color.White, C(170, 185, 205), C(50, 60, 80), C(210, 220, 235))
                .WithStripes(Color.White, 0.25f, 60f, 0.18f)
                .WithBevel(Color.White, Color.Black, 1f, 0.75f, 0.8f, 0.4f),
            S("Racing flag", "RACE", Bungee).In("Layered titles").OnStage(C(240, 240, 240))
                .WithShadow(Color.Black, 0.5f, 0, 5, 3, 6)
                .WithOutline(Color.Black, 6)
                .WithOutline(Color.White, 3)
                .WithFill(C(255, 90, 80), C(210, 10, 20))
                .WithPattern(ShadingPattern.Checker, Color.Black, 0.3f, 12)
                .WithGloss(0.45f, 0.45f),
            S("Dragon scales", "DRAGON", Cinzel).In("Layered titles").OnStage(C(20, 35, 25)).Spaced(2)
                .WithShadow(Color.Black, 0.7f, 0, 5, 4, 3)
                .WithGlow(C(80, 255, 120), 14, 0.6f)
                .WithOutline(C(10, 30, 10), 4)
                .WithOutline(C(220, 180, 60), 1.5f)
                .WithFill(C(210, 240, 120), C(70, 160, 50), C(20, 80, 30))
                .WithPattern(ShadingPattern.Scales, C(0, 40, 10), 0.5f, 9)
                .WithBevel(C(255, 250, 200), Color.Black, 1.2f, 1f, 0.7f, 0.5f),
            S("Temple stone", "TEMPLE", Cinzel).In("Layered titles").OnStage(C(80, 120, 90)).Spaced(2)
                .WithShadow(Color.Black, 0.7f, 0, 5, 3, 3)
                .WithOutline(C(40, 60, 30), 3.5f)
                .WithFill(C(215, 210, 195), C(150, 145, 130), C(100, 95, 85))
                .WithPattern(ShadingPattern.Noise, Color.Black, 0.45f, 5)
                .WithInnerShadow(Color.Black, 2.5f, 2, 0.5f)
                .WithBevel(Color.White, Color.Black, 2f, 1f, 0.5f, 0.5f),
            S("Tavern wood", "TAVERN", Rye).In("Layered titles").OnStage(C(40, 70, 40))
                .WithShadow(Color.Black, 0.6f, 0, 5, 3, 3)
                .WithOutline(C(50, 25, 5), 3.5f)
                .WithFill(C(220, 160, 95), C(165, 100, 50), C(110, 60, 25))
                .WithStripes(C(80, 40, 10), 0.35f, 85f, 0.14f, 0f, false, 0.12f)
                .WithPattern(ShadingPattern.Noise, C(60, 30, 0), 0.3f, 4)
                .WithBevel(C(255, 225, 180), Color.Black, 1.5f, 1f, 0.6f, 0.5f),
            S("Inferno rock", "INFERNO", BlackOps).In("Layered titles").OnStage(C(20, 8, 5)).Spaced(2)
                .WithGlow(C(255, 80, 0), 16, 1f)
                .WithOutline(Color.Black, 2.5f)
                .WithFill(C(80, 45, 35), C(30, 15, 10))
                .WithPattern(ShadingPattern.Blotches, C(255, 120, 0), 0.8f, 14, true)
                .WithInnerGlow(C(255, 140, 0), 3, 1f),
            S("Glacier", "GLACIER", Cinzel).In("Layered titles").OnStage(C(20, 50, 90)).Spaced(2)
                .WithGlow(C(80, 180, 255), 16, 0.8f)
                .WithOutline(C(10, 40, 100), 4)
                .WithOutline(Color.White, 1.5f)
                .WithFill(Color.White, C(160, 225, 255), C(50, 140, 230))
                .WithStripes(Color.White, 0.3f, 60f, 0.3f, 0f, true, 0.15f)
                .WithGloss(0.5f, 0.45f)
                .WithInnerGlow(C(220, 245, 255), 3, 0.8f)
                .WithSparkles(Color.White, 10, 16),
            S("Mecha panel", "MECHA", Audiowide).In("Layered titles").OnStage(C(25, 30, 40)).Spaced(3)
                .WithShadow(Color.Black, 0.7f, 0, 4, 3, 2)
                .WithGlow(C(0, 200, 255), 10, 0.6f)
                .WithOutline(C(10, 15, 25), 3)
                .WithStops(90f, 0.7f, C(220, 228, 240), C(130, 140, 160), C(80, 90, 110), C(180, 190, 210))
                .WithPattern(ShadingPattern.Grid, Color.Black, 0.35f, 10)
                .WithGloss(0.35f, 0.45f)
                .WithInnerGlow(C(120, 230, 255), 1.5f, 0.8f),
            S("Retro TV", "RETRO", Bungee).In("Layered titles").OnStage(C(30, 15, 45))
                .WithGlow(C(255, 60, 180), 14, 0.8f)
                .WithOutline(C(40, 0, 50), 3.5f)
                .WithFill(C(255, 220, 80), C(255, 100, 120), C(170, 40, 200))
                .WithPattern(ShadingPattern.ScanLines, Color.Black, 0.35f, 6)
                .WithGloss(0.35f, 0.4f),
            S("Comic halftone", "BANG!", Bangers).In("Layered titles").OnStage(C(60, 180, 240)).Rotated(-6).Spaced(2)
                .WithShadow(Color.Black, 1f, 5, 5, 0, 5)
                .WithOutline(Color.Black, 5)
                .WithFill(C(255, 245, 120), C(255, 150, 0))
                .WithPattern(ShadingPattern.Dots, C(220, 40, 0), 0.4f, 6)
                .WithGloss(0.5f, 0.4f),
            S("Wild zebra", "WILD", Luckiest).In("Layered titles").OnStage(C(255, 200, 40))
                .WithShadow(Color.Black, 0.8f, 4, 5, 0, 5)
                .WithOutline(Color.Black, 5)
                .WithFill(Color.White, C(225, 225, 230))
                .WithStripes(Color.Black, 0.9f, 70f, 0.22f, 0f, false, 0.03f)
                .WithGloss(0.3f, 0.4f),
            S("Empire marble", "EMPIRE", Cinzel).In("Layered titles").OnStage(C(40, 35, 50)).Spaced(3)
                .WithShadow(Color.Black, 0.6f, 0, 4, 3, 4)
                .WithOutline(C(30, 25, 40), 4)
                .WithOutline(C(230, 190, 90), 1.8f)
                .WithFill(Color.White, C(230, 230, 235))
                .WithPattern(ShadingPattern.Blotches, C(110, 110, 130), 0.35f, 20)
                .WithBevel(Color.White, Color.Black, 1.2f, 1f, 0.6f, 0.4f)
                .WithInnerShadow(Color.Black, 2f, 2f, 0.25f),
            S("Camo squad", "SQUAD", BlackOps).In("Layered titles").OnStage(C(200, 190, 150)).Spaced(3)
                .WithShadow(Color.Black, 0.6f, 3, 4, 1, 2)
                .WithOutline(C(25, 30, 15), 3)
                .WithFill(C(140, 150, 90), C(100, 110, 60))
                .WithPattern(ShadingPattern.Blotches, C(40, 50, 20), 0.75f, 16)
                .WithInnerShadow(Color.Black, 2, 1.5f, 0.4f),
            S("Emerald jewel", "JEWEL", Cinzel).In("Layered titles").OnStage(C(15, 25, 20)).Spaced(2)
                .WithGlow(C(40, 255, 140), 14, 0.8f)
                .WithOutline(C(5, 25, 15), 4)
                .WithOutline(C(240, 200, 90), 1.8f)
                .WithStops(35f, 1f, C(170, 255, 200), C(40, 190, 100), C(120, 240, 170), C(10, 120, 60), C(80, 220, 140))
                .WithInnerGlow(Color.White, 2, 0.6f)
                .WithSparkles(Color.White, 10, 18),
            S("Ruby jewel", "RUBY", Lilita).In("Layered titles").OnStage(C(30, 10, 25))
                .WithGlow(C(255, 40, 90), 14, 0.8f)
                .WithOutline(C(60, 0, 20), 4)
                .WithOutline(C(255, 220, 140), 1.8f)
                .WithStops(30f, 1f, C(255, 160, 180), C(220, 20, 60), C(255, 90, 120), C(140, 0, 30), C(240, 60, 90))
                .WithGloss(0.45f, 0.4f)
                .WithSparkles(Color.White, 8, 20, 0f, 7),
            S("Sapphire jewel", "SAPPHIRE", Cinzel).In("Layered titles").OnStage(C(10, 15, 35)).Spaced(1)
                .WithGlow(C(60, 120, 255), 14, 0.8f)
                .WithOutline(C(5, 10, 40), 4)
                .WithOutline(C(220, 220, 240), 1.5f)
                .WithStops(40f, 1f, C(170, 200, 255), C(40, 80, 230), C(100, 150, 255), C(15, 30, 140), C(70, 120, 250))
                .WithBevel(Color.White, Color.Black, 1.2f, 1f, 0.8f, 0.4f)
                .WithSparkles(Color.White, 12, 16, 0f, 3),
            S("Bronze legion", "LEGION", Rye).In("Layered titles").OnStage(C(60, 30, 25))
                .WithShadow(Color.Black, 0.7f, 0, 5, 3, 3)
                .WithOutline(C(40, 20, 5), 4)
                .WithFill(C(255, 205, 150), C(195, 110, 50), C(110, 55, 20))
                .WithPattern(ShadingPattern.Noise, C(60, 30, 0), 0.35f, 5)
                .WithBevel(C(255, 235, 200), Color.Black, 2f, 1f, 0.8f, 0.6f)
                .WithInnerGlow(C(255, 220, 170), 1.2f, 0.5f),
            S("Bubblegum", "POP!", Luckiest).In("Layered titles").OnStage(C(110, 80, 200))
                .WithShadow(C(120, 0, 70), 1f, 0, 5, 0, 5)
                .WithOutline(C(120, 0, 70), 5)
                .WithFill(C(255, 190, 230), C(255, 80, 170))
                .WithPattern(ShadingPattern.Dots, Color.White, 0.35f, 10, true)
                .WithGloss(0.8f, 0.45f, null, 0.05f)
                .WithInnerShadow(C(150, 0, 80), -2, 1.5f, 0.5f),
            S("Golden plaque", "VICTORY", AlfaSlab).In("Layered titles").OnStage(C(25, 15, 40))
                .WithShadow(Color.Black, 0.7f, 0, 5, 4, 6)
                .WithOutline(C(255, 220, 120), 6)
                .WithOutline(C(70, 30, 0), 3.5f)
                .WithStops(90f, 0.35f, C(255, 250, 215), C(250, 200, 80), C(170, 100, 20), C(255, 225, 120))
                .WithPattern(ShadingPattern.Diagonal, Color.White, 0.15f, 4, true)
                .WithGloss(0.45f, 0.42f)
                .WithBevel(Color.White, C(90, 40, 0), 1.5f, 1f, 0.8f, 0.5f),
            S("Spooky mist", "SPOOKY", Creepster).In("Layered titles").OnStage(C(15, 10, 25)).Spaced(2)
                .WithGlow(C(120, 255, 80), 14, 0.7f)
                .WithOutline(Color.Black, 2.5f)
                .WithFill(C(200, 140, 255), C(90, 30, 160))
                .WithPattern(ShadingPattern.Blotches, Color.Black, 0.45f, 18)
                .WithInnerGlow(C(150, 255, 120), 3, 0.9f),
            S("Toy plastic", "TOYS", Lilita).In("Layered titles").OnStage(C(255, 230, 120))
                .WithShadow(C(0, 30, 90), 0.6f, 0, 6, 2, 7)
                .WithOutline(C(0, 30, 90), 7)
                .WithOutline(Color.White, 3.5f)
                .WithFill(C(140, 200, 255), C(30, 110, 230))
                .WithGloss(0.8f, 0.48f)
                .WithInnerShadow(C(0, 30, 120), -2.5f, 1.5f, 0.5f),

            // ---- Layered and animated
            S("Fantasy magic", "Nimavora", Cinzel).In("Layered & animated").OnStage(C(20, 30, 70))
                .WithShadow(Color.Black, 0.6f, 0, 5, 5, 4)
                .WithGlow(C(60, 120, 255), 18, 0.9f).Animated(breathe: 0.04f)
                .WithOutline(C(12, 16, 50), 5)
                .WithOutline(C(120, 70, 20), 2.5f)
                .WithStops(90f, 0.3f, C(255, 252, 215), C(245, 190, 70), C(150, 80, 20), C(255, 225, 130), C(200, 120, 30))
                .WithPattern(ShadingPattern.Noise, C(120, 60, 0), 0.25f, 6)
                .WithBevel(Color.White, C(90, 40, 0), 1.5f, 1f, 0.9f, 0.6f)
                .WithInnerGlow(C(255, 250, 220), 1.5f, 0.7f)
                .WithSparkles(Color.White, 14, 22, 0.6f)
                .WithShine(C(255, 250, 225), 30, 25, 1f, 3.5f),
            S("Shiny arena", "CLASH", Luckiest).In("Layered & animated").OnStage(C(70, 110, 170))
                .WithShadow(C(20, 10, 5), 0.8f, 0, 6, 2, 5)
                .WithOutline(C(30, 18, 10), 5)
                .WithFill(C(255, 225, 100), C(255, 160, 25), C(225, 95, 0))
                .WithGloss(0.55f, 0.45f)
                .WithInnerShadow(C(120, 40, 0), -2.5f, 1.5f, 0.7f)
                .WithShine(Color.White, 28, 25, 0.7f, 2.5f),
            S("Jackpot", "JACKPOT", Bungee).In("Layered & animated").OnStage(C(60, 0, 40))
                .WithShadow(Color.Black, 0.6f, 0, 5, 3, 4)
                .WithGlow(C(255, 200, 40), 16, 1f).Animated(1f, 0.4f)
                .WithOutline(C(80, 30, 0), 4)
                .WithFill(C(255, 250, 190), C(255, 200, 40), C(220, 120, 0))
                .WithGloss(0.6f, 0.45f)
                .WithSparkles(Color.White, 14, 18, 1.2f, 5)
                .WithShine(Color.White, 26, 25, 0.6f, 2f),
            S("Casino lights", "CASINO", Bungee).In("Layered & animated").OnStage(C(25, 5, 15))
                .WithGlow(C(255, 40, 60), 14, 0.8f)
                .WithOutline(C(255, 210, 80), 5)
                .WithOutline(C(60, 0, 10), 3)
                .WithFill(C(255, 100, 100), C(200, 0, 30))
                .WithPattern(ShadingPattern.Dots, C(255, 230, 120), 0.7f, 8, true, 16f, 0f)
                .WithGloss(0.4f, 0.45f),
            S("Candy stripes", "CANDY", Luckiest).In("Layered & animated").OnStage(C(200, 240, 230))
                .WithShadow(C(130, 0, 30), 0.5f, 0, 5, 2, 5)
                .WithOutline(C(130, 0, 30), 5)
                .WithFill(Color.White, C(240, 235, 240))
                .WithStripes(C(230, 20, 50), 0.95f, 45f, 0.25f, 0.4f, false, 0.03f)
                .WithGloss(0.7f, 0.45f),
            S("Magma flow", "MAGMA", BlackOps).In("Layered & animated").OnStage(C(15, 5, 5)).Spaced(2)
                .WithGlow(C(255, 70, 0), 18, 1.2f).Animated(flicker: 0.4f)
                .WithOutline(Color.Black, 2.5f)
                .WithFill(C(70, 35, 25), C(25, 10, 5))
                .WithPattern(ShadingPattern.Blotches, C(255, 130, 0), 0.9f, 16, true, 0f, -10f)
                .WithInnerGlow(C(255, 180, 40), 3, 1.1f, 1f),
            S("Hacker", "HACKER", PressStart).In("Layered & animated").OnStage(C(0, 8, 0))
                .WithGlow(C(40, 255, 80), 10, 0.8f).Animated(flicker: 0.3f)
                .WithOutline(C(0, 40, 0), 2)
                .WithFill(C(180, 255, 180), C(30, 200, 60))
                .WithPattern(ShadingPattern.ScanLines, Color.Black, 0.45f, 5, false, 0f, 8f),
            S("Holo grid", "HOLO", Audiowide).In("Layered & animated").OnStage(C(5, 15, 30)).Spaced(4)
                .WithGlow(C(0, 220, 255), 14, 0.9f).Animated(flicker: 0.3f)
                .WithOutline(C(0, 40, 70), 2.5f)
                .WithFill(C(180, 250, 255), C(0, 160, 230))
                .WithPattern(ShadingPattern.Grid, Color.White, 0.5f, 9, true, 12f, 12f)
                .WithShine(C(200, 250, 255), 12, 0, 1.2f, 2f),
            S("Frozen sparkle", "FROZEN", Cinzel).In("Layered & animated").OnStage(C(20, 45, 85)).Spaced(2)
                .WithGlow(C(80, 180, 255), 18, 0.9f).Animated(breathe: 0.04f)
                .WithOutline(C(10, 40, 100), 4)
                .WithOutline(Color.White, 1.5f)
                .WithFill(Color.White, C(160, 225, 255), C(50, 140, 230))
                .WithGloss(0.5f, 0.45f)
                .WithInnerGlow(C(220, 245, 255), 3, 0.8f)
                .WithSparkles(Color.White, 16, 16, 1f, 4),
            S("Cosmos", "COSMOS", Audiowide).In("Layered & animated").OnStage(C(5, 5, 20)).Spaced(3)
                .WithGlow(C(150, 80, 255), 18, 1f).Animated(breathe: 0.05f)
                .WithOutline(C(30, 10, 60), 3)
                .WithFill(C(60, 40, 140), C(15, 10, 50))
                .WithPattern(ShadingPattern.Blotches, C(255, 100, 220), 0.4f, 24, true, 4f, 0f)
                .WithInnerGlow(C(150, 200, 255), 2.5f, 0.9f)
                .WithSparkles(Color.White, 24, 12, 0.8f, 9),
            S("Arcane runes", "Arcane", Fraktur).In("Layered & animated").OnStage(C(15, 5, 30))
                .WithGlow(Color.White, 18, 1f).Animated(breathe: 0.05f, cycle: ShadingPresets.MagicColors)
                .WithOutline(C(30, 0, 60), 3)
                .WithFill(C(230, 190, 255), C(130, 50, 220))
                .WithPattern(ShadingPattern.Scales, C(255, 220, 255), 0.35f, 10, true, 0f, -6f)
                .WithSparkles(C(255, 220, 255), 10, 18, 0.7f, 2),
            S("Fire stripes", "BLAZE", Bangers).In("Layered & animated").OnStage(C(25, 8, 4)).Spaced(2)
                .WithGlow(C(255, 80, 10), 18, 1.2f).Animated(flicker: 0.45f)
                .WithOutline(C(80, 10, 0), 3.5f)
                .WithFill(C(255, 240, 120), C(255, 120, 0), C(255, 240, 120), 0.5f, 0f, -90f).Scrolling(0.3f)
                .WithStripes(C(200, 30, 0), 0.4f, 120f, 0.2f, 0.6f, false, 0.08f)
                .WithGloss(0.35f, 0.4f),
            S("Electric", "VOLT", BlackOps).In("Layered & animated").OnStage(C(10, 10, 30)).Spaced(3)
                .WithGlow(C(80, 220, 255), 16, 1.1f).Animated(flicker: 0.5f)
                .WithOutline(C(20, 20, 50), 3)
                .WithFill(C(255, 255, 160), C(255, 210, 0))
                .WithStripes(Color.White, 0.6f, 60f, 0.15f, 1.5f, true, 0.06f)
                .WithInnerGlow(C(150, 240, 255), 2, 1f, 2f),
            S("Ocean waves", "OCEAN", Luckiest).In("Layered & animated").OnStage(C(250, 235, 190))
                .WithShadow(C(0, 40, 90), 0.5f, 0, 5, 3, 5)
                .WithOutline(C(0, 40, 90), 5)
                .WithFill(C(150, 230, 255), C(30, 120, 220))
                .WithStripes(Color.White, 0.3f, 90f, 0.25f, 0.4f, true, 0.12f)
                .WithGloss(0.45f, 0.4f),
            S("Treasure", "TREASURE", Rye).In("Layered & animated").OnStage(C(40, 25, 15))
                .WithShadow(Color.Black, 0.6f, 0, 5, 3, 3)
                .WithGlow(C(255, 180, 40), 16, 0.9f).Animated(0.6f, 0.4f)
                .WithOutline(C(60, 25, 0), 3.5f)
                .WithFill(C(255, 245, 190), C(245, 185, 50), C(170, 100, 15))
                .WithPattern(ShadingPattern.Noise, C(90, 40, 0), 0.3f, 5)
                .WithSparkles(Color.White, 14, 20, 0.9f, 11)
                .WithShine(C(255, 250, 220), 28, 25, 0.9f, 3f),
            S("Rainbow party", "PARTY!", Luckiest).In("Layered & animated").OnStage(C(30, 25, 50))
                .WithShadow(Color.Black, 0.5f, 0, 6, 3, 7)
                .WithOutline(C(30, 20, 60), 7)
                .WithOutline(Color.White, 3.5f)
                .WithStops(0f, 0.5f, C(255, 70, 70), C(255, 180, 40), C(250, 240, 70), C(70, 230, 110), C(60, 170, 255), C(190, 90, 255), C(255, 70, 70)).Scrolling(0.2f)
                .WithGloss(0.6f, 0.45f),
            S("Toxic bubbles", "TOXIC", Creepster).In("Layered & animated").OnStage(C(10, 20, 10)).Spaced(2)
                .WithGlow(C(120, 255, 40), 16, 1.1f).Animated(0.8f, 0.5f)
                .WithOutline(C(10, 30, 0), 2.5f)
                .WithFill(C(210, 255, 120), C(50, 170, 20))
                .WithPattern(ShadingPattern.Dots, C(230, 255, 180), 0.45f, 9, true, 0f, -14f)
                .WithInnerGlow(C(230, 255, 160), 3, 0.9f, 1.2f),
            S("Royal shine", "ROYAL", Cinzel).In("Layered & animated").OnStage(C(25, 10, 45)).Spaced(3)
                .WithShadow(Color.Black, 0.6f, 0, 4, 3, 4)
                .WithOutline(C(40, 0, 60), 5)
                .WithOutline(C(255, 215, 100), 2)
                .WithFill(C(230, 180, 255), C(130, 50, 220), C(70, 15, 140))
                .WithGloss(0.45f, 0.42f)
                .WithSparkles(C(255, 230, 160), 10, 18, 0.7f, 6)
                .WithShine(C(255, 240, 200), 26, 25, 0.9f, 3f),
            S("Disco", "DISCO", Bungee).In("Layered & animated").OnStage(C(15, 5, 25))
                .WithGlow(Color.White, 16, 1f).Animated(cycle: ShadingPresets.RainbowColors)
                .WithOutline(C(20, 10, 40), 3.5f)
                .WithFill(C(255, 120, 230), C(120, 80, 255))
                .WithPattern(ShadingPattern.Checker, Color.White, 0.3f, 10, true, 10f, 5f)
                .WithGloss(0.4f, 0.45f),
            S("Legendary loot", "LEGENDARY", Cinzel).In("Layered & animated").OnStage(C(35, 20, 10)).Spaced(1)
                .WithShadow(Color.Black, 0.6f, 0, 4, 3, 3)
                .WithGlow(C(255, 140, 0), 18, 1.1f).Animated(0.8f, 0.4f)
                .WithOutline(C(70, 25, 0), 3.5f)
                .WithStops(90f, 0.35f, C(255, 245, 190), C(255, 170, 30), C(190, 80, 0), C(255, 200, 80))
                .WithPattern(ShadingPattern.Noise, C(100, 40, 0), 0.25f, 5)
                .WithSparkles(Color.White, 12, 18, 0.9f, 8)
                .WithShine(C(255, 250, 220), 30, 25, 0.9f, 2.8f),
            S("Fairy dust", "Fairy", Pacifico).In("Layered & animated").OnStage(C(40, 20, 60))
                .WithGlow(C(255, 140, 230), 18, 1f).Animated(breathe: 0.05f)
                .WithOutline(C(90, 20, 90), 2.5f)
                .WithFill(C(255, 230, 250), C(250, 140, 220))
                .WithGloss(0.4f, 0.45f, null, 0.08f)
                .WithSparkles(Color.White, 16, 16, 1.4f, 13)
        };
    }
}
