using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Controls.Effects;
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
    /// A catalog of 1250 numbered buttons in five color families of 250 buttons (purple, blue, green, red and gold),
    /// each with its own combination of button effects (auras, frames, inner lights, sparkles, particles and animated
    /// lights) and a text with a shadow and a glow. The buttons are created from their number, so the same number
    /// always gives the same button. Only the rows near the visible area of the scroll viewer exist, so scrolling
    /// through the whole catalog stays light.
    /// </summary>
    public class ButtonCatalogScreen : Screen
    {
        private const int Columns = 5;
        private const int CellWidth = 232;
        private const int CellHeight = 124;
        private const int ViewerTop = 140;
        private const int ViewerHeight = 800 - ViewerTop - 6;
        private const int ExtraRows = 1;

        private static readonly Color StageColor = new Color(12, 14, 28);
        private static readonly Color IdColor = new Color(255, 225, 120);
        private static readonly Color FavoriteColor = new Color(110, 255, 140);
        private static readonly Color JumpBorderColor = new Color(110, 110, 130);

        private readonly Dictionary<int, Panel> rows = new Dictionary<int, Panel>();
        private readonly Dictionary<int, Label> idLabels = new Dictionary<int, Label>();
        private static readonly SortedSet<int> favorites = new SortedSet<int>();
        private Button[] jumpButtons;
        private int currentFamily = -1;
        private ScrollViewer viewer;
        private MultiLineLabel detailsLabel;
        private Label favoritesLabel;
        private Label rangeLabel;

        public static void OpenButtonCatalogScreen() => ServiceProvider.ScreenManager.ChangeScreen(new ButtonCatalogScreen());

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ($"{ButtonCatalog.Count} Buttons", OpenButtonCatalogScreen)
            });

            var font = ContentHandler.Instance.Font;
            var screenWidth = ServiceProvider.ScreenManager.ScreenSize.X;

            detailsLabel = new MultiLineLabel(font, "Click a button to see its recipe and mark it as a favorite (click again to unmark).",
                    new Vector2(Config.ScreenContentMargin, 54), Color.White, screenWidth - Config.ScreenContentMargin * 2)
                .SetScale(0.5f)
                .AddToScreen();
            favoritesLabel = new Label(font, string.Empty, new Vector2(Config.ScreenContentMargin, 90), FavoriteColor)
                .SetScale(0.5f)
                .AddToScreen();

            // Buttons that jump to the first button of each color family.
            new Label(font, "Go to:", new Vector2(Config.ScreenContentMargin, 114), Color.Gray)
                .SetScale(0.5f)
                .AddToScreen();
            jumpButtons = new Button[ButtonCatalog.Families.Length];
            for (var i = 0; i < ButtonCatalog.Families.Length; i++)
            {
                var family = ButtonCatalog.Families[i];
                var first = i * ButtonCatalog.FamilySize + 1;
                var last = first + ButtonCatalog.FamilySize - 1;
                var jump = new Button(new Vector2(80 + i * 128, 110), new Vector2(120, 26), family.Swatch)
                    .SetCornerRadius(13)
                    .SetBorder(JumpBorderColor, 1.5f)
                    .SetText(font, $"{family.Name}  #{first}-{last}", Color.White)
                    .AddEffects(new ButtonEffect[] { new HighlightEffect().SetInset(8, 3).SetHeightRate(0.42f).SetIntensity(0.3f) })
                    .AddOnClick(args => viewer.ScrollTo(new Vector2(0, -((first - 1) / Columns) * CellHeight), 0.4f))
                    .AddToScreen();
                jump.TextLabel.SetScale(0.46f);
                jumpButtons[i] = jump;
            }

            rangeLabel = new Label(font, string.Empty, new Vector2(screenWidth - 360, 114), Color.Gray)
                .SetScale(0.5f)
                .AddToScreen();
            new FpsCounter(font, new Vector2(screenWidth - 100, 114), Color.Yellow)
                .SetScale(0.5f)
                .AddToScreen();

            new RectangleControl(new Rectangle(0, ViewerTop, screenWidth, ViewerHeight), StageColor)
                .AddToScreen();
            viewer = new ScrollViewer(new Vector2(0, ViewerTop), new Vector2(screenWidth, ViewerHeight))
                .SetHideOverflow(true)
                .SetHorizontalScrollEnabled(false)
                .AddToScreen();

            // The rows are created while they are near the visible area: an empty control at the end gives the content its height.
            new RectangleControl(new Rectangle(0, RowCount * CellHeight, 1, 16), Color.Transparent)
                .AddToScreen(viewer.ContentPanel);

            RefreshFavorites();
            base.OnInit();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            RefreshVisibleRows();
        }

        private static int RowCount => (ButtonCatalog.Count + Columns - 1) / Columns;

        /// <summary>Creates the rows that entered the visible area and disposes the rows that left it.</summary>
        private void RefreshVisibleRows()
        {
            var top = -viewer.ScrollPosition.Y;
            var first = Math.Max(0, (int)Math.Floor(top / CellHeight) - ExtraRows);
            var last = Math.Min(RowCount - 1, (int)Math.Floor((top + ViewerHeight) / CellHeight) + ExtraRows);

            foreach (var index in rows.Keys.Where(index => index < first || index > last).ToList())
            {
                for (var column = 0; column < Columns; column++)
                    idLabels.Remove(index * Columns + column + 1);
                rows[index].Dispose();
                rows.Remove(index);
            }

            for (var index = first; index <= last; index++)
            {
                if (!rows.ContainsKey(index))
                    rows[index] = CreateRow(index);
            }

            var firstId = Math.Max(1, (int)Math.Floor(top / CellHeight) * Columns + 1);
            var lastId = Math.Min(ButtonCatalog.Count, (int)Math.Ceiling((top + ViewerHeight) / CellHeight) * Columns);

            // The family in the middle of the visible area is the current one.
            var middleId = Math.Min(ButtonCatalog.Count, (int)Math.Floor((top + ViewerHeight / 2f) / CellHeight) * Columns + 1);
            var family = (middleId - 1) / ButtonCatalog.FamilySize;
            if (family != currentFamily)
            {
                currentFamily = family;
                for (var i = 0; i < jumpButtons.Length; i++)
                    jumpButtons[i].SetBorder(i == family ? Color.White : JumpBorderColor, i == family ? 3f : 1.5f);
            }

            var range = $"{ButtonCatalog.Families[family].Name}: showing #{firstId} - #{lastId} of {ButtonCatalog.Count}";
            if (rangeLabel.Text != range)
                rangeLabel.Text = range;
        }

        private Panel CreateRow(int rowIndex)
        {
            var width = ServiceProvider.ScreenManager.ScreenSize.X;
            var row = new Panel(new Rectangle(0, rowIndex * CellHeight, width, CellHeight))
                .AddToScreen(viewer.ContentPanel);

            var left = (width - Columns * CellWidth) / 2f;
            for (var column = 0; column < Columns; column++)
            {
                var id = rowIndex * Columns + column + 1;
                if (id > ButtonCatalog.Count)
                    break;
                CreateCell(row, ButtonCatalog.Get(id), new Vector2(left + column * CellWidth, 0));
            }

            return row;
        }

        private void CreateCell(Panel row, ButtonCatalog.Spec spec, Vector2 cellPosition)
        {
            var font = ContentHandler.Instance.Font;

            var idLabel = new Label(font, IdText(spec.Id), cellPosition + new Vector2(8, 4), IdColor)
                .SetScale(0.55f)
                .AddToScreen(row);
            idLabels[spec.Id] = idLabel;
            RefreshIdLabel(spec.Id);

            var radius = spec.Radius < 0f ? spec.Size.Y / 2f : spec.Radius;
            var button = new Button(Vector2.Zero, spec.Size, spec.Background)
                .SetCornerRadius(radius)
                .SetText(font, spec.Text, spec.TextColor)
                .AddEffects(spec.CreateEffects())
                .AddOnClick(args => SelectButton(spec))
                .AddToScreen(row);
            button.TextLabel.SetScale(FitText(spec.Text, spec.Size, radius))
                .AddShadings(spec.CreateTextShadings());
            button.SetOriginRate(new Vector2(0.5f))
                .SetPosition(cellPosition + new Vector2(CellWidth / 2f, CellHeight / 2f + 10f));
            CenterText(button);
        }

        private void SelectButton(ButtonCatalog.Spec spec)
        {
            if (!favorites.Remove(spec.Id))
                favorites.Add(spec.Id);

            var mark = favorites.Contains(spec.Id) ? "[favorite]" : "[removed from favorites]";
            detailsLabel.Text = $"#{spec.Id} {mark}  {spec.Description}";
            RefreshIdLabel(spec.Id);
            RefreshFavorites();
        }

        private void RefreshIdLabel(int id)
        {
            if (!idLabels.TryGetValue(id, out var label))
                return;
            var isFavorite = favorites.Contains(id);
            label.Text = isFavorite ? IdText(id) + "  (favorite)" : IdText(id);
            label.Color = isFavorite ? FavoriteColor : IdColor;
        }

        private void RefreshFavorites()
        {
            favoritesLabel.Text = favorites.Count == 0
                ? "Favorites: none yet"
                : $"Favorites ({favorites.Count}): " + string.Join(", ", favorites.Select(id => "#" + id));
        }

        private static string IdText(int id) => "#" + id.ToString("0000");

        /// <summary>The scale of a text that fits a button: from its height, and never wider than the button.</summary>
        private static float FitText(string text, Vector2 size, float radius)
        {
            var byHeight = MathHelper.Clamp(size.Y / 64f, 0.42f, 0.85f);
            var textWidth = ContentHandler.Instance.Font.MeasureString(text).X;
            var room = size.X - 2f * Math.Max(12f, Math.Min(radius, size.Y / 2f) * 0.6f);
            return Math.Max(0.3f, Math.Min(byHeight, room / textWidth));
        }

        /// <summary>
        /// The children of a control are placed from its position, not from its origin: the text of a button centered
        /// with an origin is moved back by the origin.
        /// </summary>
        private static void CenterText<TButton>(ButtonAbstract<TButton> button) where TButton : ButtonAbstract<TButton>
            => button.TextLabel?.SetPosition(-button.OriginWithoutScale);
    }

    /// <summary>
    /// Builds the buttons of the catalog from their number. Every block of 250 buttons is a color family: it starts
    /// with the presets painted with the colors of the family, then combinations of an aura, a border, a body light,
    /// static lights, animated lights and a text style, picked by a random generator seeded with the number. Two
    /// numbers never give the same combination.
    /// </summary>
    internal static class ButtonCatalog
    {
        public const int FamilySize = 250;

        private static readonly Dictionary<int, Spec> specs = new Dictionary<int, Spec>();
        private static bool isBuilt;

        /// <summary>A button of the catalog.</summary>
        public sealed class Spec
        {
            public int Id;
            public string Text;
            public Vector2 Size;

            /// <summary>The corner radius (a negative value = a pill).</summary>
            public float Radius;
            public Color Background;
            public Color Accent;
            public Color TextColor;
            public string Description;
            public Func<List<ButtonEffect>> CreateEffects;
            public Func<List<ShadingEffect>> CreateTextShadings;
        }

        /// <summary>A color family: the background colors of its buttons, the colors of their lights and a color cycle.</summary>
        public sealed class Family
        {
            public Family(char code, string name, Color swatch, Color dark, Color[] backgrounds, Color[] lights, Color[] cycle, string[] coloredPresets)
            {
                Code = code;
                Name = name;
                Swatch = swatch;
                Dark = dark;
                Backgrounds = backgrounds;
                Lights = lights;
                Cycle = cycle;
                ColoredPresets = coloredPresets;
            }

            public char Code { get; }
            public string Name { get; }

            /// <summary>The color of the Go to button of the family.</summary>
            public Color Swatch { get; }

            /// <summary>A very dark shade, for text outlines.</summary>
            public Color Dark { get; }
            public Color[] Backgrounds { get; }
            public Color[] Lights { get; }
            public Color[] Cycle { get; }

            /// <summary>The presets with lights of their own colors that suit the family (the other presets use the color of the button).</summary>
            public string[] ColoredPresets { get; }
        }

        private delegate IEnumerable<ButtonEffect> Part(Family family, Color background, Color accent, int seed);

        private delegate IEnumerable<ShadingEffect> TextPart(Family family, Color accent);

        private sealed class Option<T>
        {
            public Option(string name, T create, int weight = 1, string families = null)
            {
                Name = name;
                Create = create;
                Weight = weight;
                Families = families;
            }

            public string Name { get; }
            public T Create { get; }
            public int Weight { get; }

            /// <summary>The codes of the families that use the option (null = all of them).</summary>
            public string Families { get; }

            public bool Suits(Family family) => Families == null || Families.IndexOf(family.Code) >= 0;
        }

        // ---- Families

        public static readonly Family[] Families =
        {
            new Family('P', "Purple", new Color(130, 60, 200), new Color(35, 5, 60),
                new[]
                {
                    new Color(60, 25, 125), new Color(105, 35, 200), new Color(150, 80, 210), new Color(90, 20, 110), new Color(130, 60, 190),
                    new Color(180, 110, 240), new Color(45, 15, 80), new Color(200, 40, 200), new Color(120, 40, 160), new Color(75, 40, 140)
                },
                new[]
                {
                    new Color(190, 110, 255), new Color(255, 90, 220), new Color(220, 160, 255), new Color(150, 90, 255),
                    new Color(255, 140, 240), Color.White, new Color(255, 200, 70), new Color(60, 220, 255)
                },
                new[] { new Color(190, 80, 255), new Color(255, 90, 210), new Color(120, 90, 255) },
                new[] { "Galaxy", "Magic", "Neon", "Rainbow", "Subtle" }),
            new Family('B', "Blue", new Color(40, 120, 230), new Color(0, 20, 60),
                new[]
                {
                    new Color(20, 60, 150), new Color(40, 130, 230), new Color(40, 180, 220), new Color(25, 35, 95), new Color(20, 80, 120),
                    new Color(60, 110, 255), new Color(15, 40, 90), new Color(80, 160, 230), new Color(30, 90, 210), new Color(10, 25, 60)
                },
                new[]
                {
                    new Color(60, 220, 255), new Color(90, 150, 255), new Color(170, 240, 255), new Color(120, 200, 255),
                    Color.White, new Color(255, 200, 70), new Color(80, 255, 230), new Color(200, 220, 255)
                },
                new[] { new Color(40, 140, 255), new Color(60, 230, 255), new Color(150, 200, 255) },
                new[] { "Galaxy", "Royal", "Ice", "Glass", "Subtle" }),
            new Family('G', "Green", new Color(40, 160, 70), new Color(0, 35, 10),
                new[]
                {
                    new Color(15, 90, 50), new Color(40, 140, 70), new Color(110, 200, 40), new Color(30, 165, 120), new Color(20, 60, 35),
                    new Color(60, 180, 90), new Color(140, 210, 60), new Color(25, 120, 100), new Color(10, 50, 30), new Color(80, 160, 40)
                },
                new[]
                {
                    new Color(70, 240, 140), new Color(200, 255, 120), new Color(140, 255, 190), new Color(120, 255, 80),
                    Color.White, new Color(255, 220, 90), new Color(60, 255, 200), new Color(230, 255, 170)
                },
                new[] { new Color(40, 220, 120), new Color(170, 255, 80), new Color(60, 255, 200) },
                new[] { "Emerald", "Glass", "Subtle" }),
            new Family('R', "Red", new Color(200, 35, 50), new Color(50, 0, 5),
                new[]
                {
                    new Color(170, 20, 40), new Color(225, 50, 60), new Color(150, 20, 20), new Color(255, 70, 70), new Color(120, 10, 30),
                    new Color(200, 40, 90), new Color(235, 90, 60), new Color(90, 10, 20), new Color(210, 30, 30), new Color(255, 100, 110)
                },
                new[]
                {
                    new Color(255, 70, 70), new Color(255, 130, 40), new Color(255, 200, 70), new Color(255, 90, 150),
                    Color.White, new Color(255, 170, 120), new Color(255, 40, 90), new Color(255, 230, 150)
                },
                new[] { new Color(255, 40, 40), new Color(255, 120, 30), new Color(255, 60, 140) },
                new[] { "Fire", "Alert", "Subtle" }),
            new Family('Y', "Gold", new Color(235, 170, 30), new Color(60, 30, 0),
                new[]
                {
                    new Color(255, 180, 20), new Color(215, 125, 20), new Color(150, 90, 20), new Color(120, 60, 10), new Color(230, 170, 40),
                    new Color(255, 200, 60), new Color(180, 120, 20), new Color(200, 150, 30), new Color(90, 55, 10), new Color(245, 160, 40)
                },
                new[]
                {
                    new Color(255, 200, 70), new Color(255, 235, 150), new Color(255, 170, 40), new Color(255, 130, 40),
                    Color.White, new Color(255, 215, 120), new Color(255, 250, 200), new Color(255, 150, 60)
                },
                new[] { new Color(255, 170, 30), new Color(255, 230, 120), new Color(255, 140, 40) },
                new[] { "Gold", "Legendary", "Subtle" })
        };

        /// <summary>The number of buttons of the catalog.</summary>
        public static int Count => Families.Length * FamilySize;

        private static readonly string[] Words =
        {
            "PLAY", "START", "BATTLE", "SHOP", "BUY", "CLAIM", "COLLECT", "UPGRADE", "LEVEL UP", "CONTINUE", "RETRY",
            "NEXT", "REWARD", "SPIN", "OPEN", "QUEST", "VICTORY", "BONUS", "GEMS", "FREE", "VIP", "JOIN", "FIGHT",
            "CRAFT", "SUMMON", "ENTER", "READY", "BOOST", "EPIC", "LEGEND", "MAGIC", "RAID", "ARENA", "HERO", "DAILY",
            "EVENT", "PREMIUM", "POWER", "LUCKY", "GO!", "Play", "Start", "Shop", "Claim", "Next", "Continue", "Spin",
            "Explore", "Unlock", "Treasure", "Rewards", "Settings", "Profile", "Missions"
        };

        private static readonly string[] ShortWords = { "GO", "+", "!", "?", "$", "x2", "x5", "VIP", "1", "OK", "S", "UP" };

        /// <summary>The presets that use the color of the button, with the corner radius that suits them (a negative radius = a pill).</summary>
        private static readonly (string name, float radius)[] StaticPresets =
        {
            ("Ornate", 20), ("Starlight", 16), ("Treasure", 14), ("Crystal", 12), ("Fairy", -1), ("Sunburst", 14),
            ("Candy", -1), ("Jelly", -1), ("Glossy", 16), ("Cartoon", 14), ("Bubble", -1), ("Pearl", -1), ("Soft Glow", -1),
            ("Inner Light", 12), ("Framed", 10), ("Minimal", 8)
        };

        /// <summary>The corner radius of the presets with lights of their own colors.</summary>
        private static readonly Dictionary<string, float> ColoredRadii = new Dictionary<string, float>
        {
            { "Galaxy", 18 }, { "Gold", 16 }, { "Royal", 10 }, { "Magic", 18 }, { "Ice", 12 }, { "Fire", 14 }, { "Emerald", 20 }, { "Neon", 22 },
            { "Rainbow", 16 }, { "Legendary", 10 }, { "Glass", 12 }, { "Alert", 8 }, { "Subtle", 8 }
        };

        // ---- Parts of the combinations

        private static readonly Option<Part>[] Auras =
        {
            new Option<Part>("No aura", (f, b, a, s) => None),
            new Option<Part>("Soft aura", (f, b, a, s) => One(new GlowEffect().UseButtonColor(-0.4f).SetRadius(16).SetIntensity(0.5f).IgnoreStates()), 2),
            new Option<Part>("Light aura", (f, b, a, s) => One(new GlowEffect().SetColor(a).SetRadius(18).SetIntensity(0.55f).IgnoreStates()), 3),
            new Option<Part>("Pulsing aura", (f, b, a, s) => One(new GlowEffect().SetColor(a).SetRadius(18).SetPulse(1.2f, 0.45f)), 2),
            new Option<Part>("Big halo", (f, b, a, s) => One(new GlowEffect().SetColor(a).SetRadius(28).SetFalloff(1.6f).SetIntensity(0.6f).IgnoreStates()), 2),
            new Option<Part>("Double aura", (f, b, a, s) => new ButtonEffect[]
            {
                new GlowEffect().SetColor(a).SetRadius(30).SetFalloff(1.4f).SetIntensity(0.35f).IgnoreStates(),
                new GlowEffect().SetColor(Color.Lerp(a, Color.White, 0.5f)).SetRadius(8).SetFalloff(2.5f).SetIntensity(0.7f)
            }, 2),
            new Option<Part>("Color cycle aura", (f, b, a, s) => One(new GlowEffect().SetColorCycle(f.Cycle, 4f).SetRadius(16).SetIntensity(0.6f))),
            new Option<Part>("Neon aura", (f, b, a, s) => One(new GlowEffect().SetColorCycle(f.Cycle, 3f).SetRadius(16).SetFalloff(1.6f).SetPulse(3f, 0.15f))),
            new Option<Part>("Still rays", (f, b, a, s) => new ButtonEffect[]
            {
                new LightRaysEffect().SetColor(a).SetRays(10 + s % 6, 0, 14).SetRotationSpeed(0).SetFlicker(0).SetIntensity(0.35f).IgnoreStates(),
                new GlowEffect().SetColor(a).SetRadius(18).SetIntensity(0.45f).IgnoreStates()
            }, 2),
            new Option<Part>("Turning rays", (f, b, a, s) => new ButtonEffect[]
            {
                new LightRaysEffect().SetColor(a).SetRays(12, 0, 16).SetRotationSpeed(s % 2 == 0 ? 14 : -14),
                new GlowEffect().SetColor(a).SetRadius(20).SetPulse(1f, 0.35f)
            }, 2),
            new Option<Part>("Drop shadow", (f, b, a, s) => One(new DropShadowEffect().SetOffset(new Vector2(0, 6)).SetBlur(10).SetIntensity(0.35f))),
            new Option<Part>("Shadow and aura", (f, b, a, s) => new ButtonEffect[]
            {
                new DropShadowEffect().SetOffset(new Vector2(0, 5)).SetBlur(8).SetIntensity(0.3f),
                new GlowEffect().SetColor(a).SetRadius(14).SetIntensity(0.4f).IgnoreStates()
            }, 2)
        };

        private static readonly Option<Part>[] Borders =
        {
            new Option<Part>("No border", (f, b, a, s) => None),
            new Option<Part>("Gold frame", (f, b, a, s) => One(FrameEffect.Gold()), 3),
            new Option<Part>("Silver frame", (f, b, a, s) => One(FrameEffect.Silver()), 2),
            new Option<Part>("Bronze frame", (f, b, a, s) => One(FrameEffect.Bronze())),
            new Option<Part>("Gem frame", (f, b, a, s) => One(FrameEffect.Gem(a)), 3),
            new Option<Part>("Own color frame", (f, b, a, s) => One(FrameEffect.ButtonColor()), 2),
            new Option<Part>("Dark frame", (f, b, a, s) => One(FrameEffect.Dark())),
            new Option<Part>("Candy frame", (f, b, a, s) => new ButtonEffect[] { new DepthEffect().SetDepth(5).UseButtonColor(0.4f), FrameEffect.Candy() }),
            new Option<Part>("White outline", (f, b, a, s) => One(new OutlineEffect().SetColor(Color.White * 0.85f).SetThickness(2))),
            new Option<Part>("Light outline", (f, b, a, s) => One(new OutlineEffect().SetColor(a).SetThickness(2.5f))),
            new Option<Part>("Rim light", (f, b, a, s) => One(new GlowEffect(GlowPlacement.Rim).SetColor(Color.Lerp(a, Color.White, 0.4f)).SetThickness(2).SetSoftness(2).SetOffset(-2).SetIntensity(0.8f)), 3),
            new Option<Part>("Color cycle rim", (f, b, a, s) => One(new GlowEffect(GlowPlacement.Rim).SetColorCycle(f.Cycle, 3f).SetThickness(2).SetSoftness(2).SetOffset(-1))),
            new Option<Part>("Cartoon lip", (f, b, a, s) => new ButtonEffect[]
            {
                new DepthEffect().SetDepth(6).UseButtonColor(0.55f),
                new OutlineEffect().UseButtonColor(0.72f).SetThickness(3)
            }, 2),
            new Option<Part>("Inner line", (f, b, a, s) => One(new OutlineEffect(OutlinePosition.Inside).SetColor(Color.White * 0.55f).SetOffset(3).SetThickness(1.5f))),
            new Option<Part>("Rim and outline", (f, b, a, s) => new ButtonEffect[]
            {
                new OutlineEffect().UseButtonColor(0.6f).SetThickness(2),
                new GlowEffect(GlowPlacement.Rim).SetColor(a).SetThickness(1.5f).SetSoftness(1.5f).SetOffset(-2).SetIntensity(0.7f)
            }, 2)
        };

        private static readonly Option<Part>[] Bodies =
        {
            new Option<Part>("Band highlight", (f, b, a, s) => new ButtonEffect[]
            {
                new InnerShadeEffect().SetHeightRate(0.5f).SetIntensity(0.15f),
                new HighlightEffect().SetInset(10, 4).SetHeightRate(0.4f).SetFade(0.7f).SetIntensity(0.35f)
            }, 3),
            new Option<Part>("Strip highlight", (f, b, a, s) => new ButtonEffect[]
            {
                new InnerShadeEffect().SetHeightRate(0.4f).SetIntensity(0.15f),
                new HighlightEffect(HighlightStyle.Strip).SetInset(12, 5).SetThickness(3).SetIntensity(0.4f)
            }, 2),
            new Option<Part>("Bubble highlight", (f, b, a, s) => new ButtonEffect[]
            {
                new InnerShadeEffect(ShadePlacement.Edges).SetRadius(12).SetIntensity(0.2f),
                new HighlightEffect(HighlightStyle.Ellipse).SetInset(12, 3).SetHeightRate(0.38f).SetIntensity(0.45f)
            }, 2),
            new Option<Part>("Gloss", (f, b, a, s) => One(new GlossEffect().SetHeightRate(0.5f).SetIntensity(0.4f)), 2),
            new Option<Part>("Inner glow", (f, b, a, s) => One(new GlowEffect(GlowPlacement.Inner).SetColor(a).SetRadius(12).SetIntensity(0.6f)), 3),
            new Option<Part>("Glow from below", (f, b, a, s) => new ButtonEffect[]
            {
                new InnerShadeEffect().SetHeightRate(0.55f).SetIntensity(0.25f),
                new GlowEffect(GlowPlacement.Inner).SetEdges(GlowEdges.Bottom).SetColor(a).SetRadius(18).SetIntensity(0.55f),
                new HighlightEffect().SetInset(12, 4).SetHeightRate(0.38f).SetFade(0.75f).SetIntensity(0.3f)
            }, 3),
            new Option<Part>("Light from above", (f, b, a, s) => new ButtonEffect[]
            {
                new InnerShadeEffect().SetHeightRate(0.45f).SetIntensity(0.2f),
                new GlowEffect(GlowPlacement.Inner).SetEdges(GlowEdges.Top).SetColor(Color.Lerp(a, Color.White, 0.5f)).SetRadius(26).SetFalloff(1.6f).SetIntensity(0.5f)
            }, 2),
            new Option<Part>("Vignette", (f, b, a, s) => new ButtonEffect[]
            {
                new InnerShadeEffect(ShadePlacement.Edges).SetRadius(16).SetFalloff(1.6f).SetIntensity(0.45f),
                new GlowEffect(GlowPlacement.Inner).UseButtonColor(-0.6f).SetRadius(9).SetFalloff(2.4f).SetIntensity(0.6f),
                new HighlightEffect().SetInset(12, 4).SetHeightRate(0.38f).SetFade(0.75f).SetIntensity(0.22f)
            }, 2),
            new Option<Part>("Pulsing fill", (f, b, a, s) => new ButtonEffect[]
            {
                new GlowEffect(GlowPlacement.Inner).SetColor(a).SetRadius(14).SetIntensity(0.5f),
                new LightFillEffect().SetColor(a).SetIntensity(0.15f).SetPulse(2f, 0.8f)
            }),
            new Option<Part>("Jelly", (f, b, a, s) => new ButtonEffect[]
            {
                new InnerShadeEffect().SetHeightRate(0.6f).SetIntensity(0.28f),
                new GlowEffect(GlowPlacement.Inner).SetEdges(GlowEdges.Bottom).UseButtonColor(-0.55f).SetRadius(18).SetIntensity(0.5f),
                new HighlightEffect().SetInset(12, 4).SetHeightRate(0.4f).SetFade(0.75f).SetIntensity(0.45f)
            }, 2),
            new Option<Part>("Glass", (f, b, a, s) => new ButtonEffect[]
            {
                new InnerShadeEffect().SetHeightRate(0.5f).SetIntensity(0.18f),
                new HighlightEffect().SetInset(3, 2).SetHeightRate(0.48f).SetSoftness(0.03f).SetFade(0.3f).SetIntensity(0.32f),
                new GlossEffect().SetFromBottom(true).SetHeightRate(0.3f).SetIntensity(0.2f)
            }, 2)
        };

        private static readonly Option<Part>[] Lights =
        {
            new Option<Part>("No lights", (f, b, a, s) => None),
            new Option<Part>("Glints", (f, b, a, s) => One(new GlintEffect().SetCorners(s % 2 == 0 ? Anchor.TopLeft : Anchor.TopRight).SetDots(2, 6))),
            new Option<Part>("Corner lights", (f, b, a, s) => One(new CornerLightEffect().SetAnchors(Anchor.TopLeft, Anchor.TopRight).SetColor(a).SetSize(12, 26).SetTwinkle(0f, 0f).SetOffset(-4)), 2),
            new Option<Part>("Gem with streak", (f, b, a, s) => One(new CornerLightEffect().SetAnchors(Anchor.TopCenter).SetShape(LightShape.Diamond).SetColor(a).SetSize(12, 34).SetStreak(110, 3).SetTwinkle(0f, 0f)), 3),
            new Option<Part>("Five gems", (f, b, a, s) => One(new CornerLightEffect().SetAnchors(Anchor.TopCenter, Anchor.TopLeft, Anchor.TopRight, Anchor.BottomLeft, Anchor.BottomRight)
                .SetShape(LightShape.Diamond).SetColor(a).SetSize(9, 24).SetTwinkle(0.8f, 0.3f))),
            new Option<Part>("Glowing particles below", (f, b, a, s) => One(new ParticleFieldEffect(ParticleFieldArea.Bottom).SetColor(a).SetCount(14).SetSize(3, 10).SetSeed(s)), 3),
            new Option<Part>("Star field", (f, b, a, s) => One(new ParticleFieldEffect().SetShapes(LightShape.Circle, LightShape.Circle, LightShape.Flare, LightShape.Star)
                .SetColors(Color.White, Color.Lerp(a, Color.White, 0.6f)).SetCount(24).SetSize(1.5f, 7).SetIntensity(0.45f).SetSeed(s)), 2),
            new Option<Part>("Lights above", (f, b, a, s) => One(new ParticleFieldEffect(ParticleFieldArea.Above).SetColors(a, Color.White)
                .SetShapes(LightShape.Flare, LightShape.Glow, LightShape.Star).SetCount(10).SetSize(4, 9).SetSpread(20).SetSeed(s)), 2),
            new Option<Part>("Lights around", (f, b, a, s) => One(new ParticleFieldEffect(ParticleFieldArea.Around).SetColor(a).SetShapes(LightShape.Glow, LightShape.Circle, LightShape.Flare)
                .SetCount(12).SetSize(2, 6).SetSpread(14).SetIntensity(0.45f).SetSeed(s)), 2),
            new Option<Part>("Flares on the edge", (f, b, a, s) => One(new ParticleFieldEffect(ParticleFieldArea.Edge).SetColor(a).SetShapes(LightShape.Flare).SetCount(5).SetSize(7, 13).SetSpread(6).SetIntensity(0.45f).SetSeed(s))),
            new Option<Part>("Bokeh", (f, b, a, s) => One(new ParticleFieldEffect().SetColors(f.Cycle).SetShapes(LightShape.Glow)
                .SetCount(8).SetSize(12, 24).SetOpacityMin(0.1f).SetCoreRate(0f).SetIntensity(0.4f).SetSeed(s)), 2),
            new Option<Part>("Stars on the corners", (f, b, a, s) => One(new CornerLightEffect().SetAnchors(CornerLightEffect.Corners).SetShape(LightShape.Star).SetColor(a).SetSize(10, 22).SetTwinkle(0f, 0f).SetOffset(-4))),
            new Option<Part>("Frost diamonds", (f, b, a, s) => One(new ParticleFieldEffect(ParticleFieldArea.Top).SetShapes(LightShape.Diamond, LightShape.Flare, LightShape.Circle).SetCount(10).SetSize(3, 8).SetSeed(s)))
        };

        private static readonly Option<Part>[] Motions =
        {
            new Option<Part>("Still", (f, b, a, s) => None, 4),
            new Option<Part>("Shine sweep", (f, b, a, s) => One(new ShineSweepEffect().SetColor(Color.Lerp(a, Color.White, 0.7f)).SetTiming(0.7f, 2f + s % 3)), 3),
            new Option<Part>("Wide sweep", (f, b, a, s) => One(new ShineSweepEffect().SetTiming(0.6f, 2f).SetWidth(44).SetAngle(30))),
            new Option<Part>("Sparkles", (f, b, a, s) => One(new SparkleEffect().SetColor(Color.Lerp(a, Color.White, 0.4f)).SetRate(4)), 2),
            new Option<Part>("Spinning stars", (f, b, a, s) => One(new SparkleEffect().SetColor(a).SetShape(LightShape.Star).SetArea(SparkleArea.Around, 10).SetRate(8).SetSpinSpeed(120).SetClickBurst(14)), 2),
            new Option<Part>("Running light", (f, b, a, s) => One(new RunningLightEffect().SetColor(a).SetSpeed(150).SetSize(10).SetTail(80)), 2),
            new Option<Part>("Twin running lights", (f, b, a, s) => One(new RunningLightEffect().SetColor(a).SetCount(2).SetSpeed(200).SetSize(11)), 2),
            new Option<Part>("Color cycle running lights", (f, b, a, s) => One(new RunningLightEffect().SetColorCycle(f.Cycle, 2f).SetCount(2).SetSpeed(200).SetSize(10))),
            new Option<Part>("Rising embers", (f, b, a, s) => One(new SparkleEffect().SetColor(a).SetShape(LightShape.Glow).SetArea(SparkleArea.Top, 4)
                .SetRate(14).SetSize(8, 0.5f).SetLifetime(0.6f, 1.3f).SetMotion(18, new Vector2(0, -70)).SetClickBurst(16)), 2),
            new Option<Part>("Inner sparkles", (f, b, a, s) => One(new SparkleEffect().SetColor(Color.Lerp(a, Color.White, 0.5f)).SetArea(SparkleArea.Inside, 4).SetRate(5).SetSize(8))),
            new Option<Part>("Sweep and sparkles", (f, b, a, s) => new ButtonEffect[]
            {
                new ShineSweepEffect().SetTiming(0.6f, 2.5f).SetWidth(30),
                new SparkleEffect().SetColor(a).SetArea(SparkleArea.Around, 12).SetRate(8).SetClickBurst(20)
            }, 2),
            new Option<Part>("Twinkling corners", (f, b, a, s) => One(new CornerLightEffect().SetAnchors(CornerLightEffect.Corners).SetColor(a).SetSize(10, 24).SetTwinkle(1.6f, 0.6f).SetSpinSpeed(40))),
            new Option<Part>("Ripple and hover light", (f, b, a, s) => new ButtonEffect[]
            {
                new GlowEffect(GlowPlacement.Inner).SetColor(a).SetRadius(12).SetIntensity(0.5f).VisibleOnHover(),
                new ClickRippleEffect().SetColor(Color.Lerp(a, Color.White, 0.6f))
            })
        };

        /// <summary>The styles of the texts: all of them have a shadow and a glow; some only suit some families.</summary>
        private static readonly Option<TextPart>[] TextStyles =
        {
            new Option<TextPart>("soft shadow + glow", (f, a) => new ShadingEffect[]
            {
                new Shadow(new Vector2(0, 3), 4, Color.Black * 0.6f),
                new Glow(a, 10).SetIntensity(0.9f)
            }, 4),
            new Option<TextPart>("hard shadow + white glow", (f, a) => new ShadingEffect[]
            {
                new Shadow(new Vector2(2, 3), 0, Color.Black * 0.75f),
                new Glow(Color.White, 8).SetIntensity(0.6f)
            }, 2),
            new Option<TextPart>("shadow + outline + glow", (f, a) => new ShadingEffect[]
            {
                new Shadow(new Vector2(0, 3), 3, Color.Black * 0.6f).SetSpread(2),
                new Glow(a, 12).SetIntensity(1f),
                new Outline(f.Dark, 2)
            }, 3),
            new Option<TextPart>("neon", (f, a) => new ShadingEffect[]
            {
                new Shadow(new Vector2(0, 2), 3, Color.Black * 0.5f),
                new Glow(a, 16).SetIntensity(1f).SetFlicker(0.3f, 5f),
                new Glow(Color.Lerp(a, Color.White, 0.4f), 4).SetIntensity(1.1f)
            }, 2),
            new Option<TextPart>("pulsing glow", (f, a) => new ShadingEffect[]
            {
                new Shadow(new Vector2(0, 3), 4, Color.Black * 0.6f),
                new Glow(a, 14).SetIntensity(1.2f).SetPulse(1.2f, 0.5f)
            }, 2),
            new Option<TextPart>("color cycle glow", (f, a) => new ShadingEffect[]
            {
                new Shadow(new Vector2(0, 3), 4, Color.Black * 0.6f),
                new Glow(f.Cycle[0], 16).SetIntensity(1.1f).SetColorCycle(f.Cycle, 3f),
                new Glow(f.Cycle[1], 5).SetIntensity(1f).SetColorCycle(f.Cycle, 3f).SetTimeOffset(0.6f),
                new Outline(f.Dark, 1.5f)
            }, 2),
            new Option<TextPart>("long shadow + glow", (f, a) => new ShadingEffect[]
            {
                new Shadow(new Vector2(6, 6), 0, Color.Black * 0.2f),
                new Shadow(new Vector2(4, 4), 0, Color.Black * 0.25f),
                new Shadow(new Vector2(2, 2), 0, Color.Black * 0.35f),
                new Glow(a, 8).SetIntensity(0.7f)
            }),
            new Option<TextPart>("comic", (f, a) => new ShadingEffect[]
            {
                new Shadow(new Vector2(3, 4), 0, Color.Black).SetSpread(2),
                new Glow(a, 10).SetIntensity(0.8f),
                new Outline(Color.Black, 2.5f)
            }),
            new Option<TextPart>("sticker", (f, a) => new ShadingEffect[]
            {
                new Shadow(new Vector2(0, 4), 5, Color.Black * 0.5f).SetSpread(3),
                new Glow(a, 12).SetIntensity(0.8f),
                new Outline(Color.White, 2.5f)
            }),
            new Option<TextPart>("emboss + glow", (f, a) => new ShadingEffect[]
            {
                new Shadow(new Vector2(-1, -1), 1, Color.White * 0.5f),
                new Shadow(new Vector2(1.5f, 1.5f), 1.5f, Color.Black * 0.7f),
                new Glow(a, 10).SetIntensity(0.7f)
            }),
            new Option<TextPart>("shiny + glow", (f, a) => new ShadingEffect[]
            {
                new Shadow(new Vector2(0, 3), 5, Color.Black * 0.5f),
                new Glow(a, 10).SetIntensity(0.8f),
                new Shine(Color.White).SetTiming(0.7f, 2.2f).SetWidth(24).SetIntensity(0.9f)
            }, 2),
            new Option<TextPart>("retro", (f, a) => new ShadingEffect[]
            {
                new Shadow(new Vector2(3, 3), 0, new Color(60, 220, 255)),
                new Shadow(new Vector2(1.5f, 1.5f), 0, new Color(255, 60, 170)),
                new Glow(a, 10).SetIntensity(0.7f)
            }, 1, "P"),
            new Option<TextPart>("gold title", (f, a) => Prepend(ShadingPresets.GoldTitle()), 2, "PRY"),
            new Option<TextPart>("fire", (f, a) => Prepend(ShadingPresets.Fire()), 2, "RY"),
            new Option<TextPart>("heartbeat", (f, a) => ShadingPresets.Heartbeat(), 1, "R"),
            new Option<TextPart>("selected", (f, a) => Prepend(ShadingPresets.Selected()), 1, "Y"),
            new Option<TextPart>("ice", (f, a) => ShadingPresets.Ice(), 2, "B"),
            new Option<TextPart>("hologram", (f, a) => Prepend(ShadingPresets.Hologram()), 1, "B"),
            new Option<TextPart>("ghost", (f, a) => Prepend(ShadingPresets.Ghost()), 1, "BP"),
            new Option<TextPart>("magic", (f, a) => Prepend(ShadingPresets.Magic()), 2, "P"),
            new Option<TextPart>("plasma", (f, a) => Prepend(ShadingPresets.Plasma()), 1, "PB"),
            new Option<TextPart>("glitch", (f, a) => Prepend(ShadingPresets.Glitch()), 1, "PB"),
            new Option<TextPart>("toxic", (f, a) => ShadingPresets.Toxic(), 2, "G")
        };

        private static readonly ButtonEffect[] None = Array.Empty<ButtonEffect>();

        private static IEnumerable<ButtonEffect> One(ButtonEffect effect) => new[] { effect };

        /// <summary>Adds a shadow under a text preset that has none.</summary>
        private static IEnumerable<ShadingEffect> Prepend(IEnumerable<ShadingEffect> effects)
            => new ShadingEffect[] { new Shadow(new Vector2(0, 3), 4, Color.Black * 0.55f) }.Concat(effects);

        /// <summary>
        /// Dims the glows of a text style: the lights of the button are added under them, and full glows on a small
        /// text cover its letters.
        /// </summary>
        private static List<ShadingEffect> Soften(IEnumerable<ShadingEffect> effects)
        {
            var list = effects.ToList();
            foreach (var glow in list.OfType<Glow>())
                glow.Intensity *= 0.55f;
            return list;
        }

        /// <summary>The button with a number from 1 to <see cref="Count"/>.</summary>
        public static Spec Get(int id)
        {
            Build();
            return specs[id];
        }

        private static void Build()
        {
            if (isBuilt)
                return;
            isBuilt = true;

            var used = new HashSet<string>();
            for (var familyIndex = 0; familyIndex < Families.Length; familyIndex++)
            {
                var family = Families[familyIndex];
                var id = familyIndex * FamilySize + 1;
                var last = id + FamilySize - 1;

                // The presets first, painted with the colors of the family.
                var presets = StaticPresets.Concat(family.ColoredPresets.Select(name => (name, ColoredRadii[name]))).ToList();
                for (var i = 0; i < presets.Count; i++, id++)
                    specs[id] = CreatePreset(id, family, presets[i], i);

                for (; id <= last; id++)
                    specs[id] = CreateCombination(id, family, used);
            }
        }

        private static Spec CreatePreset(int id, Family family, (string name, float radius) preset, int index)
        {
            var random = new Random(Hash(id));
            var background = family.Backgrounds[index % family.Backgrounds.Length];
            var accent = family.Lights[random.Next(family.Lights.Length)];
            var text = Pick(TextStyles, family, random);
            var name = preset.name;
            return new Spec
            {
                Id = id,
                Text = Words[random.Next(Words.Length)],
                Size = new Vector2(180, 54),
                Radius = preset.radius,
                Background = background,
                Accent = accent,
                TextColor = TextColorOn(background),
                Description = $"{family.Name} | preset \"{name}\" | text: {text.Name} | background {Format(background)} | text glow {Format(accent)}",
                CreateEffects = () => ButtonEffectPresets.Create(name),
                CreateTextShadings = () => Soften(text.Create(family, accent))
            };
        }

        private static Spec CreateCombination(int id, Family family, HashSet<string> used)
        {
            var random = new Random(Hash(id));
            while (true)
            {
                var background = family.Backgrounds[random.Next(family.Backgrounds.Length)];
                var accent = family.Lights[random.Next(family.Lights.Length)];
                var aura = Pick(Auras, family, random);
                var border = Pick(Borders, family, random);
                var body = Pick(Bodies, family, random);
                var lights = Pick(Lights, family, random);
                var motion = Pick(Motions, family, random);
                var text = Pick(TextStyles, family, random);
                var key = string.Join("|", Format(background), Format(accent), aura.Name, border.Name, body.Name, lights.Name, motion.Name, text.Name);
                if (!used.Add(key))
                    continue;

                // Mostly wide buttons, a few round icons.
                var isIcon = random.Next(12) == 0;
                var size = isIcon
                    ? new Vector2(64, 64)
                    : new Vector2(150 + random.Next(5) * 10, 44 + random.Next(5) * 4);
                var radii = new[] { -1f, -1f, 8f, 12f, 16f, 22f, 4f, 14f };
                var radius = isIcon ? (random.Next(2) == 0 ? -1f : 16f) : radii[random.Next(radii.Length)];
                var word = isIcon ? ShortWords[random.Next(ShortWords.Length)] : Words[random.Next(Words.Length)];
                var seed = random.Next(1, 1000);
                var parts = new[] { aura, border, body, lights, motion };
                return new Spec
                {
                    Id = id,
                    Text = word,
                    Size = size,
                    Radius = radius,
                    Background = background,
                    Accent = accent,
                    TextColor = TextColorOn(background),
                    Description = $"{family.Name} | " + string.Join(" | ", parts.Where(part => !part.Name.StartsWith("No ")).Select(part => part.Name))
                        + $" | text: {text.Name} | background {Format(background)} | light {Format(accent)}",
                    CreateEffects = () => parts.SelectMany(part => part.Create(family, background, accent, seed)).ToList(),
                    CreateTextShadings = () => Soften(text.Create(family, accent))
                };
            }
        }

        private static Option<T> Pick<T>(Option<T>[] options, Family family, Random random)
        {
            var total = options.Where(option => option.Suits(family)).Sum(option => option.Weight);
            var value = random.Next(total);
            foreach (var option in options)
            {
                if (!option.Suits(family))
                    continue;
                value -= option.Weight;
                if (value < 0)
                    return option;
            }

            return options[0];
        }

        /// <summary>
        /// Mixes the bits of a number: the first values of generators seeded with close numbers are alike, so the seeds
        /// of neighbor buttons must be far apart.
        /// </summary>
        private static int Hash(int id)
        {
            unchecked
            {
                var x = (uint)id;
                x ^= x >> 16;
                x *= 0x7feb352d;
                x ^= x >> 15;
                x *= 0x846ca68b;
                x ^= x >> 16;
                return (int)(x & 0x7fffffff);
            }
        }

        /// <summary>Dark text on light buttons, white text on the others.</summary>
        private static Color TextColorOn(Color background)
            => (0.299f * background.R + 0.587f * background.G + 0.114f * background.B) / 255f > 0.62f ? new Color(50, 30, 10) : Color.White;

        private static string Format(Color color) => $"({color.R}, {color.G}, {color.B})";
    }
}
