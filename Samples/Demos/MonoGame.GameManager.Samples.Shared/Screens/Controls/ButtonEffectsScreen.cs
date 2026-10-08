using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Controls.Effects;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Samples.Screens.Controls
{
    /// <summary>
    /// The effects of the buttons: a preview button where every effect and option can be changed live (presets, size,
    /// corner radius, textures, scale, rotation, the text with its color, size and border, glows, inner lights,
    /// highlights, glints, shades, outlines, lips, shadows, text outlines, gems, running lights, sweeps, sparkles, rays
    /// and ripples), and a scrolling gallery of buttons of different sizes, shapes, colors and text styles.
    /// </summary>
    public class ButtonEffectsScreen : Screen
    {
        private const int SectionTop = Config.ScreenContentMargin + 60;
        private const int SectionHeight = 690;
        private const int SectionDivisionLeft = 550;
        private const int OptionsWidth = SectionDivisionLeft - Config.ScreenContentMargin * 2;
        private const int PreviewLeft = SectionDivisionLeft + Config.ScreenContentMargin;
        private const int PreviewWidth = 600;
        private const int StageTop = 34;
        private const int StageHeight = 330;
        private const int GalleryTop = 400;
        private const int PagesTop = 78;
        private const int RowHeight = 36;
        private const int ActionsTop = 648;

        private static readonly Vector2 StageCenter = new Vector2(PreviewWidth / 2f, StageHeight / 2f);
        private static readonly Color DarkStageColor = new Color(10, 14, 28);
        private static readonly Color TabColor = new Color(60, 60, 60);
        private static readonly Color SelectedTabColor = new Color(80, 160, 230);
        private static readonly Color ActionColor = new Color(70, 110, 70);
        private static readonly Color SmallButtonBorder = new Color(110, 110, 110);
        private static readonly string[] TabNames = { "Button", "Text", "Glow", "Inner", "Borders", "Particles", "Motion" };
        private static readonly string[] Texts =
        {
            "PLAY", "Start", "NEXT", "Yes", "BUY", "More Games", "Rate Us", "BATTLE", "EXPLORE AGAIN", "SPIN", "Confirm Purchase", "(no text)"
        };
        private static readonly string[] ShapeNames = Enum.GetNames(typeof(LightShape));
        private static readonly string[] AreaNames = Enum.GetNames(typeof(SparkleArea));
        private static readonly string[] HighlightStyleNames = Enum.GetNames(typeof(HighlightStyle));
        private static readonly string[] ShadePlacementNames = Enum.GetNames(typeof(ShadePlacement));
        private static readonly string[] GlowEdgeNames = Enum.GetNames(typeof(GlowEdges));
        private static readonly string[] ColorSourceNames = { "Own color", "Button color" };
        private static readonly string[] BlendNames = { "Light (added)", "Paint (over)" };
        private static readonly string[] PositionNames = { "Outside", "Inside" };
        private static readonly string[] FieldAreaNames = Enum.GetNames(typeof(ParticleFieldArea));
        private static readonly string[] FrameStyleNames = { "Gold", "Silver", "Bronze", "Button color", "Gem (sky)", "Gem (ruby)", "Dark", "Candy (white)" };
        private static readonly Func<FrameEffect>[] FrameStyles =
        {
            FrameEffect.Gold, FrameEffect.Silver, FrameEffect.Bronze, FrameEffect.ButtonColor,
            () => FrameEffect.Gem(new Color(60, 170, 255)), () => FrameEffect.Gem(new Color(230, 40, 80)), FrameEffect.Dark, FrameEffect.Candy
        };
        private static readonly string[] FieldShapeNames = { "Glows and dots", "Glows", "Sparkles", "Stars", "Dots", "Diamonds", "Everything" };
        private static readonly LightShape[][] FieldShapes =
        {
            new[] { LightShape.Glow, LightShape.Glow, LightShape.Flare, LightShape.Circle },
            new[] { LightShape.Glow },
            new[] { LightShape.Flare },
            new[] { LightShape.Star, LightShape.Flare },
            new[] { LightShape.Circle },
            new[] { LightShape.Diamond, LightShape.Flare },
            new[] { LightShape.Glow, LightShape.Flare, LightShape.Star, LightShape.Circle, LightShape.Diamond }
        };
        private static readonly string[] AnchorChoiceNames = { "Top center", "Top corners", "Top + corners", "Corners", "Edges", "All", "Center" };
        private static readonly Anchor[][] AnchorChoices =
        {
            new[] { Anchor.TopCenter },
            new[] { Anchor.TopLeft, Anchor.TopRight },
            new[] { Anchor.TopCenter, Anchor.TopLeft, Anchor.TopRight, Anchor.BottomLeft, Anchor.BottomRight },
            CornerLightEffect.Corners,
            CornerLightEffect.Edges,
            CornerLightEffect.All,
            new[] { Anchor.Center }
        };
        private static readonly string[] GlintCornerNames = { "Top left", "Top right", "Both top", "Bottom right" };
        private static readonly Anchor[][] GlintCorners =
        {
            new[] { Anchor.TopLeft }, new[] { Anchor.TopRight }, new[] { Anchor.TopLeft, Anchor.TopRight }, new[] { Anchor.BottomRight }
        };
        private static readonly string[] RayAnchorNames = { "Center", "Top center", "Bottom center" };
        private static readonly Anchor[] RayAnchors = { Anchor.Center, Anchor.TopCenter, Anchor.BottomCenter };
        private static readonly Type[] AnimatedEffects = { typeof(ShineSweepEffect), typeof(RunningLightEffect), typeof(SparkleEffect), typeof(LightRaysEffect) };

        private static readonly List<Color> LightColors = new List<Color>
        {
            Color.White, new Color(255, 190, 60), new Color(255, 110, 40), new Color(255, 60, 60),
            new Color(255, 90, 220), new Color(170, 90, 255), new Color(60, 200, 255), new Color(60, 230, 120)
        };

        private static readonly List<Color> PaintColors = new List<Color>
        {
            Color.White, Color.Black, new Color(60, 30, 10), new Color(90, 40, 0), new Color(20, 40, 90),
            new Color(20, 70, 20), new Color(255, 230, 150), new Color(120, 20, 20)
        };

        private static readonly List<Color> BackgroundColors = new List<Color>
        {
            new Color(255, 180, 20), new Color(250, 120, 30), new Color(110, 200, 40), new Color(40, 130, 230),
            new Color(150, 80, 210), new Color(235, 90, 160), new Color(220, 50, 70), new Color(25, 20, 40)
        };

        private static readonly List<Color> TextColors = new List<Color>
        {
            Color.White, new Color(255, 240, 190), new Color(255, 215, 90), new Color(255, 170, 220),
            new Color(150, 225, 255), new Color(190, 255, 130), new Color(70, 35, 0), Color.Black
        };

        private static readonly List<Color> TextBorderColors = new List<Color>
        {
            Color.Black, new Color(90, 45, 0), new Color(120, 20, 20), new Color(70, 20, 110),
            new Color(15, 40, 100), new Color(10, 70, 30), new Color(255, 200, 60), Color.White
        };

        private static readonly List<Color> StageColors = new List<Color>
        {
            DarkStageColor, new Color(70, 60, 110), new Color(60, 60, 70), new Color(35, 70, 60), new Color(215, 235, 240)
        };

        /// <summary>The background color and the corner radius that suit each preset (a negative radius = a pill).</summary>
        private static readonly Dictionary<string, (Color background, float radius)> PresetStyles = new Dictionary<string, (Color, float)>
        {
            { "Ornate", (new Color(30, 90, 210), 20) },
            { "Starlight", (new Color(25, 35, 95), 16) },
            { "Treasure", (new Color(215, 125, 20), 14) },
            { "Crystal", (new Color(50, 150, 215), 12) },
            { "Galaxy", (new Color(60, 25, 125), 18) },
            { "Fairy", (new Color(45, 150, 80), -1) },
            { "Sunburst", (new Color(235, 105, 20), 14) },
            { "Candy", (new Color(255, 180, 20), -1) },
            { "Jelly", (new Color(110, 200, 40), -1) },
            { "Glossy", (new Color(40, 130, 230), 16) },
            { "Cartoon", (new Color(250, 120, 30), 14) },
            { "Bubble", (new Color(150, 80, 210), -1) },
            { "Pearl", (new Color(235, 110, 170), -1) },
            { "Soft Glow", (new Color(30, 170, 190), -1) },
            { "Inner Light", (new Color(220, 50, 70), 12) },
            { "Framed", (new Color(230, 170, 40), 10) },
            { "Minimal", (new Color(70, 90, 120), 8) },
            { "Gold", (new Color(150, 90, 20), 16) },
            { "Royal", (new Color(20, 60, 150), 10) },
            { "Magic", (new Color(70, 25, 120), 18) },
            { "Ice", (new Color(20, 80, 120), 12) },
            { "Fire", (new Color(150, 50, 10), 14) },
            { "Emerald", (new Color(15, 90, 50), 20) },
            { "Neon", (new Color(25, 20, 40), 36) },
            { "Rainbow", (new Color(40, 30, 70), 16) },
            { "Legendary", (new Color(120, 60, 10), 10) },
            { "Glass", (new Color(20, 90, 200), 12) },
            { "Alert", (new Color(130, 20, 20), 8) },
            { "Subtle", (new Color(60, 60, 75), 8) }
        };

        // Colors of the gallery
        private static readonly Color Yellow = new Color(255, 190, 30);
        private static readonly Color Orange = new Color(250, 120, 30);
        private static readonly Color Red = new Color(225, 50, 60);
        private static readonly Color Pink = new Color(235, 90, 160);
        private static readonly Color Purple = new Color(150, 80, 210);
        private static readonly Color Violet = new Color(105, 35, 200);
        private static readonly Color Blue = new Color(40, 130, 230);
        private static readonly Color Sky = new Color(40, 180, 220);
        private static readonly Color Teal = new Color(30, 165, 165);
        private static readonly Color Green = new Color(110, 200, 40);
        private static readonly Color Forest = new Color(40, 140, 70);
        private static readonly Color Night = new Color(30, 25, 45);
        private static readonly Color Gray = new Color(70, 80, 95);
        private static readonly Color Ruby = new Color(170, 20, 40);

        /// <summary>The sections of the gallery: presets and hand-made combinations on buttons of many sizes, shapes and colors.</summary>
        private static readonly GallerySection[] GallerySections =
        {
            new GallerySection("Rich and static: a lot of glow and particles, nothing moves", new[]
            {
                Item("Ornate", "PLAY", 170, 52, 20),
                Item("Ornate", "PLAY", 170, 52, 20, Violet).WithText(new Color(255, 230, 150), new Color(50, 20, 90), 3),
                Item("Ornate", "PLAY", 170, 52, 20, new Color(205, 105, 15)).WithText(Color.White, new Color(110, 50, 0), 3),
                Item("Ornate", "QUEST", 150, 50, 20, Forest).WithText(new Color(255, 215, 90), new Color(10, 50, 20), 3),
                Item("Ornate", "FIGHT", 150, 50, 20, Ruby).WithText(Color.White, new Color(90, 0, 10), 3),
                Item("Ornate", "SHOP", 150, 50, 20, Teal).WithText(new Color(220, 255, 250)),
                Item("Starlight", "Stars", 160, 48, 16),
                Item("Starlight", "Night", 150, 48, 24, new Color(60, 30, 110)).WithText(new Color(200, 220, 255), new Color(20, 10, 50), 2),
                Item("Starlight", "i", 60, 60, -1).WithText(new Color(255, 230, 120)),
                Item("Treasure", "Treasure", 160, 48, 14).WithText(new Color(255, 240, 190), new Color(100, 50, 0), 3),
                Item("Treasure", "OPEN", 140, 48, 14, new Color(200, 70, 20)),
                Item("Treasure", "$", 64, 64, 16).WithText(new Color(255, 220, 80), new Color(90, 40, 0), 4),
                Item("Crystal", "Crystal", 150, 46, 12),
                Item("Crystal", "Gem", 120, 46, 12, new Color(220, 80, 150)).WithText(Color.White, new Color(120, 20, 70), 2),
                Item("Crystal", "Amethyst", 170, 46, -1, new Color(130, 70, 200)).WithText(new Color(240, 220, 255)),
                Item("Galaxy", "Galaxy", 160, 48, 18),
                Item("Galaxy", "Cosmos", 150, 48, 18, new Color(25, 50, 130)).WithText(new Color(180, 230, 255), new Color(10, 20, 70), 3),
                Item("Galaxy", "WARP", 220, 44, 22, new Color(90, 20, 110)).WithText(new Color(255, 170, 240), new Color(40, 0, 60), 2),
                Item("Fairy", "Fairy", 150, 46, -1),
                Item("Fairy", "Bloom", 150, 46, -1, new Color(215, 80, 150)).WithText(Color.White, new Color(130, 30, 90), 2),
                Item("Fairy", "Spring", 150, 46, -1, new Color(30, 160, 150)).WithText(new Color(230, 255, 200)),
                Item("Sunburst", "REWARD", 150, 48, 14).WithText(new Color(255, 250, 200), new Color(140, 50, 0), 3),
                Item("Sunburst", "LEVEL UP", 170, 48, 14, new Color(130, 50, 200)).WithText(new Color(255, 220, 90), new Color(60, 10, 100), 3),
                Item("Sunburst", "1st", 70, 70, -1, new Color(240, 160, 20)).WithText(Color.White, new Color(130, 60, 0), 4),
                Item("Ornate", "VIP", 150, 50, 14, Night).WithText(new Color(255, 215, 120)).WithEffects(Vip),
                Item("Ornate", "MYTHIC", 160, 50, 12, Ruby).WithEffects(Mythic),
                Item("Crystal", "FROZEN", 160, 50, 16, new Color(40, 120, 190)).WithText(new Color(225, 245, 255)).WithEffects(Frozen)
            }),
            new GallerySection("Static: glossy, candy and cartoon styles", new[]
            {
                Item("Candy", "Start", 150, 48, -1, Yellow),
                Item("Candy", "Play", 140, 48, -1, Blue).WithText(new Color(255, 240, 150)),
                Item("Candy", "Bonus", 140, 48, -1, Purple).WithText(new Color(255, 225, 120)),
                Item("Candy", "Gift", 120, 48, -1, Orange).WithText(new Color(255, 245, 210)),
                Item("Candy", "Like", 120, 46, -1, Pink),
                Item("Candy", "OK", 100, 46, -1, Teal).WithText(new Color(230, 255, 250)),
                Item("Candy", "+", 54, 54, -1, Green).WithText(Color.White, new Color(20, 80, 10), 3),
                Item("Candy", "x", 54, 54, -1, Red).WithText(Color.White, new Color(100, 10, 20), 3),
                Item("Candy", "?", 54, 54, -1, Blue).WithText(new Color(255, 230, 120), new Color(10, 30, 90), 3),
                Item("Candy", "!", 54, 54, -1, Yellow).WithText(new Color(200, 30, 30), Color.White, 3),
                Item("Candy", "game over", 170, 42, 10, Red),
                Item("Candy", "ok", 64, 30, -1, Green),
                Item("Jelly", "Yes", 140, 48, -1, Green).WithText(Color.White, new Color(30, 90, 10), 3),
                Item("Jelly", "No", 120, 48, -1, Red).WithText(Color.White, new Color(110, 10, 20), 3),
                Item("Jelly", "Coins", 130, 48, -1, Yellow).WithText(new Color(110, 55, 0)),
                Item("Jelly", "Magic", 140, 48, -1, Purple).WithText(new Color(255, 225, 255), new Color(70, 20, 110), 2),
                Item("Jelly", "shop", 96, 40, -1, Orange),
                Item("Jelly", "Love", 130, 46, -1, Pink).WithText(Color.White, new Color(150, 20, 80), 2),
                Item("Jelly", "?", 58, 58, 16, Blue),
                Item("Jelly", "+", 58, 58, 16, Teal).WithText(new Color(220, 255, 140), new Color(0, 60, 60), 3),
                Item("Glossy", "NEXT", 140, 46, 14, Blue).WithText(Color.White, new Color(10, 40, 100), 2),
                Item("Glossy", "continue", 150, 40, 10, new Color(60, 170, 60)),
                Item("Glossy", "pause", 130, 44, 12, Red).WithText(new Color(255, 225, 225)),
                Item("Glossy", "back", 120, 44, 12, Orange).WithText(new Color(80, 30, 0)),
                Item("Glossy", "settings", 150, 44, 12, Purple),
                Item("Glossy", "Menu", 130, 44, 12, Night).WithText(new Color(120, 220, 255), new Color(10, 40, 80), 2),
                Item("Glossy", "II", 56, 56, 14, Sky).WithText(Color.White, new Color(0, 60, 90), 3),
                Item("Cartoon", "PLAY", 100, 46, 12, Orange),
                Item("Cartoon", "Loading...", 280, 40, -1, Green).WithText(new Color(240, 255, 200)),
                Item("Cartoon", "NEXT >>", 140, 46, 12, Blue),
                Item("Cartoon", "BUY", 110, 46, 12, Yellow).WithText(new Color(255, 250, 220)),
                Item("Cartoon", "EXIT", 110, 46, 12, Red),
                Item("Cartoon", "GO!", 100, 46, 12, Green).WithText(new Color(255, 240, 120)),
                Item("Cartoon", "SKIP", 110, 46, 12, Purple).WithText(new Color(255, 200, 255)),
                Item("Cartoon", "START GAME", 220, 50, 14, Orange).WithText(new Color(255, 245, 160)),
                Item("Bubble", "More Games", 180, 48, -1, Purple).WithText(Color.White, new Color(80, 30, 130), 2),
                Item("Bubble", "levels", 130, 42, -1, Sky),
                Item("Bubble", "Bubble", 130, 46, -1, Green).WithText(new Color(240, 255, 210), new Color(20, 80, 20), 2),
                Item("Bubble", "Pop", 100, 46, -1, Pink).WithText(new Color(120, 20, 70)),
                Item("Bubble", "5", 60, 60, -1, Yellow).WithText(Color.White, new Color(170, 90, 0), 4),
                Item("Pearl", "Rate Us", 150, 46, -1, new Color(235, 110, 170)).WithText(Color.White, new Color(170, 50, 110), 2),
                Item("Pearl", "Calm", 130, 46, -1, new Color(110, 160, 230)).WithText(new Color(25, 45, 100)),
                Item("Pearl", "Dream", 130, 46, -1, new Color(180, 140, 230)).WithText(Color.White, new Color(110, 70, 170), 2),
                Item("Framed", "BUY", 110, 46, 8, new Color(230, 170, 40)).WithText(new Color(90, 50, 0)),
                Item("Framed", "victory", 150, 42, 8, new Color(200, 40, 200)).WithText(new Color(255, 235, 140), new Color(90, 0, 90), 3),
                Item("Framed", "Shop", 120, 44, 8, Blue),
                Item("Framed", "retry", 120, 44, 8, Red).WithText(Color.White, new Color(90, 0, 0), 2),
                Item("Framed", "Upgrade", 150, 44, 8, Forest).WithText(new Color(255, 215, 90)),
                Item("Minimal", "back", 84, 36, 8, Gray),
                Item("Minimal", "cancel", 100, 36, 8, Night).WithText(new Color(255, 140, 140)),
                Item("Minimal", "info", 90, 36, 8, Blue).WithText(new Color(200, 230, 255)),
                Item("Soft Glow", "Next", 150, 46, -1, Teal),
                Item("Soft Glow", "Glow", 130, 46, -1, Purple).WithText(new Color(255, 220, 255), new Color(80, 30, 120), 2),
                Item("Soft Glow", "Warm", 130, 46, -1, Orange).WithText(new Color(255, 245, 200)),
                Item("Inner Light", "pause", 140, 46, 12, Red),
                Item("Inner Light", "Inner", 130, 46, 12, Blue).WithText(Color.White, new Color(10, 30, 90), 2),
                Item("Inner Light", "Fresh", 130, 46, 12, Forest).WithText(new Color(220, 255, 200)),
                Item("Pearl", "Soft UI", 150, 48, 16, new Color(225, 228, 235)).WithText(new Color(60, 70, 90)).WithEffects(SoftUi),
                Item("Cartoon", "Wood", 140, 48, 10, new Color(150, 90, 45)).WithText(new Color(255, 230, 180)).WithEffects(Wood),
                Item("Cartoon", "Comic", 130, 48, 12, new Color(255, 215, 50)).WithText(Color.Black).WithEffects(Comic)
            }),
            new GallerySection("Animated lights", new[]
            {
                Item("Gold", "SPIN", 130, 50, 25).WithText(new Color(255, 240, 180), new Color(110, 60, 0), 3),
                Item("Gold", "COLLECT", 150, 48, 14).WithText(Color.White, new Color(120, 70, 0), 2),
                Item("Gold", "x2", 64, 64, -1).WithText(new Color(255, 230, 100), new Color(90, 40, 0), 4),
                Item("Royal", "BATTLE", 150, 50, 10).WithText(new Color(255, 215, 100), new Color(10, 25, 80), 3),
                Item("Royal", "ARENA", 140, 48, 10),
                Item("Royal", "GUILD", 140, 48, 10, new Color(70, 30, 130)).WithText(new Color(230, 200, 255)),
                Item("Magic", "EXPLORE AGAIN", 200, 50, 14),
                Item("Magic", "SPELL", 120, 48, 18).WithText(new Color(230, 180, 255), new Color(40, 0, 70), 3),
                Item("Magic", "PORTAL", 150, 48, 18, new Color(20, 50, 140)).WithText(new Color(160, 240, 255), new Color(10, 20, 70), 2),
                Item("Ice", "+", 56, 56, 14),
                Item("Ice", "FREEZE", 200, 46, 12).WithText(new Color(220, 245, 255), new Color(10, 60, 110), 3),
                Item("Ice", "*", 56, 56, -1).WithText(new Color(180, 240, 255)),
                Item("Fire", "Confirm Purchase", 190, 46, 12),
                Item("Fire", "BURN", 120, 48, 14).WithText(new Color(255, 230, 120), new Color(120, 20, 0), 3),
                Item("Fire", "HOT", 90, 40, 10).WithText(new Color(255, 200, 60), new Color(90, 10, 0), 2),
                Item("Glass", "Buy", 90, 34, 8),
                Item("Glass", "Glass", 120, 40, 10).WithText(Color.White, new Color(10, 40, 110), 2),
                Item("Glass", "Accept", 130, 40, 10, new Color(40, 150, 70)).WithText(new Color(230, 255, 220)),
                Item("Emerald", "GO", 64, 92, 14).WithText(Color.White, new Color(0, 60, 30), 4),
                Item("Emerald", "NATURE", 150, 48, 20).WithText(new Color(220, 255, 180), new Color(0, 60, 30), 3),
                Item("Emerald", "UP", 60, 80, 14),
                Item("Neon", "NEON", 92, 40, 20).WithText(new Color(255, 120, 230), new Color(80, 0, 70), 2),
                Item("Neon", "PLAY", 120, 44, -1).WithText(new Color(120, 255, 250)),
                Item("Neon", "ARCADE", 200, 44, 22).WithText(new Color(120, 255, 250), new Color(0, 60, 90), 2),
                Item("Legendary", "LEGENDARY", 170, 48, 8).WithText(new Color(255, 210, 90), new Color(70, 30, 0), 3),
                Item("Legendary", "EPIC", 130, 48, 8, new Color(90, 30, 150)).WithText(new Color(240, 200, 255), new Color(50, 10, 90), 3),
                Item("Legendary", "S", 64, 64, 16).WithText(new Color(255, 230, 120), new Color(100, 40, 0), 4),
                Item("Rainbow", "Textured", 0, 0, 6).AsTextured(),
                Item("Rainbow", "Rainbow", 150, 46, -1),
                Item("Rainbow", "PARTY", 140, 46, 14, new Color(30, 20, 50)).WithText(new Color(255, 240, 120), new Color(80, 20, 120), 3),
                Item("Alert", "ALERT", 104, 38, 8).Rotated(-6f),
                Item("Alert", "WARNING", 150, 44, 8).WithText(new Color(255, 230, 60), new Color(60, 0, 0), 3),
                Item("Alert", "!", 56, 56, -1).WithText(Color.White, new Color(110, 0, 0), 4),
                Item("Subtle", "Subtle", 70, 28, 6),
                Item("Subtle", "Options", 120, 36, 8).WithText(new Color(200, 205, 220)),
                Item("Subtle", "Help", 90, 36, 8, new Color(30, 80, 150)).WithText(new Color(200, 225, 255)),
                Item("Neon", "AUTO: OFF", 110, 36, 18, new Color(40, 40, 50)).AsToggle()
            })
        };

        private static GalleryItem Item(string preset, string text, float width, float height, float radius, Color? background = null)
            => new GalleryItem(preset, text, new Vector2(width, height), radius) { Background = background };

        // ---- Hand-made combinations of the gallery

        /// <summary>A dark VIP button: a golden frame, golden light gathering at the bottom, sparkles and a gem.</summary>
        private static List<ButtonEffect> Vip()
        {
            var gold = new Color(255, 205, 100);
            return new List<ButtonEffect>
            {
                new GlowEffect().SetColor(new Color(255, 180, 60)).SetRadius(14).SetIntensity(0.35f).IgnoreStates(),
                FrameEffect.Gold(),
                new InnerShadeEffect(ShadePlacement.Edges).SetRadius(14).SetIntensity(0.4f),
                new GlowEffect(GlowPlacement.Inner).SetEdges(GlowEdges.Bottom).SetColor(new Color(255, 170, 60)).SetRadius(18).SetIntensity(0.35f),
                new ParticleFieldEffect(ParticleFieldArea.Bottom).SetColor(gold).SetCount(14).SetSize(3, 9).SetSeed(31),
                new ParticleFieldEffect().SetShapes(LightShape.Flare, LightShape.Circle).SetColor(gold).SetCount(6).SetSize(3, 7).SetIntensity(0.35f).SetSeed(12),
                new HighlightEffect(HighlightStyle.Strip).SetInset(12, 5).SetThickness(2.5f).SetIntensity(0.18f),
                new TextOutlineEffect().SetOutline(new Color(60, 35, 0), 1.5f).SetShadow(new Vector2(0, 2), Color.Black * 0.5f),
                new CornerLightEffect().SetAnchors(Anchor.TopCenter).SetShape(LightShape.Diamond).SetColor(gold).SetSize(10, 26).SetStreak(80, 3).SetTwinkle(0f, 0f)
            };
        }

        /// <summary>A mythic button: still red rays, a ruby frame, embers at the bottom and a warm reflection.</summary>
        private static List<ButtonEffect> Mythic()
        {
            var ember = new Color(255, 120, 60);
            return new List<ButtonEffect>
            {
                new LightRaysEffect().SetColor(new Color(255, 80, 70)).SetRays(10, 0, 12).SetRotationSpeed(0).SetFlicker(0).SetIntensity(0.3f).IgnoreStates(),
                new GlowEffect().SetColor(new Color(255, 60, 60)).SetRadius(16).SetIntensity(0.5f).IgnoreStates(),
                FrameEffect.Gem(new Color(255, 70, 90)),
                new InnerShadeEffect(ShadePlacement.Edges).SetRadius(14).SetIntensity(0.35f),
                new ParticleFieldEffect(ParticleFieldArea.Bottom).SetColors(ember, new Color(255, 200, 90), new Color(255, 70, 50)).SetCount(16).SetSize(3, 9).SetSeed(23),
                new HighlightEffect().SetInset(10, 4).SetHeightRate(0.38f).SetIntensity(0.25f),
                new GlintEffect().SetCorners(Anchor.TopLeft).SetDots(2, 5)
            };
        }

        /// <summary>A frozen button: a silver frame, a cold inner light, frost diamonds at the top and snow above it.</summary>
        private static List<ButtonEffect> Frozen()
        {
            return new List<ButtonEffect>
            {
                new GlowEffect().SetColor(new Color(170, 230, 255)).SetRadius(14).SetIntensity(0.4f).IgnoreStates(),
                FrameEffect.Silver(),
                new GlowEffect(GlowPlacement.Inner).SetRadius(12).SetIntensity(0.45f),
                new HighlightEffect().SetInset(8, 3).SetHeightRate(0.45f).SetFade(0.5f).SetIntensity(0.3f),
                new ParticleFieldEffect(ParticleFieldArea.Top).SetShapes(LightShape.Diamond, LightShape.Flare).SetCount(8).SetSize(3, 7).SetSeed(41),
                new ParticleFieldEffect(ParticleFieldArea.Above).SetShapes(LightShape.Circle, LightShape.Flare).SetCount(10).SetSize(2, 6).SetSpread(18).SetIntensity(0.4f).SetSeed(43)
            };
        }

        /// <summary>A soft UI button: a light body, a soft shadow, a white edge and a clear reflection.</summary>
        private static List<ButtonEffect> SoftUi()
        {
            return new List<ButtonEffect>
            {
                new DropShadowEffect().SetOffset(new Vector2(0, 6)).SetBlur(12).SetIntensity(0.28f),
                new OutlineEffect(OutlinePosition.Inside).SetColor(Color.White).SetThickness(1.5f).SetIntensity(0.7f),
                new InnerShadeEffect().SetHeightRate(0.5f).SetIntensity(0.08f),
                new HighlightEffect().SetInset(8, 3).SetHeightRate(0.45f).SetFade(0.8f).SetIntensity(0.5f)
            };
        }

        /// <summary>A wooden button: a bronze frame, a darker bottom, a lip and carved text.</summary>
        private static List<ButtonEffect> Wood()
        {
            return new List<ButtonEffect>
            {
                new DropShadowEffect().SetOffset(new Vector2(0, 5)).SetBlur(6).SetIntensity(0.3f),
                new DepthEffect().SetDepth(5).UseButtonColor(0.6f),
                FrameEffect.Bronze().SetThicknessScale(0.8f),
                new InnerShadeEffect(ShadePlacement.Edges).SetRadius(10).SetIntensity(0.3f),
                new InnerShadeEffect().SetHeightRate(0.4f).SetIntensity(0.15f),
                new HighlightEffect(HighlightStyle.Strip).SetInset(10, 5).SetThickness(3).SetIntensity(0.22f),
                new TextOutlineEffect().UseButtonColor(0.75f).SetOutline(Color.Black, 1.5f).SetShadow(new Vector2(0, 2), Color.Black * 0.35f)
            };
        }

        /// <summary>A comic book button: a thick black border, a black lip, a light strip and a light shade.</summary>
        private static List<ButtonEffect> Comic()
        {
            return new List<ButtonEffect>
            {
                new DepthEffect().SetDepth(6).UseButtonColor(null).SetColor(Color.Black),
                new OutlineEffect().SetColor(Color.Black).SetThickness(3),
                new InnerShadeEffect().SetHeightRate(0.4f).SetIntensity(0.1f),
                new HighlightEffect(HighlightStyle.Strip).SetInset(10, 5).SetThickness(3).SetIntensity(0.55f)
            };
        }

        private readonly Dictionary<ButtonEffect, float> pausedPulses = new Dictionary<ButtonEffect, float>();
        private readonly List<ButtonEffect> pausedEffects = new List<ButtonEffect>();
        private readonly Dictionary<ButtonEffect, float> baseIntensities = new Dictionary<ButtonEffect, float>();
        private Button preview;
        private RectangleControl stageBackground;
        private Label stateLabel;
        private ScrollViewer[] pages;
        private Button[] tabButtons;
        private int selectedTab;
        private int row;

        // Options of the preview button
        private string presetName = "Ornate";
        private float width = 260f;
        private float height = 72f;
        private float cornerRadius = 20f;
        private Color backgroundColor = new Color(30, 90, 210);
        private bool useTextures;
        private int textIndex;
        private string customText;
        private Color textColor = Color.White;
        private float textSize = 1f;
        private bool textBorder;
        private float textBorderThickness = 3f;
        private Color textBorderColor = Color.Black;
        private float scale = 1f;
        private float rotation;
        private float opacity = 1f;
        private bool isEnabled = true;
        private float padding;
        private bool unscaledTime;
        private bool animations = true;
        private float lightStrength = 1f;

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Button Effects", OpenButtonEffectsScreen)
            });

            CreatePreviewSection();
            CreateOptionsSection();
            new RectangleControl(new Rectangle(SectionDivisionLeft, SectionTop, 2, SectionHeight), Color.White)
                .AddToScreen();

            ApplyPreset(presetName);
            ShowTab(0);

            base.OnInit();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var text = $"State: {preview.VisualState}";
            if (stateLabel.Text != text)
                stateLabel.Text = text;
        }

        public override void Dispose()
        {
            ServiceProvider.Clock.TimeScale = 1f;
            base.Dispose();
        }

        public static void OpenButtonEffectsScreen()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new ButtonEffectsScreen());
        }

        /// <summary>Replaces the effects of the preview with a preset, with the background color and corner radius that suit it.</summary>
        public void ApplyPreset(string name)
        {
            presetName = name;
            textColor = Color.White;
            textBorder = false;
            if (PresetStyles.TryGetValue(name, out var style))
            {
                backgroundColor = style.background;
                cornerRadius = style.radius < 0f ? height / 2f : style.radius;
            }

            SetPreviewEffects(ButtonEffectPresets.Create(name));
        }

        /// <summary>Shows one of the option pages.</summary>
        public void ShowTab(int index)
        {
            selectedTab = Math.Max(0, Math.Min(pages.Length - 1, index));
            for (var i = 0; i < pages.Length; i++)
            {
                pages[i].SetIsVisible(i == selectedTab);
                tabButtons[i].BackgroundColor = i == selectedTab ? SelectedTabColor : TabColor;
            }
        }

        private void SetPreviewEffects(IEnumerable<ButtonEffect> effects)
        {
            pausedEffects.Clear();
            pausedPulses.Clear();
            preview.SetEffects(effects);
            baseIntensities.Clear();
            foreach (var effect in preview.Effects)
                baseIntensities[effect] = effect.Intensity;
            ApplyButtonLayout();
            RebuildPages();
            if (!animations)
                SetAnimations(false);
            ApplyLightStrength();
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

            var stage = new Panel(new Rectangle(0, StageTop, PreviewWidth, StageHeight))
                .SetHideOverflow(true)
                .AddToScreen(container);
            stageBackground = new RectangleControl(Vector2.Zero, stage.Size, DarkStageColor)
                .AddToScreen(stage);

            preview = new Button(Vector2.Zero, new Vector2(width, height), backgroundColor)
                .SetText(font, Texts[textIndex], Color.White)
                .AddToScreen(stage);

            var infoTop = StageTop + StageHeight + 6;
            stateLabel = new Label(font, string.Empty, new Vector2(0, infoTop), Color.White)
                .SetScale(0.65f)
                .AddToScreen(container);
            new Label(font, "Hover, press and click the button", new Vector2(150, infoTop), Color.Gray)
                .SetScale(0.6f)
                .AddToScreen(container);
            new FpsCounter(font, new Vector2(PreviewWidth - 90, infoTop), Color.Yellow)
                .SetScale(0.65f)
                .AddToScreen(container);

            CreateGallery(container);
        }

        /// <summary>Applies the size, shape, textures, text and transformation options to the preview.</summary>
        private void ApplyButtonLayout()
        {
            var content = ContentHandler.Instance;
            if (useTextures)
            {
                var texture = content.TextureButtonBackground;
                preview.SetTextures(texture, content.TextureButtonBackgroundHover, content.TextureButtonBackgroundPressed)
                    .SetBackgroundScale(new Vector2(width / texture.Width, height / texture.Height));
            }
            else
            {
                preview.SetTextures(null);
                preview.SetBackgroundScale(Vector2.One);
                preview.Size = new Vector2(width, height);
            }

            preview.SetBackgroundColors(backgroundColor);
            preview.SetCornerRadius(useTextures ? 6f * Math.Min(width / 145f, height / 40f) : cornerRadius)
                .SetEffectPadding(padding)
                .SetEffectsUseUnscaledTime(unscaledTime)
                .SetIsEnabled(isEnabled)
                .SetOpacity(opacity);

            var text = customText ?? (textIndex < Texts.Length - 1 ? Texts[textIndex] : string.Empty);
            preview.SetText(string.IsNullOrEmpty(text) ? " " : text);
            preview.TextLabel.SetIsVisible(!string.IsNullOrEmpty(text));
            preview.TextLabel.Color = textColor;
            preview.TextLabel.SetOutline(textBorderColor, textBorder ? textBorderThickness : 0f);
            preview.TextLabel.SetScale(textSize * Math.Min(MathHelper.Clamp(height / 70f, 0.45f, 1.4f),
                string.IsNullOrEmpty(text) ? 1f : (width - Math.Max(20f, cornerRadius)) / ContentHandler.Instance.Font.MeasureString(text).X));

            preview.SetOriginRate(new Vector2(0.5f))
                .SetPosition(StageCenter)
                .SetRotationInDegree(rotation)
                .SetScale(scale);
            CenterText(preview);
        }

        /// <summary>
        /// The children of a control are placed from its position, not from its origin: the text of a button centered
        /// with an origin is moved back by the origin.
        /// </summary>
        private static void CenterText<TButton>(ButtonAbstract<TButton> button) where TButton : ButtonAbstract<TButton>
            => button.TextLabel?.SetPosition(-button.OriginWithoutScale);

        // ---- Gallery

        private const float GalleryMargin = 16f;
        private const float GalleryGapX = 34f;
        private const float GalleryGapY = 40f;

        private void CreateGallery(Panel container)
        {
            var font = ContentHandler.Instance.Font;
            var frame = new Panel(new Rectangle(0, GalleryTop, PreviewWidth, SectionHeight - GalleryTop))
                .SetHideOverflow(true)
                .AddToScreen(container);
            new RectangleControl(Vector2.Zero, frame.Size, new Color(45, 40, 75))
                .AddToScreen(frame);
            new Label(font, "Gallery: scroll for more, click a button to edit it", new Vector2(12, 6), Color.Yellow)
                .SetScale(0.6f)
                .AddToScreen(frame);

            var gallery = new ScrollViewer(new Vector2(0, 26), new Vector2(PreviewWidth, frame.Size.Y - 26))
                .SetHideOverflow(true)
                .AddToScreen(frame);
            var content = gallery.ContentPanel;

            // Each section: a title, then rows of buttons centered on the width of the gallery.
            var y = 6f;
            foreach (var section in GallerySections)
            {
                new Label(font, $"{section.Title} ({section.Items.Length})", new Vector2(12, y), new Color(200, 200, 230))
                    .SetScale(0.55f)
                    .AddToScreen(content);
                y += 44f;

                var row = new List<GalleryItem>();
                var rowWidth = 0f;
                foreach (var item in section.Items)
                {
                    var size = GetGallerySize(item);
                    if (row.Count > 0 && rowWidth + GalleryGapX + size.X > PreviewWidth - GalleryMargin * 2f)
                    {
                        y += AddGalleryRow(content, row, rowWidth, y) + GalleryGapY;
                        row.Clear();
                        rowWidth = 0f;
                    }

                    rowWidth += (row.Count > 0 ? GalleryGapX : 0f) + size.X;
                    row.Add(item);
                }

                if (row.Count > 0)
                    y += AddGalleryRow(content, row, rowWidth, y) + GalleryGapY;
                y += 10f;
            }

            // Some space under the last row, so its lips and shadows are not cut when scrolled to the bottom.
            new RectangleControl(new Rectangle(0, (int)y, 1, 10), Color.Transparent)
                .AddToScreen(content);
        }

        private static Vector2 GetGallerySize(GalleryItem item)
            => item.Textured ? ContentHandler.Instance.TextureButtonBackground.Bounds.Size.ToVector2() : item.Size;

        /// <summary>Adds a row of buttons centered on the gallery, and returns its height.</summary>
        private float AddGalleryRow(Panel content, List<GalleryItem> row, float rowWidth, float top)
        {
            var height = 0f;
            foreach (var item in row)
                height = Math.Max(height, GetGallerySize(item).Y);

            var x = (PreviewWidth - rowWidth) / 2f;
            foreach (var item in row)
            {
                var size = GetGallerySize(item);
                var center = new Vector2(x + size.X / 2f, top + height / 2f);
                if (item.Toggle)
                    CreateToggleExample(content, item, center);
                else
                    CreateGalleryButton(content, item, center);
                x += size.X + GalleryGapX;
            }

            return height;
        }

        private void CreateGalleryButton(Panel content, GalleryItem item, Vector2 center)
        {
            var font = ContentHandler.Instance.Font;
            var background = item.Background ?? PresetStyles[item.Preset].background;
            var button = item.Textured
                ? new Button(ContentHandler.Instance.TextureButtonBackground, Vector2.Zero)
                    .SetHoverTexture(ContentHandler.Instance.TextureButtonBackgroundHover)
                    .SetMousePressedTexture(ContentHandler.Instance.TextureButtonBackgroundPressed)
                : new Button(Vector2.Zero, item.Size, background);

            var size = GetGallerySize(item);
            button.SetCornerRadius(item.Radius < 0f ? size.Y / 2f : item.Radius)
                .SetText(font, item.Text, item.TextColor ?? Color.White)
                .AddEffects(item.CreateEffects())
                .AddOnClick(args => ApplyGalleryItem(item))
                .AddToScreen(content);
            button.TextLabel.SetOutline(item.TextBorderColor, item.TextBorderThickness)
                .SetScale(FitText(item.Text, size, item.Radius < 0f ? size.Y / 2f : item.Radius));
            button.SetOriginRate(new Vector2(0.5f))
                .SetPosition(center)
                .SetRotationInDegree(item.Rotation);
            CenterText(button);
        }

        /// <summary>The scale of a text that fits a button: from its height, and never wider than the button.</summary>
        private static float FitText(string text, Vector2 size, float radius)
        {
            var byHeight = MathHelper.Clamp(size.Y / 64f, 0.42f, 0.85f);
            if (string.IsNullOrEmpty(text))
                return byHeight;

            var textWidth = ContentHandler.Instance.Font.MeasureString(text).X;
            var room = size.X - 2f * Math.Max(10f, Math.Min(radius, size.Y / 2f) * 0.6f);
            return Math.Max(0.3f, Math.Min(byHeight, room / textWidth));
        }

        /// <summary>A toggle button whose lights turn on with it.</summary>
        private static void CreateToggleExample(Panel gallery, GalleryItem item, Vector2 center)
        {
            var font = ContentHandler.Instance.Font;
            var effects = item.CreateEffects();
            var toggle = new ToggleButton(Vector2.Zero, item.Size, item.Background ?? new Color(40, 40, 50))
                .SetToggledBackgroundColor(new Color(25, 20, 40))
                .SetCornerRadius(item.Radius < 0f ? item.Size.Y / 2f : item.Radius)
                .SetBorder(new Color(90, 90, 110), 1.5f)
                .SetText(font, "AUTO: OFF", Color.Gray)
                .AddEffects(effects)
                .AddToScreen(gallery);
            toggle.TextLabel.SetScale(0.5f);
            toggle.SetOriginRate(new Vector2(0.5f)).SetPosition(center);
            CenterText(toggle);

            void ApplyToggle(bool isOn)
            {
                foreach (var effect in effects)
                    effect.IsEnabled = isOn;
                toggle.SetText(isOn ? "AUTO: ON" : "AUTO: OFF");
                toggle.TextLabel.Color = isOn ? Color.White : Color.Gray;
            }

            toggle.AddOnToggled(ApplyToggle);
            ApplyToggle(false);
        }

        private void ApplyGalleryItem(GalleryItem item)
        {
            useTextures = item.Textured;
            var size = GetGallerySize(item);
            var zoom = MathHelper.Clamp(Math.Min(420f / size.X, 64f / size.Y), 1f, 1.6f);
            width = size.X * zoom;
            height = size.Y * zoom;
            rotation = item.Rotation < 0f ? 360f + item.Rotation : item.Rotation;
            customText = item.Text;
            textColor = item.TextColor ?? Color.White;
            textBorder = item.TextBorderThickness > 0f;
            if (textBorder)
            {
                textBorderColor = item.TextBorderColor;
                textBorderThickness = item.TextBorderThickness;
            }

            presetName = item.Preset;
            backgroundColor = item.Background ?? PresetStyles[item.Preset].background;
            cornerRadius = item.Radius < 0f ? height / 2f : item.Radius * zoom;
            SetPreviewEffects(item.CreateEffects());
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
            const float tabWidth = 70f;
            for (var i = 0; i < TabNames.Length; i++)
            {
                var index = i;
                var textScale = Math.Min(0.6f, (tabWidth - 10f) / font.MeasureString(TabNames[i]).X);
                tabButtons[i] = CreateSmallButton(container, TabNames[i], new Vector2(i * (tabWidth + 3f), 36), new Vector2(tabWidth, 32), TabColor, textScale, () => ShowTab(index));
                pages[i] = new ScrollViewer(new Vector2(0, PagesTop), new Vector2(OptionsWidth, ActionsTop - PagesTop - 8))
                    .SetHideOverflow(true)
                    .SetIsVisible(false)
                    .AddToScreen(container);
            }

            var actionSize = new Vector2(120, 36);
            CreateSmallButton(container, "Reset", new Vector2(0, ActionsTop), actionSize, ActionColor, 0.65f, () => ApplyPreset(presetName));
            CreateSmallButton(container, "Random", new Vector2(130, ActionsTop), actionSize, ActionColor, 0.65f, Randomize);
            CreateSmallButton(container, "Random calm", new Vector2(260, ActionsTop), actionSize, ActionColor, 0.65f, RandomizeCalm);
            CreateSmallButton(container, "No effects", new Vector2(390, ActionsTop), actionSize, ActionColor, 0.65f, () => SetPreviewEffects(Array.Empty<ButtonEffect>()));
        }

        private void RebuildPages()
        {
            if (pages == null)
                return;

            foreach (var page in pages)
                page.ClearChildren();

            BuildButtonPage(pages[0].ContentPanel);
            BuildTextPage(pages[1].ContentPanel);
            BuildGlowPage(pages[2].ContentPanel);
            BuildInnerPage(pages[3].ContentPanel);
            BuildBordersPage(pages[4].ContentPanel);
            BuildLightsPage(pages[5].ContentPanel);
            BuildMotionPage(pages[6].ContentPanel);
        }

        private void BuildButtonPage(Panel page)
        {
            row = 0;
            var presetIndex = Math.Max(0, IndexOf(ButtonEffectPresets.Names, presetName));
            Choice(page, "Preset", new List<string>(ButtonEffectPresets.Names), presetIndex, index => ApplyPreset(ButtonEffectPresets.Names[index]));
            Check(page, "Animations", animations, value => { animations = value; SetAnimations(value); });
            Slider(page, "Light strength", 0, 2, lightStrength, value => { lightStrength = value; ApplyLightStrength(); }, 0.05f, "{0:0.0}x");
            Slider(page, "Width", 40, 440, width, value => { width = value; ApplyButtonLayout(); }, 2, "{0:0}");
            Slider(page, "Height", 24, 180, height, value => { height = value; ApplyButtonLayout(); }, 2, "{0:0}");
            Slider(page, "Corner radius", 0, 90, cornerRadius, value => { cornerRadius = value; ApplyButtonLayout(); }, 1, "{0:0}");
            Colors(page, "Background", BackgroundColors, color => { backgroundColor = color; ApplyButtonLayout(); });
            Colors(page, "Stage", StageColors, color => stageBackground.Color = color);
            Check(page, "Textures", useTextures, value => { useTextures = value; ApplyButtonLayout(); });
            Slider(page, "Scale", 0.5f, 2.5f, scale, value => { scale = value; ApplyButtonLayout(); }, 0.05f, "{0:0.00}");
            Slider(page, "Rotation", 0, 360, rotation, value => { rotation = value; ApplyButtonLayout(); }, 1, "{0:0}");
            Slider(page, "Opacity", 0, 1, opacity, value => { opacity = value; ApplyButtonLayout(); }, 0.05f, "{0:0.00}");
            Check(page, "Enabled", isEnabled, value => { isEnabled = value; ApplyButtonLayout(); });
            Slider(page, "Effect padding", 0, 20, padding, value => { padding = value; ApplyButtonLayout(); }, 1, "{0:0}");
            Slider(page, "Game speed", 0, 2, ServiceProvider.Clock.TimeScale, value => ServiceProvider.Clock.TimeScale = value, 0.1f, "{0:0.0}x");
            Check(page, "Unscaled time", unscaledTime, value => { unscaledTime = value; ApplyButtonLayout(); });
        }

        private void BuildGlowPage(Panel page)
        {
            row = 0;
            var outer = GetGlow(GlowPlacement.Outer);
            Check(page, "Outer glow", outer.IsEnabled, value => outer.IsEnabled = value);
            Slider(page, "   Radius", 0, 60, outer.Radius, value => outer.Radius = value, 1, "{0:0}");
            Slider(page, "   Falloff", 0.5f, 5, outer.Falloff, value => outer.Falloff = value, 0.1f, "{0:0.0}");
            IntensitySlider(page, outer, 2);
            Slider(page, "   Pulse", 0, 4, outer.PulseSpeed, value => { outer.PulseSpeed = value; outer.PulseAmount = Math.Max(outer.PulseAmount, 0.4f); }, 0.1f, "{0:0.0}/s");
            ColorRows(page, outer, LightColors);

            var inner = GetGlow(GlowPlacement.Inner);
            Check(page, "Inner glow", inner.IsEnabled, value => inner.IsEnabled = value);
            Choice(page, "   Edges", GlowEdgeNames, (int)inner.Edges, index => inner.Edges = (GlowEdges)index);
            Slider(page, "   Radius", 0, 60, inner.Radius, value => inner.Radius = value, 1, "{0:0}");
            Slider(page, "   Falloff", 0.5f, 5, inner.Falloff, value => inner.Falloff = value, 0.1f, "{0:0.0}");
            IntensitySlider(page, inner, 2);
            ColorRows(page, inner, LightColors);

            var rim = GetGlow(GlowPlacement.Rim);
            Check(page, "Rim light", rim.IsEnabled, value => rim.IsEnabled = value);
            Slider(page, "   Thickness", 0.5f, 10, rim.Thickness, value => rim.Thickness = value, 0.5f, "{0:0.0}");
            Slider(page, "   Softness", 0, 8, rim.Softness, value => rim.Softness = value, 0.5f, "{0:0.0}");
            Slider(page, "   Offset", -12, 6, rim.Offset, value => rim.Offset = value, 0.5f, "{0:0.0}");
            IntensitySlider(page, rim, 2);
            ColorRows(page, rim, LightColors);
        }

        private void BuildInnerPage(Panel page)
        {
            row = 0;
            var highlight = GetEffect(() => new HighlightEffect());
            Check(page, "Highlight", highlight.IsEnabled, value => highlight.IsEnabled = value);
            Choice(page, "   Style", HighlightStyleNames, (int)highlight.Style, index => highlight.Style = (HighlightStyle)index);
            Slider(page, "   Side inset", 0, 40, highlight.Inset, value => highlight.Inset = value, 1, "{0:0}");
            Slider(page, "   Top inset", 0, 30, highlight.TopInset, value => highlight.TopInset = value, 1, "{0:0}");
            Slider(page, "   Height", 0.05f, 1, highlight.HeightRate, value => highlight.HeightRate = value, 0.01f, "{0:0.00}");
            Slider(page, "   Strip thickness", 1, 12, highlight.Thickness, value => highlight.Thickness = value, 0.5f, "{0:0.0}");
            Slider(page, "   Softness", 0, 0.5f, highlight.Softness, value => highlight.Softness = value, 0.01f, "{0:0.00}");
            Slider(page, "   Fade", 0, 1, highlight.Fade, value => highlight.Fade = value, 0.05f, "{0:0.00}");
            Slider(page, "   Roundness", 0, 1, highlight.Roundness, value => highlight.Roundness = value, 0.05f, "{0:0.00}");
            Choice(page, "   Blend", BlendNames, (int)highlight.Blend, index => highlight.Blend = (ButtonEffectBlend)index);
            IntensitySlider(page, highlight, 1.5f);

            var glint = GetEffect(() => new GlintEffect());
            Check(page, "Glints", glint.IsEnabled, value => glint.IsEnabled = value);
            Choice(page, "   Corners", GlintCornerNames, FindAnchorChoice(glint.Corners, GlintCorners), index => glint.Corners = new List<Anchor>(GlintCorners[index]));
            Slider(page, "   Dots", 1, 3, glint.Count, value => glint.Count = (int)value, 1, "{0:0}");
            Slider(page, "   Size", 2, 20, glint.Size, value => glint.Size = value, 0.5f, "{0:0.0}");
            Slider(page, "   Stretch", 1, 3, glint.Stretch, value => glint.Stretch = value, 0.1f, "{0:0.0}");
            Slider(page, "   Angle", -60, 60, glint.AngleDegrees, value => glint.AngleDegrees = value, 1, "{0:0}");
            Slider(page, "   Inset", 2, 30, glint.Inset, value => glint.Inset = value, 1, "{0:0}");
            Choice(page, "   Shape", ShapeNames, (int)glint.Shape, index => glint.Shape = (LightShape)index);

            var shade = GetEffect(() => new InnerShadeEffect());
            Check(page, "Inner shade", shade.IsEnabled, value => shade.IsEnabled = value);
            Choice(page, "   Where", ShadePlacementNames, (int)shade.Placement, index => shade.Placement = (ShadePlacement)index);
            Slider(page, "   Height", 0.05f, 1, shade.HeightRate, value => shade.HeightRate = value, 0.01f, "{0:0.00}");
            Slider(page, "   Edge radius", 0, 40, shade.Radius, value => shade.Radius = value, 1, "{0:0}");
            Slider(page, "   Darkness", 0, 1, shade.Intensity, value => shade.Intensity = value, 0.01f, "{0:0.00}");

            var gloss = GetEffect(() => new GlossEffect());
            Check(page, "Gloss", gloss.IsEnabled, value => gloss.IsEnabled = value);
            Slider(page, "   Height", 0, 1, gloss.HeightRate, value => gloss.HeightRate = value, 0.05f, "{0:0.00}");
            IntensitySlider(page, gloss, 1.5f);
            Check(page, "   From bottom", gloss.FromBottom, value => gloss.FromBottom = value);

            var fill = GetEffect(() => new LightFillEffect());
            Check(page, "Light fill", fill.IsEnabled, value => fill.IsEnabled = value);
            IntensitySlider(page, fill, 1);
            Check(page, "   Only on hover", fill.NormalIntensity == 0f, value =>
            {
                if (value)
                    fill.VisibleOnHover();
                else
                    fill.SetStateIntensities(1f, 1.25f, 1.1f);
            });
        }

        private void BuildBordersPage(Panel page)
        {
            row = 0;
            var frame = GetEffect(FrameEffect.Gold);
            Check(page, "Frame (levels)", frame.IsEnabled, value => frame.IsEnabled = value);
            Choice(page, "   Style", FrameStyleNames, 0, index => frame.Layers = FrameStyles[index]().Layers);
            Choice(page, "   Position", PositionNames, (int)frame.Position, index => frame.Position = (OutlinePosition)index);
            Slider(page, "   Thickness", 0.25f, 3, frame.ThicknessScale, value => frame.ThicknessScale = value, 0.05f, "{0:0.00}x");
            Slider(page, "   Gap", 0, 10, frame.Offset, value => frame.Offset = value, 0.5f, "{0:0.0}");
            Slider(page, "   Opacity", 0, 1, frame.Intensity, value => frame.Intensity = value, 0.05f, "{0:0.00}");
            var outside = GetOutline(OutlinePosition.Outside);
            Check(page, "Outline", outside.IsEnabled, value => outside.IsEnabled = value);
            Slider(page, "   Thickness", 0.5f, 12, outside.Thickness, value => outside.Thickness = value, 0.5f, "{0:0.0}");
            Slider(page, "   Gap", 0, 10, outside.Offset, value => outside.Offset = value, 0.5f, "{0:0.0}");
            ColorRows(page, outside, PaintColors);

            var inside = GetOutline(OutlinePosition.Inside);
            Check(page, "Inner line", inside.IsEnabled, value => inside.IsEnabled = value);
            Slider(page, "   Thickness", 0.5f, 8, inside.Thickness, value => inside.Thickness = value, 0.5f, "{0:0.0}");
            Slider(page, "   Inset", 0, 16, inside.Offset, value => inside.Offset = value, 0.5f, "{0:0.0}");
            Slider(page, "   Opacity", 0, 1, inside.Intensity, value => inside.Intensity = value, 0.05f, "{0:0.00}");
            ColorRows(page, inside, PaintColors);

            var depth = GetEffect(() => new DepthEffect());
            Check(page, "3D lip", depth.IsEnabled, value => depth.IsEnabled = value);
            Slider(page, "   Depth", 1, 16, depth.Depth, value => depth.Depth = value, 0.5f, "{0:0.0}");
            Slider(page, "   Pressed depth", 0, 1, depth.PressedDepthRate, value => depth.PressedDepthRate = value, 0.05f, "{0:0.00}");
            ColorRows(page, depth, PaintColors);

            var shadow = GetEffect(() => new DropShadowEffect());
            Check(page, "Drop shadow", shadow.IsEnabled, value => shadow.IsEnabled = value);
            Slider(page, "   Offset", -10, 20, shadow.Offset.Y, value => shadow.Offset = new Vector2(shadow.Offset.X, value), 0.5f, "{0:0.0}");
            Slider(page, "   Blur", 0, 30, shadow.Blur, value => shadow.Blur = value, 1, "{0:0}");
            Slider(page, "   Spread", -10, 10, shadow.Spread, value => shadow.Spread = value, 0.5f, "{0:0.0}");
            Slider(page, "   Darkness", 0, 1, shadow.Intensity, value => shadow.Intensity = value, 0.05f, "{0:0.00}");
        }

        private void BuildTextPage(Panel page)
        {
            row = 0;
            Choice(page, "Text", Texts, textIndex, index => { textIndex = index; customText = null; ApplyButtonLayout(); });
            Slider(page, "Size", 0.5f, 1.5f, textSize, value => { textSize = value; ApplyButtonLayout(); }, 0.05f, "{0:0.00}x");
            Colors(page, "Color", TextColors, color => { textColor = color; ApplyButtonLayout(); });

            // The outline of the label itself (Label.SetOutline): in font pixels, it grows with the scale of the text.
            Check(page, "Border", textBorder, value => { textBorder = value; ApplyButtonLayout(); });
            Slider(page, "   Thickness", 0.5f, 8, textBorderThickness, value => { textBorderThickness = value; ApplyButtonLayout(); }, 0.5f, "{0:0.0}");
            Colors(page, "   Color", TextBorderColors, color => { textBorderColor = color; ApplyButtonLayout(); });

            // The effect painted under the text (TextOutlineEffect): an outline in screen pixels and a shadow.
            var outline = GetEffect(() => new TextOutlineEffect());
            Check(page, "Outline effect", outline.IsEnabled, value => outline.IsEnabled = value);
            Slider(page, "   Thickness", 0, 5, outline.Thickness, value => outline.Thickness = value, 0.5f, "{0:0.0}");
            Slider(page, "   Shadow", 0, 8, outline.ShadowOffset.Y, value => outline.ShadowOffset = new Vector2(0, value), 0.5f, "{0:0.0}");
            Slider(page, "   Shadow darkness", 0, 1, outline.ShadowColor.A / 255f, value => outline.ShadowColor = Color.Black * value, 0.05f, "{0:0.00}");
            ColorRows(page, outline, PaintColors);
        }

        private void BuildLightsPage(Panel page)
        {
            row = 0;
            ParticleFieldRows(page, "Particles 1", GetEffectAt(0, () => new ParticleFieldEffect(ParticleFieldArea.Bottom).SetCount(20).SetSeed(11)));
            ParticleFieldRows(page, "Particles 2", GetEffectAt(1, () => new ParticleFieldEffect().SetShapes(LightShape.Flare, LightShape.Circle).SetCount(10).SetSeed(3)));
            var corners = GetEffect(() => new CornerLightEffect());
            Check(page, "Corner lights", corners.IsEnabled, value => corners.IsEnabled = value);
            Choice(page, "   Where", AnchorChoiceNames, FindAnchorChoice(corners.Anchors, AnchorChoices), index => corners.Anchors = new List<Anchor>(AnchorChoices[index]));
            Choice(page, "   Shape", ShapeNames, (int)corners.Shape, index => corners.Shape = (LightShape)index);
            Slider(page, "   Size", 0, 40, corners.Size, value => corners.Size = value, 1, "{0:0}");
            Slider(page, "   Glow size", 0, 100, corners.GlowSize, value => corners.GlowSize = value, 1, "{0:0}");
            Slider(page, "   Streak", 0, 220, corners.StreakLength, value => corners.StreakLength = value, 2, "{0:0}");
            Slider(page, "   Twinkle", 0, 4, corners.TwinkleSpeed, value => corners.TwinkleSpeed = value, 0.1f, "{0:0.0}/s");
            Slider(page, "   Twinkle amount", 0, 1, corners.TwinkleAmount, value => corners.TwinkleAmount = value, 0.05f, "{0:0.00}");
            IntensitySlider(page, corners, 2);
            ColorRows(page, corners, LightColors);

            var running = GetEffect(() => new RunningLightEffect());
            Check(page, "Running lights", running.IsEnabled, value => running.IsEnabled = value);
            Slider(page, "   Count", 1, 8, running.Count, value => running.Count = (int)value, 1, "{0:0}");
            Slider(page, "   Speed", 0, 700, running.Speed, value => running.Speed = value, 5, "{0:0}");
            Slider(page, "   Size", 2, 30, running.Size, value => running.Size = value, 1, "{0:0}");
            Slider(page, "   Tail", 0, 250, running.TailLength, value => running.TailLength = value, 2, "{0:0}");
            Check(page, "   Reverse", running.Reverse, value => running.Reverse = value);
            ColorRows(page, running, LightColors);
        }

        private void ParticleFieldRows(Panel page, string title, ParticleFieldEffect field)
        {
            Check(page, title, field.IsEnabled, value => field.IsEnabled = value);
            Choice(page, "   Where", FieldAreaNames, (int)field.Area, index => field.Area = (ParticleFieldArea)index);
            Choice(page, "   Shapes", FieldShapeNames, 0, index => field.Shapes = new List<LightShape>(FieldShapes[index]));
            Slider(page, "   Count", 0, 120, field.Count, value => field.Count = (int)value, 1, "{0:0}");
            Slider(page, "   Smallest", 1, 30, field.SizeMin, value => field.SizeMin = Math.Min(value, field.SizeMax), 0.5f, "{0:0.0}");
            Slider(page, "   Biggest", 1, 40, field.SizeMax, value => field.SizeMax = Math.Max(value, field.SizeMin), 0.5f, "{0:0.0}");
            Slider(page, "   Pattern", 1, 60, field.Seed, value => field.Seed = (int)value, 1, "{0:0}");
            Slider(page, "   Spread", 2, 40, field.Spread, value => field.Spread = value, 1, "{0:0}");
            Slider(page, "   Faintest", 0, 1, field.OpacityMin, value => field.OpacityMin = value, 0.05f, "{0:0.00}");
            Slider(page, "   White core", 0, 0.6f, field.CoreRate, value => field.CoreRate = value, 0.05f, "{0:0.00}");
            Slider(page, "   Twinkle", 0, 4, field.TwinkleSpeed, value => field.TwinkleSpeed = value, 0.1f, "{0:0.0}/s");
            Slider(page, "   Float up", 0, 60, field.DriftSpeed, value => field.DriftSpeed = value, 1, "{0:0}");
            IntensitySlider(page, field, 2);
            ColorRows(page, field, LightColors);
        }

        private void BuildMotionPage(Panel page)
        {
            row = 0;
            var sweep = GetEffect(() => new ShineSweepEffect());
            Check(page, "Shine sweep", sweep.IsEnabled, value => sweep.IsEnabled = value);
            Slider(page, "   Angle", -60, 60, sweep.AngleDegrees, value => sweep.AngleDegrees = value, 1, "{0:0}");
            Slider(page, "   Width", 6, 140, sweep.Width, value => sweep.Width = value, 1, "{0:0}");
            Slider(page, "   Duration", 0.2f, 2.5f, sweep.Duration, value => sweep.Duration = value, 0.05f, "{0:0.00} s");
            Slider(page, "   Interval", -1, 6, sweep.Interval, value => sweep.Interval = value, 0.1f, "{0:0.0} s");
            Check(page, "   Reverse", sweep.Reverse, value => sweep.Reverse = value);
            Check(page, "   Sweep on hover", sweep.SweepOnHover, value => sweep.SweepOnHover = value);

            var sparkles = GetEffect(() => new SparkleEffect());
            Check(page, "Sparkles", sparkles.IsEnabled, value => sparkles.IsEnabled = value);
            Slider(page, "   Rate", 0, 40, sparkles.Rate, value => sparkles.Rate = value, 1, "{0:0}/s");
            Choice(page, "   Area", AreaNames, (int)sparkles.Area, index => sparkles.Area = (SparkleArea)index);
            Slider(page, "   Spread", 0, 30, sparkles.Spread, value => sparkles.Spread = value, 1, "{0:0}");
            Choice(page, "   Shape", ShapeNames, (int)sparkles.Shape, index => sparkles.Shape = (LightShape)index);
            Slider(page, "   Size", 2, 30, sparkles.Size, value => sparkles.Size = value, 1, "{0:0}");
            Slider(page, "   Lifetime", 0.2f, 3, sparkles.Settings.LifetimeMax, value => sparkles.SetLifetime(value * 0.5f, value), 0.1f, "{0:0.0} s");
            Slider(page, "   Rise", -150, 150, -sparkles.Gravity.Y, value => sparkles.Gravity = new Vector2(0, -value), 5, "{0:0}");
            Slider(page, "   Click burst", 0, 40, sparkles.ClickBurst, value => sparkles.ClickBurst = (int)value, 1, "{0:0}");
            ColorRows(page, sparkles, LightColors);

            var rays = GetEffect(() => new LightRaysEffect());
            Check(page, "Light rays", rays.IsEnabled, value => rays.IsEnabled = value);
            Slider(page, "   Count", 2, 24, rays.Count, value => rays.Count = (int)value, 1, "{0:0}");
            Slider(page, "   Length", 0, 300, rays.Length, value => rays.Length = value, 5, "{0:0}");
            Slider(page, "   Width", 2, 40, rays.Width, value => rays.Width = value, 1, "{0:0}");
            Slider(page, "   Rotation", -90, 90, rays.RotationSpeed, value => rays.RotationSpeed = value, 1, "{0:0}/s");
            Choice(page, "   From", RayAnchorNames, Math.Max(0, Array.IndexOf(RayAnchors, rays.Anchor)), index => rays.Anchor = RayAnchors[index]);
            ColorRows(page, rays, LightColors);

            var ripple = GetEffect(() => new ClickRippleEffect());
            Check(page, "Click ripple", ripple.IsEnabled, value => ripple.IsEnabled = value);
            Slider(page, "   Duration", 0.2f, 2, ripple.Duration, value => ripple.Duration = value, 0.05f, "{0:0.00} s");
            Slider(page, "   Flash", 0, 1, ripple.FlashIntensity, value => ripple.FlashIntensity = value, 0.05f, "{0:0.00}");
            ColorRows(page, ripple, LightColors);
        }

        // ---- Animations and light strength

        /// <summary>Stops (or restores) every pulse and the effects that move: sweeps, running lights, sparkles and rays.</summary>
        private void SetAnimations(bool enabled)
        {
            if (enabled)
            {
                foreach (var pair in pausedPulses)
                    pair.Key.PulseAmount = pair.Value;
                foreach (var effect in pausedEffects)
                    effect.IsEnabled = true;
                pausedPulses.Clear();
                pausedEffects.Clear();
                RebuildPages();
                return;
            }

            foreach (var effect in preview.Effects)
            {
                if (effect.PulseAmount > 0f && !pausedPulses.ContainsKey(effect))
                {
                    pausedPulses[effect] = effect.PulseAmount;
                    effect.PulseAmount = 0f;
                }

                if (effect.IsEnabled && Array.IndexOf(AnimatedEffects, effect.GetType()) >= 0)
                {
                    effect.IsEnabled = false;
                    pausedEffects.Add(effect);
                }
            }

            RebuildPages();
        }

        /// <summary>Multiplies the intensity of every light (not the painted effects) by the light strength.</summary>
        private void ApplyLightStrength()
        {
            foreach (var effect in preview.Effects)
            {
                if (effect.Blend == ButtonEffectBlend.Light && baseIntensities.TryGetValue(effect, out var intensity))
                    effect.Intensity = intensity * lightStrength;
            }
        }

        // ---- Random

        /// <summary>Gives the preview a random combination of animated lights.</summary>
        private void Randomize()
        {
            var random = ServiceProvider.Random;
            Color RandomLight() => LightColors[random.Next(1, LightColors.Count - 1)];
            var main = RandomLight();
            var effects = new List<ButtonEffect>();

            if (random.Chance(0.15f))
                effects.Add(new LightRaysEffect().SetColor(main).SetRays(random.Next(6, 14), 0, random.NextFloat(8, 20)).SetRotationSpeed(random.NextFloat(-25, 25)));
            if (random.Chance(0.85f))
                effects.Add(new GlowEffect().SetColor(main).SetRadius(random.NextFloat(8, 26)).SetFalloff(random.NextFloat(1.2f, 3.5f))
                    .SetPulse(random.Chance(0.6f) ? random.NextFloat(0.4f, 2.2f) : 0f, random.NextFloat(0.2f, 0.7f)));
            if (random.Chance(0.4f))
                effects.Add(new GlowEffect(GlowPlacement.Inner).SetColor(RandomLight()).SetRadius(random.NextFloat(6, 18)).SetIntensity(random.NextFloat(0.3f, 0.8f)));
            if (random.Chance(0.6f))
                effects.Add(new GlossEffect().SetHeightRate(random.NextFloat(0.3f, 0.6f)).SetIntensity(random.NextFloat(0.2f, 0.55f)));
            if (random.Chance(0.6f))
                effects.Add(new ShineSweepEffect().SetAngle(random.NextFloat(-35, 35)).SetWidth(random.NextFloat(16, 60))
                    .SetTiming(random.NextFloat(0.4f, 1f), random.NextFloat(1f, 4f)).SetReverse(random.NextBool()));
            if (random.Chance(0.3f))
                effects.Add(new ClickRippleEffect().SetColor(RandomLight()));
            if (random.Chance(0.6f))
                effects.Add(new GlowEffect(GlowPlacement.Rim).SetColor(RandomLight()).SetThickness(random.NextFloat(1, 4)).SetOffset(random.NextFloat(-5, 0)));
            if (random.Chance(0.55f))
                effects.Add(new CornerLightEffect().SetAnchors(random.Pick(AnchorChoices)).SetShape(random.Chance(0.5f) ? LightShape.Flare : LightShape.Diamond)
                    .SetColor(main).SetSize(random.NextFloat(8, 18), random.NextFloat(20, 50)).SetStreak(random.Chance(0.4f) ? random.NextFloat(60, 160) : 0f));
            if (random.Chance(0.45f))
                effects.Add(new RunningLightEffect().SetColor(RandomLight()).SetCount(random.Next(1, 4)).SetSpeed(random.NextFloat(100, 400)).SetReverse(random.NextBool()));
            if (random.Chance(0.6f))
                effects.Add(new SparkleEffect().SetColor(RandomLight()).SetArea((SparkleArea)random.Next(0, AreaNames.Length - 1), random.NextFloat(4, 14))
                    .SetShape(random.Chance(0.7f) ? LightShape.Flare : LightShape.Star).SetRate(random.NextFloat(3, 14)).SetClickBurst(random.Next(0, 20)));
            if (random.Chance(0.15f))
            {
                foreach (var effect in effects)
                {
                    if (effect is GlowEffect || effect is RunningLightEffect || effect is SparkleEffect)
                        effect.SetColorCycleOf(ButtonEffectPresets.RainbowColors);
                }
            }

            backgroundColor = random.Pick(BackgroundColors);
            cornerRadius = random.Chance(0.25f) ? 0f : random.NextFloat(6, Math.Min(width, height) / 2f);
            SetPreviewEffects(effects);
        }

        /// <summary>Gives the preview a random combination of static effects: highlights, glints, shades, borders, lips and shadows.</summary>
        private void RandomizeCalm()
        {
            var random = ServiceProvider.Random;
            var effects = new List<ButtonEffect>();

            if (random.Chance(0.6f))
                effects.Add(new DropShadowEffect().SetOffset(new Vector2(0, random.NextFloat(2, 8))).SetBlur(random.NextFloat(4, 14)).SetIntensity(random.NextFloat(0.2f, 0.45f)));
            if (random.Chance(0.5f))
                effects.Add(new DepthEffect().SetDepth(random.NextFloat(3, 8)).UseButtonColor(random.NextFloat(0.35f, 0.65f)));
            if (random.Chance(0.3f))
                effects.Add(new GlowEffect().UseButtonColor(-0.4f).SetRadius(random.NextFloat(6, 16)).SetIntensity(random.NextFloat(0.15f, 0.35f)));
            if (random.Chance(0.7f))
            {
                var outline = new OutlineEffect().SetThickness(random.NextFloat(1.5f, 4));
                if (random.Chance(0.5f))
                    outline.SetColor(Color.White);
                else
                    outline.UseButtonColor(random.NextFloat(0.4f, 0.75f));
                effects.Add(outline);
            }
            if (random.Chance(0.7f))
                effects.Add(new InnerShadeEffect().SetHeightRate(random.NextFloat(0.3f, 0.65f)).SetIntensity(random.NextFloat(0.1f, 0.3f)));
            if (random.Chance(0.25f))
                effects.Add(new InnerShadeEffect(ShadePlacement.Edges).SetRadius(random.NextFloat(6, 14)).SetIntensity(random.NextFloat(0.15f, 0.3f)));
            if (random.Chance(0.4f))
                effects.Add(new GlowEffect(GlowPlacement.Inner).SetEdges((GlowEdges)random.Next(0, 2)).UseButtonColor(-0.6f).SetRadius(random.NextFloat(8, 22)).SetIntensity(random.NextFloat(0.25f, 0.5f)));
            if (random.Chance(0.3f))
                effects.Add(new OutlineEffect(OutlinePosition.Inside).SetColor(Color.White * 0.5f).SetOffset(random.NextFloat(2, 5)).SetThickness(random.NextFloat(1, 2)));
            effects.Add(new HighlightEffect((HighlightStyle)random.Next(0, 2)).SetInset(random.NextFloat(4, 14), random.NextFloat(2, 6))
                .SetHeightRate(random.NextFloat(0.3f, 0.5f)).SetThickness(random.NextFloat(2, 5)).SetSoftness(random.NextFloat(0, 0.3f)).SetFade(random.NextFloat(0.2f, 0.8f)).SetIntensity(random.NextFloat(0.25f, 0.45f)));
            if (random.Chance(0.6f))
                effects.Add(new GlintEffect().SetCorners(random.Pick(GlintCorners)).SetDots(random.Next(1, 3), random.NextFloat(4, 9)));
            if (random.Chance(0.35f))
                effects.Add(FrameStyles[random.Next(0, FrameStyles.Length - 1)]());
            if (random.Chance(0.5f))
                effects.Add(new ParticleFieldEffect((ParticleFieldArea)random.Next(0, FieldAreaNames.Length - 1)).SetShapes(random.Pick(FieldShapes))
                    .UseButtonColor(-0.7f).SetCount(random.Next(6, 30)).SetSize(random.NextFloat(2, 4), random.NextFloat(6, 14)).SetSeed(random.Next(1, 60)));
            if (random.Chance(0.5f))
                effects.Add(new TextOutlineEffect().UseButtonColor(random.NextFloat(0.5f, 0.75f)).SetOutline(Color.Black, random.NextFloat(1.5f, 3)));

            backgroundColor = random.Pick(BackgroundColors);
            cornerRadius = random.Chance(0.5f) ? height / 2f : random.NextFloat(6, Math.Min(width, height) / 2f);
            SetPreviewEffects(effects);
        }

        // ---- Helpers

        /// <summary>The first glow of a placement of the preview, or a new disabled one (so it can be enabled).</summary>
        private GlowEffect GetGlow(GlowPlacement placement)
        {
            foreach (var effect in preview.Effects)
            {
                if (effect is GlowEffect glow && glow.Placement == placement)
                    return glow;
            }

            var created = new GlowEffect(placement).SetIsEnabled(false);
            if (placement == GlowPlacement.Inner)
                created.SetIntensity(0.6f);
            preview.AddEffect(created);
            return created;
        }

        /// <summary>The first outline of a position of the preview, or a new disabled one (so it can be enabled).</summary>
        private OutlineEffect GetOutline(OutlinePosition position)
        {
            foreach (var effect in preview.Effects)
            {
                if (effect is OutlineEffect outline && outline.Position == position)
                    return outline;
            }

            var created = new OutlineEffect(position).SetIsEnabled(false);
            if (position == OutlinePosition.Inside)
                created.SetColor(Color.White).SetIntensity(0.5f).SetOffset(3).SetThickness(1.5f);
            else
                created.UseButtonColor(0.5f);
            preview.AddEffect(created);
            return created;
        }

        /// <summary>The first effect of a type of the preview, or a new disabled one (so it can be enabled).</summary>
        private TEffect GetEffect<TEffect>(Func<TEffect> create) where TEffect : ButtonEffect
        {
            var effect = preview.FindEffect<TEffect>();
            if (effect != null)
                return effect;

            effect = create();
            effect.IsEnabled = false;
            preview.AddEffect(effect);
            return effect;
        }

        /// <summary>The effect of a type at an index (0 = the first) of the preview, or a new disabled one.</summary>
        private TEffect GetEffectAt<TEffect>(int index, Func<TEffect> create) where TEffect : ButtonEffect
        {
            var found = 0;
            foreach (var effect in preview.Effects)
            {
                if (effect is TEffect typed && found++ == index)
                    return typed;
            }

            var created = create();
            created.IsEnabled = false;
            preview.AddEffect(created);
            return created;
        }

        private static int FindAnchorChoice(IList<Anchor> anchors, Anchor[][] choices)
        {
            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (anchors != null && anchors.Count == choice.Length)
                {
                    var same = true;
                    for (var j = 0; j < choice.Length && same; j++)
                        same = anchors.Contains(choice[j]);
                    if (same)
                        return i;
                }
            }

            return 0;
        }

        private static int IndexOf(IReadOnlyList<string> items, string value)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (string.Equals(items[i], value, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        private float NextRow() => row++ * RowHeight;

        private void Slider(Panel page, string text, float minimum, float maximum, float value, Action<float> onChanged, float step, string format)
            => SliderOption.CreateSliderOption(page, text, NextRow(), minimum, maximum, MathHelper.Clamp(value, minimum, maximum), onChanged, step, format);

        /// <summary>A slider for the intensity of an effect, that keeps the light strength of the page.</summary>
        private void IntensitySlider(Panel page, ButtonEffect effect, float maximum)
            => Slider(page, "   Intensity", 0, maximum, effect.Intensity, value =>
            {
                effect.Intensity = value;
                baseIntensities[effect] = effect.Blend == ButtonEffectBlend.Light && lightStrength > 0f ? value / lightStrength : value;
            }, 0.05f, "{0:0.00}");

        private void Check(Panel page, string text, bool value, Action<bool> onChanged)
            => CheckboxOption.CreateCheckboxControlOption(page, text, NextRow(), value, onChanged);

        private void Choice(Panel page, string text, IList<string> choices, int selected, Action<int> onChanged)
            => ChoiceOption.CreateChoiceOption(page, text, NextRow(), choices, selected, onChanged);

        private void Colors(Panel page, string text, List<Color> colors, Action<Color> onSelected)
            => ColorOption.CreateColorOption(page, NextRow(), onSelected, text, colors, true, 0.75f, 26, SliderOption.SliderLeft);

        /// <summary>The color rows of an effect: its own color (a palette) or a shade of the background color of the button.</summary>
        private void ColorRows(Panel page, ButtonEffect effect, List<Color> palette)
        {
            Choice(page, "   Color from", ColorSourceNames, effect.ButtonColorShade.HasValue ? 1 : 0,
                index => effect.ButtonColorShade = index == 1 ? effect.ButtonColorShade ?? 0.5f : (float?)null);
            Slider(page, "   Button shade", -1, 1, effect.ButtonColorShade ?? 0.5f, value => effect.ButtonColorShade = value, 0.05f, "{0:0.00}");
            Colors(page, "   Color", palette, color =>
            {
                effect.ButtonColorShade = null;
                effect.ColorCycle = null;
                effect.Color = color;
            });
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

        /// <summary>A section of the gallery: a title and its buttons.</summary>
        private sealed class GallerySection
        {
            public GallerySection(string title, GalleryItem[] items)
            {
                Title = title;
                Items = items;
            }

            public string Title { get; }
            public GalleryItem[] Items { get; }
        }

        /// <summary>A button of the gallery.</summary>
        private sealed class GalleryItem
        {
            private Func<List<ButtonEffect>> effects;

            public GalleryItem(string preset, string text, Vector2 size, float radius)
            {
                Preset = preset;
                Text = text;
                Size = size;
                Radius = radius;
            }

            /// <summary>The preset (also the style used by the Reset button of the preview).</summary>
            public string Preset { get; }
            public string Text { get; }
            public Vector2 Size { get; }

            /// <summary>The corner radius (a negative value = a pill).</summary>
            public float Radius { get; }
            public float Rotation { get; private set; }
            public bool Textured { get; private set; }
            public bool Toggle { get; private set; }

            /// <summary>The background color (null = the color of the preset).</summary>
            public Color? Background { get; set; }

            /// <summary>The color of the text (null = white).</summary>
            public Color? TextColor { get; private set; }

            /// <summary>The color of the border of the text.</summary>
            public Color TextBorderColor { get; private set; } = Color.Black;

            /// <summary>The thickness of the border of the text in font pixels (0 = no border).</summary>
            public float TextBorderThickness { get; private set; }

            /// <summary>The effects of the button: its own combination, or the preset.</summary>
            public List<ButtonEffect> CreateEffects() => effects != null ? effects() : ButtonEffectPresets.Create(Preset);

            public GalleryItem WithEffects(Func<List<ButtonEffect>> createEffects)
            {
                effects = createEffects;
                return this;
            }

            /// <summary>Sets the color of the text and, optionally, its border (thickness in font pixels).</summary>
            public GalleryItem WithText(Color color, Color? border = null, float borderThickness = 3f)
            {
                TextColor = color;
                if (border.HasValue)
                {
                    TextBorderColor = border.Value;
                    TextBorderThickness = borderThickness;
                }

                return this;
            }

            public GalleryItem Rotated(float degrees)
            {
                Rotation = degrees;
                return this;
            }

            public GalleryItem AsTextured()
            {
                Textured = true;
                return this;
            }

            public GalleryItem AsToggle()
            {
                Toggle = true;
                return this;
            }
        }
    }

    /// <summary>Small helpers of the demo to change effects of any type.</summary>
    internal static class ButtonEffectDemoExtensions
    {
        /// <summary>Gives a color cycle to an effect.</summary>
        public static void SetColorCycleOf(this ButtonEffect effect, IList<Color> colors)
        {
            effect.ColorCycle = colors;
            effect.ColorCyclePeriod = 4f;
        }
    }
}
