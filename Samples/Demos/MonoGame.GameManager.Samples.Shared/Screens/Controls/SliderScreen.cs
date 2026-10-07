using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls;
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
    /// Every option of the <see cref="Slider"/> on a live preview, and a few examples of sliders bound to other
    /// controls (a color mixer, a stepped volume fader and an image size).
    /// </summary>
    public class SliderScreen : Screen
    {
        private const int SectionTop = Config.ScreenContentMargin + 60;
        private const int SectionHeight = 690;
        private const int SectionDivisionLeft = 550;
        private const int OptionsWidth = SectionDivisionLeft - Config.ScreenContentMargin * 2;
        private const int PreviewLeft = SectionDivisionLeft + Config.ScreenContentMargin;
        private const int PreviewWidth = 600;
        private const int StageTop = 40;
        private const int StageHeight = 380;
        private const int ExamplesTop = 430;
        private const int RowHeight = 40;

        private static readonly Vector2 StageCenter = new Vector2(PreviewWidth / 2f, 150);
        private static readonly Color StageColor = new Color(15, 15, 15);
        private static readonly string[] Orientations = { "Horizontal", "Vertical" };
        private static readonly string[] StepNames = { "0 (smooth)", "0.5", "1", "5", "10", "25" };
        private static readonly float[] Steps = { 0f, 0.5f, 1f, 5f, 10f, 25f };

        private static readonly List<Color> TrackColors = new List<Color>
        {
            new Color(70, 70, 70), Color.DimGray, Color.SlateGray, Color.Navy, Color.Maroon, Color.DarkGreen, Color.Black, Color.White
        };

        private static readonly List<Color> FillColors = new List<Color>
        {
            new Color(80, 160, 230), Color.Orange, Color.LimeGreen, Color.DeepPink, Color.Gold, Color.Cyan, Color.Red, Color.White
        };

        private static readonly List<Color> ThumbColors = new List<Color>
        {
            Color.White, Color.Yellow, Color.Orange, Color.Red, Color.DeepPink, Color.Cyan, Color.LimeGreen, Color.Gray
        };

        private Slider preview;
        private Label valueLabel;
        private Label normalizedLabel;
        private Label draggingLabel;
        private ProgressBar mirror;
        private Texture2D starTexture;
        private Texture2D trackTexture;
        private Orientation orientation = Orientation.Horizontal;
        private float minimum;
        private float maximum = 100f;
        private float length = 260f;
        private float thickness = 28f;
        private float thumbSize = 28f;
        private float trackThickness = 8f;
        private bool useTextures;
        private float rotation;
        private float scale = 1f;

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Slider", OpenSliderScreen)
            });

            starTexture = RegisterDisposable(TextureFactory.CreateStar(ServiceProvider.GraphicsDevice, 64, Color.White));
            trackTexture = RegisterDisposable(TextureFactory.CreateRoundedRectangle(ServiceProvider.GraphicsDevice, 256, 32, 16f, new Color(40, 40, 40), 3f, Color.Silver));

            CreatePreviewSection();
            CreateOptionsSection();
            new RectangleControl(new Rectangle(SectionDivisionLeft, SectionTop, 2, SectionHeight), Color.White)
                .AddToScreen();

            base.OnInit();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            draggingLabel.IsVisible = preview.IsDragging;
        }

        public static void OpenSliderScreen()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new SliderScreen());
        }

        // ---- Options

        private void CreateOptionsSection()
        {
            var container = new Panel(new Rectangle(Config.ScreenContentMargin, SectionTop, OptionsWidth, SectionHeight))
                .AddToScreen();

            new Label(ContentHandler.Instance.SpriteFontArial, "Options", Vector2.Zero, Color.Yellow)
                .SetAnchor(Enums.Anchor.TopCenter)
                .AddToScreen(container);

            var row = 0;
            float Row() => StageTop + row++ * RowHeight;

            ChoiceOption.CreateChoiceOption(container, "Orientation", Row(), Orientations, 0, index =>
            {
                orientation = index == 0 ? Orientation.Horizontal : Orientation.Vertical;
                ApplyLayout();
            });
            SliderOption.CreateSliderOption(container, "Minimum", Row(), -100, 100, minimum, value =>
            {
                minimum = value;
                preview.SetRange(minimum, maximum);
                UpdateReadouts();
            }, 1, "{0:0}");
            SliderOption.CreateSliderOption(container, "Maximum", Row(), 0, 200, maximum, value =>
            {
                maximum = value;
                preview.SetRange(minimum, maximum);
                UpdateReadouts();
            }, 1, "{0:0}");
            ChoiceOption.CreateChoiceOption(container, "Step", Row(), StepNames, 0, index =>
            {
                preview.SetStep(Steps[index]);
                UpdateReadouts();
            });
            SliderOption.CreateSliderOption(container, "Length", Row(), 80, 260, length, value => { length = value; ApplyLayout(); }, 5, "{0:0}");
            SliderOption.CreateSliderOption(container, "Thickness", Row(), 12, 60, thickness, value => { thickness = value; ApplyLayout(); }, 1, "{0:0}");
            SliderOption.CreateSliderOption(container, "Thumb size", Row(), 8, 60, thumbSize, value => { thumbSize = value; ApplyLayout(); }, 1, "{0:0}");
            SliderOption.CreateSliderOption(container, "Track thickness", Row(), 1, 30, trackThickness, value => { trackThickness = value; ApplyLayout(); }, 1, "{0:0}");
            CheckboxOption.CreateCheckboxControlOption(container, "Round thumb", Row(), true, value => preview.IsThumbRound = value);
            CheckboxOption.CreateCheckboxControlOption(container, "Textures", Row(), false, value => { useTextures = value; ApplyLayout(); });
            CheckboxOption.CreateCheckboxControlOption(container, "Enabled", Row(), true, value => preview.IsEnabled = value);
            SliderOption.CreateSliderOption(container, "Rotation", Row(), 0, 360, rotation, value => { rotation = value; ApplyLayout(); }, 5, "{0:0}");
            SliderOption.CreateSliderOption(container, "Scale", Row(), 0.5f, 2f, scale, value => { scale = value; ApplyLayout(); }, 0.05f, "{0:0.00}");
            ColorOption.CreateColorOption(container, Row(), color => preview.TrackColor = color, "Track color", TrackColors, true, 0.75f, 26, SliderOption.SliderLeft);
            ColorOption.CreateColorOption(container, Row(), color => preview.FillColor = color, "Fill color", FillColors, true, 0.75f, 26, SliderOption.SliderLeft);
            ColorOption.CreateColorOption(container, Row(), color => preview.ThumbColor = color, "Thumb color", ThumbColors, true, 0.75f, 26, SliderOption.SliderLeft);
        }

        // ---- Preview

        private void CreatePreviewSection()
        {
            var font = ContentHandler.Instance.SpriteFontArial;
            var container = new Panel(new Rectangle(PreviewLeft, SectionTop, PreviewWidth, SectionHeight))
                .AddToScreen();

            new Label(font, "Preview", Vector2.Zero, Color.Yellow)
                .SetAnchor(Enums.Anchor.TopCenter)
                .AddToScreen(container);

            var stage = new Panel(new Rectangle(0, StageTop, PreviewWidth, StageHeight))
                .SetHideOverflow(true)
                .AddToScreen(container);
            new RectangleControl(Vector2.Zero, stage.Size, StageColor)
                .AddToScreen(stage);

            preview = new Slider(Vector2.Zero, new Vector2(length, thickness), minimum, maximum)
                .SetValue(40, notify: false)
                .AddOnValueChanged(value => UpdateReadouts())
                .AddToScreen(stage);
            ApplyLayout();

            valueLabel = new Label(font, string.Empty, new Vector2(20, 296), Color.White)
                .SetScale(0.8f)
                .AddToScreen(stage);
            normalizedLabel = new Label(font, string.Empty, new Vector2(230, 296), Color.White)
                .SetScale(0.8f)
                .AddToScreen(stage);
            draggingLabel = new Label(font, "Dragging", new Vector2(480, 296), Color.Yellow)
                .SetScale(0.8f)
                .SetIsVisible(false)
                .AddToScreen(stage);
            mirror = new ProgressBar(new Vector2(20, 330), new Vector2(560, 14))
                .AddToScreen(stage);
            new Label(font, "A ProgressBar bound to the value", new Vector2(20, 350), Color.Gray)
                .SetScale(0.6f)
                .AddToScreen(stage);
            UpdateReadouts();

            CreateExamples(container);
        }

        /// <summary>Applies the size, orientation, thumb, track, textures, rotation and scale options to the preview.</summary>
        private void ApplyLayout()
        {
            preview.Size = orientation == Orientation.Horizontal ? new Vector2(length, thickness) : new Vector2(thickness, length);
            preview.SetOrientation(orientation)
                .SetThumbSize(new Vector2(thumbSize))
                .SetTrackThickness(trackThickness)
                .SetThumbTexture(useTextures ? starTexture : null)
                .SetTrackTexture(useTextures ? trackTexture : null);
            preview.SetOriginRate(new Vector2(0.5f))
                .SetPosition(StageCenter)
                .SetRotationInDegree(rotation)
                .SetScale(scale);
        }

        private void UpdateReadouts()
        {
            if (valueLabel == null)
                return;

            valueLabel.Text = $"Value: {preview.Value:0.##}";
            normalizedLabel.Text = $"Normalized: {preview.NormalizedValue:0.00}";
            mirror.SetValue(preview.NormalizedValue);
        }

        // ---- Examples

        private void CreateExamples(Panel container)
        {
            var font = ContentHandler.Instance.SpriteFontArial;
            var examples = new Panel(new Rectangle(0, ExamplesTop, PreviewWidth, SectionHeight - ExamplesTop))
                .AddToScreen(container);
            new RectangleControl(Vector2.Zero, examples.Size, StageColor)
                .AddToScreen(examples);
            new Label(font, "Examples", new Vector2(12, 8), Color.Yellow)
                .SetScale(0.8f)
                .AddToScreen(examples);

            CreateColorMixer(examples);
            CreateVolumeFader(examples);
            CreateImageSize(examples);
        }

        private void CreateColorMixer(Panel examples)
        {
            var font = ContentHandler.Instance.SpriteFontArial;
            new Label(font, "Color mixer", new Vector2(16, 44), Color.Gray)
                .SetScale(0.65f)
                .AddToScreen(examples);

            var channels = new[] { 230f, 120f, 40f };
            var swatch = new Panel(new Rectangle(16, 192, 236, 50))
                .SetBorder(Color.White, 2)
                .AddToScreen(examples);
            var hexLabel = new Label(font, string.Empty, Vector2.Zero, Color.Black)
                .SetScale(0.75f)
                .SetAnchor(Enums.Anchor.Center)
                .AddToScreen(swatch);

            void UpdateSwatch()
            {
                var color = new Color((int)channels[0], (int)channels[1], (int)channels[2]);
                swatch.SetBackgroundColor(color);
                hexLabel.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                hexLabel.Color = color.R * 0.3f + color.G * 0.59f + color.B * 0.11f > 140 ? Color.Black : Color.White;
            }

            var names = new[] { "R", "G", "B" };
            var fills = new[] { new Color(220, 60, 60), new Color(60, 190, 80), new Color(70, 120, 230) };
            for (var i = 0; i < 3; i++)
            {
                var channel = i;
                var y = 72 + i * 38;
                new Label(font, names[i], new Vector2(16, y + 2), Color.White)
                    .SetScale(0.75f)
                    .AddToScreen(examples);
                var valueText = new Label(font, ((int)channels[i]).ToString(), new Vector2(222, y + 3), Color.White)
                    .SetScale(0.7f)
                    .AddToScreen(examples);
                new Slider(new Vector2(44, y), new Vector2(170, 24), 0, 255)
                    .SetStep(1)
                    .SetColors(new Color(55, 55, 55), fills[i], Color.White)
                    .SetValue(channels[i], notify: false)
                    .AddOnValueChanged(value =>
                    {
                        channels[channel] = value;
                        valueText.Text = ((int)value).ToString();
                        UpdateSwatch();
                    })
                    .AddToScreen(examples);
            }

            UpdateSwatch();
        }

        private void CreateVolumeFader(Panel examples)
        {
            var font = ContentHandler.Instance.SpriteFontArial;
            new Label(font, "Stepped volume", new Vector2(268, 44), Color.Gray)
                .SetScale(0.65f)
                .AddToScreen(examples);

            var valueText = new Label(font, "70%", new Vector2(300, 232), Color.White)
                .SetScale(0.75f)
                .AddToScreen(examples);
            new Slider(new Vector2(306, 72), new Vector2(30, 150), 0, 100)
                .SetOrientation(Orientation.Vertical)
                .SetStep(10)
                .SetColors(new Color(55, 55, 55), Color.Orange, Color.White)
                .SetTrackThickness(12)
                .SetValue(70, notify: false)
                .AddOnValueChanged(value => valueText.Text = $"{value:0}%")
                .AddToScreen(examples);
        }

        private void CreateImageSize(Panel examples)
        {
            var font = ContentHandler.Instance.SpriteFontArial;
            var title = new Label(font, "Image size: 1.00x", new Vector2(412, 44), Color.Gray)
                .SetScale(0.65f)
                .AddToScreen(examples);

            var image = new Image(ContentHandler.Instance.TextureImage)
                .SetOriginRate(new Vector2(0.5f))
                .SetPosition(500, 142)
                .AddToScreen(examples);

            new Slider(new Vector2(412, 230), new Vector2(170, 24), 0.5f, 2f)
                .SetStep(0.05f)
                .SetColors(new Color(55, 55, 55), Color.MediumPurple, Color.White)
                .SetValue(1f, notify: false)
                .AddOnValueChanged(value =>
                {
                    image.SetScale(value);
                    title.Text = $"Image size: {value:0.00}x";
                })
                .AddToScreen(examples);
        }
    }
}
