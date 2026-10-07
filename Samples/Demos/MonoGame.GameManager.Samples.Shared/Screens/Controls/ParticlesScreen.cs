using Microsoft.Xna.Framework;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Particles;
using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Services.Inputs;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Samples.Screens.Controls
{
    /// <summary>
    /// The particles playground: every option of <see cref="ParticleSettings"/> can be changed live on a
    /// <see cref="ParticleEmitter"/>, starting from the examples of <see cref="ParticlePresets"/>.
    /// </summary>
    public class ParticlesScreen : Screen
    {
        private const int SectionTop = Config.ScreenContentMargin + 60;
        private const int SectionHeight = 690;
        private const int SectionDivisionLeft = 550;
        private const int OptionsWidth = SectionDivisionLeft - Config.ScreenContentMargin * 2;
        private const int PreviewLeft = SectionDivisionLeft + Config.ScreenContentMargin;
        private const int PreviewWidth = 600;
        private const int StageTop = 98;
        private const int StageHeight = 545;
        private const int RowHeight = 46;
        private const int BoundsInset = 50;
        private const float DefaultFloorOffset = 30f;

        private static readonly Vector2 StageOrigin = new Vector2(PreviewLeft, SectionTop + StageTop);
        private static readonly string[] TabNames = { "Emission", "Shape", "Motion", "Look", "Color", "Timeline" };
        private static readonly Color TabColor = new Color(60, 60, 60);
        private static readonly Color SelectedTabColor = new Color(80, 160, 230);
        private static readonly Color PresetColor = new Color(50, 70, 90);
        private static readonly Color SelectedPresetColor = new Color(230, 150, 40);
        private static readonly Color ActionColor = new Color(70, 110, 70);
        private static readonly Color BorderColor = new Color(110, 110, 110);

        private static readonly List<Color> Palette = new List<Color>
        {
            Color.White, Color.Yellow, Color.Orange, Color.Red, Color.DeepPink, Color.Cyan, Color.LimeGreen, Color.Transparent
        };

        private static readonly List<Color> TintPalette = new List<Color>
        {
            new Color(255, 80, 80), new Color(255, 200, 40), new Color(80, 220, 120), new Color(80, 160, 255), new Color(220, 90, 255)
        };

        private Panel stage;
        private Panel[] pages;
        private Button[] tabButtons;
        private Button[] presetButtons;
        private Button playStopButton;
        private Label statsLabel;
        private RectangleControl floorMarker;
        private Panel boundsMarker;
        private ParticleEmitter emitter;
        private ParticleSettings settings;
        private ParticleBurst burstTemplate = new ParticleBurst(0f, 50);
        private ParticleGradient presetGradient;
        private ParticleCurve presetScaleCurve;
        private EasingFunction presetScaleEasing;
        private ParticleCurve presetAlphaCurve;
        private ParticleSubEmitter presetDeath;
        private ParticleSubEmitter presetTrail;
        private string presetName = "Fire";
        private int selectedTab;
        private float? floorStageY;
        private bool useBounds;
        private bool followPointer;
        private float trailRate = 30f;
        private int frameCounter;

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Particles", OpenParticlesScreen)
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
            if (++frameCounter % 10 != 0 || statsLabel == null)
                return;

            var system = emitter.ParticleSystem;
            statsLabel.Text = $"Particles: {system.ActiveCount} / {settings.MaxParticles}   Sub-emitters: {system.TotalActiveCount - system.ActiveCount}   Time: {system.Time:0.0} s";
            UpdatePlayStopLabel();
        }

        public static void OpenParticlesScreen()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new ParticlesScreen());
        }

        /// <summary>Loads a preset of <see cref="ParticlePresets"/> and rebuilds the options.</summary>
        public void ApplyPreset(string name)
        {
            presetName = name;
            ApplySettings(ParticlePresets.Create(name), GetPresetPosition(name));
            for (var i = 0; i < presetButtons.Length; i++)
                presetButtons[i].BackgroundColor = ParticlePresets.Names[i] == name ? SelectedPresetColor : PresetColor;
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

        /// <summary>Emits particles at a position of the stage (what a click does).</summary>
        public void BurstAt(Vector2 stagePosition, int count = 60) => emitter.Burst(count, stagePosition + StageOrigin);

        private static Vector2 GetPresetPosition(string name)
        {
            switch (name)
            {
                case "Rain":
                case "Snow":
                    return new Vector2(PreviewWidth / 2f, 8);
                case "Fireworks":
                case "Fountain":
                    return new Vector2(PreviewWidth / 2f, StageHeight - 40);
                case "Sparks":
                    return new Vector2(PreviewWidth / 2f, StageHeight - 120);
                default:
                    return new Vector2(PreviewWidth / 2f, StageHeight / 2f);
            }
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

            presetButtons = new Button[ParticlePresets.Names.Count];
            for (var i = 0; i < presetButtons.Length; i++)
            {
                var name = ParticlePresets.Names[i];
                var position = new Vector2(i % 7 * 86, 34 + i / 7 * 30);
                presetButtons[i] = CreateSmallButton(container, name, position, new Vector2(82, 26), PresetColor, 0.55f, () => ApplyPreset(name));
            }

            stage = new Panel(new Rectangle(0, StageTop, PreviewWidth, StageHeight))
                .SetHideOverflow(true)
                .SetAcceptedMouseButtons(MouseButtons.Left | MouseButtons.Right)
                .AddOnMousePressed(OnStagePressed)
                .AddOnMouseMoved(OnStageMoved)
                .AddToScreen(container);

            new RectangleControl(Vector2.Zero, new Vector2(PreviewWidth, StageHeight), new Color(15, 15, 15))
                .AddToScreen(stage);

            floorMarker = new RectangleControl(new Rectangle(0, 0, PreviewWidth, 2), new Color(90, 90, 90))
                .SetIsVisible(false)
                .AddToScreen(stage);

            boundsMarker = new Panel(new Rectangle(BoundsInset, BoundsInset, PreviewWidth - BoundsInset * 2, StageHeight - BoundsInset * 2))
                .SetBorder(new Color(90, 90, 90), 2)
                .SetIsVisible(false)
                .AddToScreen(stage);

            emitter = new ParticleEmitter(ParticlePresets.Fire())
                .SetPosition(GetPresetPosition("Fire"))
                .AddToScreen(stage);
            settings = emitter.Settings;

            var infoTop = StageTop + StageHeight + 8;
            statsLabel = new Label(font, string.Empty, new Vector2(0, infoTop), Color.White)
                .SetScale(0.7f)
                .AddToScreen(container);

            new FpsCounter(font, new Vector2(PreviewWidth - 100, infoTop), Color.Yellow)
                .SetScale(0.7f)
                .AddToScreen(container);

            new Label(font, "Click: burst there     Right click: move the emitter", new Vector2(0, infoTop + 24), Color.Gray)
                .SetScale(0.6f)
                .AddToScreen(container);

            var follow = new Checkbox(new Vector2(PreviewWidth - 150, infoTop + 22), 20)
                .SetLabel(font, "Follow pointer", Color.Gray)
                .AddOnCheckedChanged(value => followPointer = value)
                .AddToScreen(container);
            follow.Label.SetScale(0.6f);
        }

        private void OnStagePressed(ControlMouseEventArgs args)
        {
            var stagePosition = args.Position.ToVector2() - StageOrigin;
            if (args.Button == MouseButtons.Right)
                MoveEmitter(stagePosition);
            else
                BurstAt(stagePosition);
        }

        private void OnStageMoved(ControlMouseEventArgs args)
        {
            if (followPointer)
                MoveEmitter(args.Position.ToVector2() - StageOrigin);
            args.ContinuePropagation();
        }

        private void MoveEmitter(Vector2 stagePosition)
        {
            emitter.SetPosition(stagePosition);
            ApplyFloorAndBounds();
        }

        // ---- Options

        private void CreateOptionsSection()
        {
            var font = ContentHandler.Instance.SpriteFontArial;
            var container = new Panel(new Rectangle(Config.ScreenContentMargin, SectionTop, OptionsWidth, SectionHeight))
                .AddToScreen();

            new Label(font, "Options", Vector2.Zero, Color.Yellow)
                .SetAnchor(Enums.Anchor.TopCenter)
                .AddToScreen(container);

            tabButtons = new Button[TabNames.Length];
            pages = new Panel[TabNames.Length];
            for (var i = 0; i < TabNames.Length; i++)
            {
                var index = i;
                tabButtons[i] = CreateSmallButton(container, TabNames[i], new Vector2(i * 86, 36), new Vector2(80, 32), TabColor, 0.6f, () => ShowTab(index));
                pages[i] = new Panel(new Rectangle(0, 78, OptionsWidth, 560))
                    .SetIsVisible(false)
                    .AddToScreen(container);
            }

            const int actionsTop = 648;
            var actionSize = new Vector2(80, 36);
            playStopButton = CreateSmallButton(container, "Stop", new Vector2(0, actionsTop), actionSize, ActionColor, 0.65f, TogglePlayStop);
            CreateSmallButton(container, "Burst", new Vector2(86, actionsTop), actionSize, ActionColor, 0.65f, () => emitter.Burst(100));
            CreateSmallButton(container, "Clear", new Vector2(172, actionsTop), actionSize, ActionColor, 0.65f, () => emitter.ParticleSystem.Clear());
            CreateSmallButton(container, "Random", new Vector2(258, actionsTop), actionSize, ActionColor, 0.65f, Randomize);
            CreateSmallButton(container, "Reset", new Vector2(344, actionsTop), actionSize, ActionColor, 0.65f, () => ApplyPreset(presetName));
            CreateSmallButton(container, "Scenes >", new Vector2(430, actionsTop), actionSize, SelectedPresetColor, 0.65f, ParticleScenesScreen.OpenParticleScenesScreen);
        }

        private static Button CreateSmallButton(Panel container, string text, Vector2 position, Vector2 size, Color color, float textScale, Action onClick)
        {
            var button = new Button(position, size, color)
                .SetBorder(BorderColor)
                .SetText(ContentHandler.Instance.SpriteFontArial, text, Color.White)
                .AddOnClick(args => onClick())
                .AddToScreen(container);
            button.TextLabel.SetScale(textScale);
            return button;
        }

        private void TogglePlayStop()
        {
            if (emitter.IsPlaying)
                emitter.Stop();
            else
                emitter.Play();
            UpdatePlayStopLabel();
        }

        private void UpdatePlayStopLabel()
        {
            var text = emitter.IsPlaying ? "Stop" : "Play";
            if (playStopButton.TextLabel.Text != text)
                playStopButton.SetText(text);
        }

        private void ApplySettings(ParticleSettings newSettings, Vector2 position)
        {
            settings = newSettings;
            burstTemplate = settings.Bursts.Count > 0 ? settings.Bursts[0] : new ParticleBurst(0f, 50);
            presetGradient = settings.ColorOverLifetime;
            presetScaleCurve = settings.ScaleOverLifetime;
            presetScaleEasing = settings.ScaleEasing;
            presetAlphaCurve = settings.AlphaOverLifetime;
            presetDeath = settings.OnDeath;
            presetTrail = settings.Trail;
            trailRate = settings.Trail?.Rate ?? 30f;
            floorStageY = StageHeight - DefaultFloorOffset;
            useBounds = false;

            emitter.SetColor(Color.White)
                .SetOpacity(1f)
                .SetSettings(settings)
                .SetPosition(position);
            ApplyFloorAndBounds();
            emitter.Restart();
            RebuildPages();
        }

        /// <summary>Converts the floor and the bounds box of the stage to the space of the particles.</summary>
        private void ApplyFloorAndBounds()
        {
            var local = settings.SimulationSpace == SimulationSpace.Local;
            var emitterPosition = emitter.PositionAnchor;
            Vector2 ToSimulationSpace(Vector2 stagePosition) => local ? stagePosition - emitterPosition : stagePosition + StageOrigin;

            if (floorStageY.HasValue)
            {
                settings.Floor = ToSimulationSpace(new Vector2(0, floorStageY.Value)).Y;
                floorMarker.SetPosition(0, floorStageY.Value).SetIsVisible(true);
            }
            else
            {
                settings.Floor = null;
                floorMarker.SetIsVisible(false);
            }

            if (useBounds)
            {
                var topLeft = ToSimulationSpace(new Vector2(BoundsInset, BoundsInset));
                settings.Bounds = new RectangleF(topLeft.X, topLeft.Y, PreviewWidth - BoundsInset * 2, StageHeight - BoundsInset * 2);
                boundsMarker.SetIsVisible(true);
            }
            else
            {
                settings.Bounds = null;
                boundsMarker.SetIsVisible(false);
            }
        }

        private void RebuildPages()
        {
            if (pages == null)
                return;

            foreach (var page in pages)
                page.ClearChildren();

            BuildEmissionPage(pages[0]);
            BuildShapePage(pages[1]);
            BuildMotionPage(pages[2]);
            BuildLookPage(pages[3]);
            BuildColorPage(pages[4]);
            BuildTimelinePage(pages[5]);
        }

        private static void Slider(Panel page, int row, string text, float minimum, float maximum, float value, Action<float> onChanged, float step = 0f, string format = "{0:0.##}")
            => SliderOption.CreateSliderOption(page, text, row * RowHeight, minimum, maximum, value, onChanged, step, format);

        private static void Choice(Panel page, int row, string text, string[] choices, int selected, Action<int> onChanged)
            => ChoiceOption.CreateChoiceOption(page, text, row * RowHeight, choices, selected, onChanged);

        private static void Toggle(Panel page, int row, string text, bool value, Action<bool> onChanged)
            => CheckboxOption.CreateCheckboxControlOption(page, text, row * RowHeight, value, onChanged);

        private void BuildEmissionPage(Panel page)
        {
            var s = settings;
            Slider(page, 0, "Rate per second", 0, 500, s.EmissionRate, value => s.EmissionRate = value, 1, "{0:0}");
            Slider(page, 1, "Max particles", 10, 5000, s.MaxParticles, value => s.MaxParticles = (int)value, 10, "{0:0}");
            Slider(page, 2, "Lifetime min", 0.05f, 10, s.LifetimeMin, value => s.LifetimeMin = value, 0.05f);
            Slider(page, 3, "Lifetime max", 0.05f, 10, s.LifetimeMax, value => s.LifetimeMax = value, 0.05f);
            Slider(page, 4, "Speed min", 0, 1000, s.SpeedMin, value => s.SpeedMin = value, 5, "{0:0}");
            Slider(page, 5, "Speed max", 0, 1000, s.SpeedMax, value => s.SpeedMax = value, 5, "{0:0}");
            Slider(page, 6, "Angle min", -180, 360, s.AngleMin, value => s.AngleMin = value, 5, "{0:0}");
            Slider(page, 7, "Angle max", -180, 360, s.AngleMax, value => s.AngleMax = value, 5, "{0:0}");
            Choice(page, 8, "Space", new[] { "World (stay behind)", "Local (follow emitter)" }, (int)s.SimulationSpace, index =>
            {
                s.SimulationSpace = (SimulationSpace)index;
                ApplyFloorAndBounds();
                emitter.Restart();
            });
            Slider(page, 9, "Per pixel moved", 0, 3, s.EmissionPerDistance, value => s.EmissionPerDistance = value, 0.05f);
            Slider(page, 10, "Inherit velocity", 0, 1, s.InheritVelocity, value => s.InheritVelocity = value, 0.05f);
            Slider(page, 11, "Prewarm seconds", 0, 10, s.PrewarmSeconds, value => s.PrewarmSeconds = value, 0.5f);
        }

        private void BuildShapePage(Panel page)
        {
            var s = settings;
            Choice(page, 0, "Shape", new[] { "Point", "Circle", "Ring", "Rectangle", "Line" }, (int)s.Shape, index => s.Shape = (EmitterShape)index);
            Slider(page, 1, "Radius", 0, 300, s.SpawnRadius, value => s.SpawnRadius = value, 1, "{0:0}");
            Slider(page, 2, "Inner radius", 0, 300, s.SpawnInnerRadius, value => s.SpawnInnerRadius = value, 1, "{0:0}");
            Slider(page, 3, "Width", 0, 600, s.SpawnSize.X, value => s.SpawnSize = new Vector2(value, s.SpawnSize.Y), 5, "{0:0}");
            Slider(page, 4, "Height", 0, 500, s.SpawnSize.Y, value => s.SpawnSize = new Vector2(s.SpawnSize.X, value), 5, "{0:0}");
            Slider(page, 5, "Shape rotation", 0, 360, s.SpawnRotation, value => s.SpawnRotation = value, 5, "{0:0}");
            Toggle(page, 6, "Emit from edge", s.EmitFromEdge, value => s.EmitFromEdge = value);
            Toggle(page, 7, "Radial velocity", s.RadialVelocity, value => s.RadialVelocity = value);
            Slider(page, 8, "Rotation min", 0, 360, s.RotationMin, value => s.RotationMin = value, 5, "{0:0}");
            Slider(page, 9, "Rotation max", 0, 360, s.RotationMax, value => s.RotationMax = value, 5, "{0:0}");
            Slider(page, 10, "Spin min", -720, 720, s.RotationSpeedMin, value => s.RotationSpeedMin = value, 10, "{0:0}");
            Slider(page, 11, "Spin max", -720, 720, s.RotationSpeedMax, value => s.RotationSpeedMax = value, 10, "{0:0}");
        }

        private void BuildMotionPage(Panel page)
        {
            var s = settings;
            Slider(page, 0, "Gravity X", -1000, 1000, s.Gravity.X, value => s.Gravity = new Vector2(value, s.Gravity.Y), 10, "{0:0}");
            Slider(page, 1, "Gravity Y", -1000, 1000, s.Gravity.Y, value => s.Gravity = new Vector2(s.Gravity.X, value), 10, "{0:0}");
            Slider(page, 2, "Drag", 0, 1, s.Drag, value => s.Drag = value, 0.01f);
            Slider(page, 3, "Turbulence", 0, 400, s.Turbulence, value => s.Turbulence = value, 5, "{0:0}");
            Slider(page, 4, "Turbulence speed", 0.1f, 5, s.TurbulenceFrequency, value => s.TurbulenceFrequency = value, 0.1f);
            Slider(page, 5, "Attraction", -500, 500, s.AttractionStrength, value => s.AttractionStrength = value, 10, "{0:0}");
            Slider(page, 6, "Vortex", -500, 500, s.VortexStrength, value => s.VortexStrength = value, 10, "{0:0}");
            Choice(page, 7, "Bounds mode", new[] { "None", "Kill", "Bounce" }, (int)s.BoundsMode, index => s.BoundsMode = (ParticleBoundsMode)index);
            Slider(page, 8, "Floor (0 = none)", 0, StageHeight, floorStageY ?? 0f, value =>
            {
                floorStageY = value > 0f ? value : (float?)null;
                ApplyFloorAndBounds();
            }, 5, "{0:0}");
            Toggle(page, 9, "Bounds box", useBounds, value =>
            {
                useBounds = value;
                ApplyFloorAndBounds();
            });
            Slider(page, 10, "Bounciness", 0, 1, s.Bounciness, value => s.Bounciness = value, 0.05f);
            Slider(page, 11, "Friction", 0, 1, s.Friction, value => s.Friction = value, 0.05f);
        }

        private void BuildLookPage(Panel page)
        {
            var s = settings;
            var appearances = new[] { "Square", "Circle", "Glow", "Ring", "Star", "Diamond", "Coin sprite sheet" };
            Choice(page, 0, "Appearance", appearances, s.Texture != null ? 6 : (int)s.Appearance, SetAppearance);
            Slider(page, 1, "Size", 1, 64, s.Size, value => s.Size = value, 1, "{0:0}");
            Slider(page, 2, "Start scale", 0, 4, s.StartScale, value => s.StartScale = value, 0.05f);
            Slider(page, 3, "End scale", 0, 4, s.EndScale, value => s.EndScale = value, 0.05f);
            Slider(page, 4, "Scale variation", 0, 1, s.ScaleVariation, value => s.ScaleVariation = value, 0.05f);
            var scaleCurves = new[] { "Start to end", "Preset curve", "Fade in-out", "Peak", "Back out", "Elastic out" };
            Choice(page, 5, "Scale over life", scaleCurves, s.ScaleOverLifetime != null ? 1 : s.ScaleEasing != null ? 4 : 0, SetScaleCurve);
            Toggle(page, 6, "Align to velocity", s.AlignToVelocity, value => s.AlignToVelocity = value);
            Slider(page, 7, "Stretch by speed", 0, 0.05f, s.VelocityStretch, value => s.VelocityStretch = value, 0.001f, "{0:0.###}");
            Toggle(page, 8, "Random flip", s.RandomFlip, value => s.RandomFlip = value);
            Choice(page, 9, "Frame mode", new[] { "Random frame", "Animate over life", "Loop frames" }, (int)s.FrameMode, index => s.FrameMode = (ParticleFrameMode)index);
            Slider(page, 10, "Frame rate", 1, 60, s.FrameRate, value => s.FrameRate = value, 1, "{0:0}");
        }

        private void BuildColorPage(Panel page)
        {
            var s = settings;
            ColorOption.CreateColorOption(page, 0, color => s.StartColor = color, "Start color", Palette, true, 0.75f, 26, SliderOption.SliderLeft);
            ColorOption.CreateColorOption(page, RowHeight, color => s.EndColor = color, "End color", Palette, true, 0.75f, 26, SliderOption.SliderLeft);
            var colorModes = new[] { "Start to end", "Preset gradient", "Fire", "Rainbow", "Ice" };
            Choice(page, 2, "Color over life", colorModes, s.ColorOverLifetime != null ? 1 : 0, SetColorMode);
            Toggle(page, 3, "Palette tints", s.Colors.Count > 0, value =>
            {
                s.Colors.Clear();
                if (value)
                    s.Colors.AddRange(TintPalette);
            });
            var alphaCurves = new[] { "None", "Preset curve", "Fade in-out", "Blink", "Flash" };
            Choice(page, 4, "Alpha over life", alphaCurves, s.AlphaOverLifetime != null ? 1 : 0, SetAlphaCurve);
            Choice(page, 5, "Blend", new[] { "Alpha", "Additive" }, s.BlendState != null ? 1 : 0, index => s.BlendState = index == 1 ? ParticleResources.AdditiveBlendState : null);
            Slider(page, 6, "Emitter opacity", 0, 1, emitter.Opacity, value => emitter.SetOpacity(value), 0.05f);
            ColorOption.CreateColorOption(page, 7 * RowHeight, color => emitter.SetColor(color), "Emitter tint", Palette, true, 0.75f, 26, SliderOption.SliderLeft);
        }

        private void BuildTimelinePage(Panel page)
        {
            var s = settings;
            Slider(page, 0, "Duration (0=off)", 0, 10, s.Duration, value => s.Duration = value, 0.1f);
            Toggle(page, 1, "Loop", s.Loop, value => s.Loop = value);
            Slider(page, 2, "Start delay", 0, 5, s.StartDelay, value => s.StartDelay = value, 0.1f);
            Slider(page, 3, "Burst count", 0, 300, s.Bursts.Count > 0 ? s.Bursts[0].CountMax : 0, value => SetBurstCount((int)value), 5, "{0:0}");
            Slider(page, 4, "Burst time", 0, 5, burstTemplate.Time, value => burstTemplate.Time = value, 0.1f);
            Slider(page, 5, "Burst cycles", 1, 10, burstTemplate.Cycles, value => burstTemplate.Cycles = (int)value, 1, "{0:0}");
            Slider(page, 6, "Burst interval", 0.05f, 2, burstTemplate.Interval, value => burstTemplate.Interval = value, 0.05f);
            Slider(page, 7, "Burst chance", 0, 1, burstTemplate.Probability, value => burstTemplate.Probability = value, 0.05f);
            Choice(page, 8, "On death", new[] { "None", "Preset", "Sparks", "Smoke puff", "Splash" }, s.OnDeath != null ? 1 : 0, SetDeathEmitter);
            Choice(page, 9, "Trail", new[] { "None", "Preset", "Smoke", "Glow" }, s.Trail != null ? 1 : 0, SetTrailEmitter);
            Slider(page, 10, "Trail rate", 0, 120, trailRate, value =>
            {
                trailRate = value;
                if (s.Trail != null)
                    s.Trail.Rate = value;
            }, 5, "{0:0}");
            Slider(page, 11, "Time scale", 0, 3, ServiceProvider.Clock.TimeScale, value => ServiceProvider.Clock.TimeScale = value, 0.1f);
        }

        // ---- Option callbacks

        private void SetAppearance(int index)
        {
            var s = settings;
            s.Frames.Clear();
            if (index == 6)
            {
                s.Texture = ContentHandler.Instance.TextureSpriteCoin;
                for (var row = 0; row < 3; row++)
                    for (var column = 0; column < 10; column++)
                        s.Frames.Add(new Rectangle(column * 96, row * 96, 96, 96));
                s.StartScale = Math.Min(s.StartScale, 0.5f);
                s.EndScale = Math.Min(s.EndScale, 0.5f);
            }
            else
            {
                s.Texture = null;
                s.Appearance = (ParticleShape)index;
            }
        }

        private void SetScaleCurve(int index)
        {
            var s = settings;
            s.ScaleOverLifetime = null;
            s.ScaleEasing = null;
            switch (index)
            {
                case 1:
                    s.ScaleOverLifetime = presetScaleCurve;
                    s.ScaleEasing = presetScaleEasing;
                    break;
                case 2:
                    s.ScaleOverLifetime = ParticleCurve.FadeInOut(0.2f, 0.3f, Math.Max(s.StartScale, 0.1f));
                    break;
                case 3:
                    s.ScaleOverLifetime = ParticleCurve.Peak(0.3f, Math.Max(s.StartScale, 0.1f) * 1.5f, 0f, s.EndScale);
                    break;
                case 4:
                    s.ScaleEasing = Easing.BackOut;
                    break;
                case 5:
                    s.ScaleEasing = Easing.ElasticOut;
                    break;
            }
        }

        private void SetColorMode(int index)
        {
            var s = settings;
            switch (index)
            {
                case 1:
                    s.ColorOverLifetime = presetGradient;
                    break;
                case 2:
                    s.ColorOverLifetime = ParticleGradient.FromColors(Color.White, Color.Yellow, Color.OrangeRed, Color.Transparent);
                    break;
                case 3:
                    s.ColorOverLifetime = ParticleGradient.FromColors(Color.Red, Color.Yellow, Color.Lime, Color.Cyan, Color.Blue, Color.Magenta, Color.Transparent);
                    break;
                case 4:
                    s.ColorOverLifetime = ParticleGradient.FromColors(Color.White, Color.LightCyan, Color.DeepSkyBlue, Color.Transparent);
                    break;
                default:
                    s.ColorOverLifetime = null;
                    break;
            }
        }

        private void SetAlphaCurve(int index)
        {
            var s = settings;
            switch (index)
            {
                case 1:
                    s.AlphaOverLifetime = presetAlphaCurve;
                    break;
                case 2:
                    s.AlphaOverLifetime = ParticleCurve.FadeInOut(0.2f, 0.4f);
                    break;
                case 3:
                    s.AlphaOverLifetime = ParticleCurve.Blink(3, 0.6f);
                    break;
                case 4:
                    s.AlphaOverLifetime = new ParticleCurve().AddKey(0f, 1f).AddKey(0.25f, 0.2f).AddKey(1f, 0f);
                    break;
                default:
                    s.AlphaOverLifetime = null;
                    break;
            }
        }

        private void SetBurstCount(int count)
        {
            var s = settings;
            if (count <= 0)
            {
                s.Bursts.Clear();
                return;
            }

            burstTemplate.CountMin = count;
            burstTemplate.CountMax = count;
            if (!s.Bursts.Contains(burstTemplate))
            {
                s.Bursts.Clear();
                s.Bursts.Add(burstTemplate);
            }
        }

        private void SetDeathEmitter(int index)
        {
            var s = settings;
            switch (index)
            {
                case 1:
                    s.OnDeath = presetDeath;
                    break;
                case 2:
                {
                    var sparks = ParticlePresets.Sparks();
                    sparks.MaxParticles = 1000;
                    sparks.SpeedMin = 60;
                    sparks.SpeedMax = 220;
                    sparks.LifetimeMin = 0.3f;
                    sparks.LifetimeMax = 0.6f;
                    s.OnDeath = new ParticleSubEmitter(sparks) { CountMin = 4, CountMax = 8 };
                    break;
                }
                case 3:
                {
                    var puff = ParticlePresets.Smoke();
                    puff.MaxParticles = 600;
                    puff.Size = 24;
                    puff.LifetimeMin = 0.5f;
                    puff.LifetimeMax = 1f;
                    s.OnDeath = new ParticleSubEmitter(puff) { CountMin = 1, CountMax = 1 };
                    break;
                }
                case 4:
                {
                    var splash = new ParticleSettings
                    {
                        MaxParticles = 1000,
                        AngleMin = -150,
                        AngleMax = -30,
                        SpeedMin = 60,
                        SpeedMax = 180,
                        Gravity = new Vector2(0, 600),
                        LifetimeMin = 0.2f,
                        LifetimeMax = 0.4f,
                        Appearance = ParticleShape.Circle,
                        Size = 4,
                        StartColor = new Color(190, 215, 255),
                        EndColor = Color.Transparent
                    };
                    s.OnDeath = new ParticleSubEmitter(splash) { CountMin = 4, CountMax = 8, InheritColor = true };
                    break;
                }
                default:
                    s.OnDeath = null;
                    break;
            }
        }

        private void SetTrailEmitter(int index)
        {
            var s = settings;
            switch (index)
            {
                case 1:
                    s.Trail = presetTrail;
                    if (s.Trail != null)
                        s.Trail.Rate = trailRate;
                    break;
                case 2:
                {
                    var smoke = ParticlePresets.Smoke();
                    smoke.MaxParticles = 2000;
                    smoke.Size = 16;
                    smoke.LifetimeMin = 0.4f;
                    smoke.LifetimeMax = 0.9f;
                    smoke.SpeedMin = 5;
                    smoke.SpeedMax = 20;
                    s.Trail = new ParticleSubEmitter(smoke) { Rate = trailRate };
                    break;
                }
                case 3:
                {
                    var glow = new ParticleSettings
                    {
                        MaxParticles = 2000,
                        SpeedMin = 0,
                        SpeedMax = 10,
                        LifetimeMin = 0.3f,
                        LifetimeMax = 0.5f,
                        Appearance = ParticleShape.Glow,
                        Size = 10,
                        BlendState = ParticleResources.AdditiveBlendState,
                        StartColor = Color.White,
                        EndColor = Color.Transparent,
                        ScaleOverLifetime = ParticleCurve.Linear(1f, 0.2f),
                        RotationMax = 0
                    };
                    s.Trail = new ParticleSubEmitter(glow) { Rate = trailRate, InheritColor = true };
                    break;
                }
                default:
                    s.Trail = null;
                    break;
            }
        }

        /// <summary>Creates random settings: a different effect every time.</summary>
        private void Randomize()
        {
            var random = ServiceProvider.Random;
            var s = new ParticleSettings
            {
                MaxParticles = 600,
                EmissionRate = random.NextFloat(20, 200),
                LifetimeMin = random.NextFloat(0.3f, 1.5f),
                SpeedMin = random.NextFloat(0, 150),
                Shape = (EmitterShape)random.Next(0, 4),
                SpawnRadius = random.NextFloat(0, 80),
                SpawnInnerRadius = random.NextFloat(0, 40),
                SpawnSize = new Vector2(random.NextFloat(50, 400), random.NextFloat(0, 200)),
                EmitFromEdge = random.NextBool(),
                RadialVelocity = random.Chance(0.4f),
                Gravity = random.Chance(0.5f) ? new Vector2(0, random.NextFloat(-200, 500)) : Vector2.Zero,
                Drag = random.Chance(0.5f) ? random.NextFloat(0, 0.6f) : 0f,
                Turbulence = random.Chance(0.5f) ? random.NextFloat(20, 150) : 0f,
                TurbulenceFrequency = random.NextFloat(0.3f, 2f),
                VortexStrength = random.Chance(0.3f) ? random.NextFloat(-300, 300) : 0f,
                AttractionStrength = random.Chance(0.3f) ? random.NextFloat(-200, 200) : 0f,
                Appearance = (ParticleShape)random.Next(0, 5),
                Size = random.NextFloat(3, 30),
                ScaleVariation = random.NextFloat(0, 0.6f),
                BlendState = random.NextBool() ? ParticleResources.AdditiveBlendState : null,
                RotationSpeedMin = random.Chance(0.5f) ? random.NextFloat(-300, 0) : 0f,
                AlignToVelocity = random.Chance(0.3f),
                VelocityStretch = random.Chance(0.3f) ? random.NextFloat(0.002f, 0.02f) : 0f
            };
            s.LifetimeMax = s.LifetimeMin + random.NextFloat(0.1f, 1.5f);
            s.SpeedMax = s.SpeedMin + random.NextFloat(20, 300);
            s.RotationSpeedMax = -s.RotationSpeedMin;
            if (random.NextBool())
            {
                s.AngleMin = random.NextFloat(-180, 180);
                s.AngleMax = s.AngleMin + random.NextFloat(10, 90);
            }

            var first = random.NextColor();
            var second = random.NextColor();
            var third = random.NextColor();
            if (random.NextBool())
            {
                s.Colors.Add(first);
                s.Colors.Add(second);
                s.Colors.Add(third);
                s.StartColor = Color.White;
                s.EndColor = Color.White;
                s.AlphaOverLifetime = ParticleCurve.FadeInOut(0.1f, 0.4f);
            }
            else
            {
                s.ColorOverLifetime = ParticleGradient.FromColors(first, second, third, Color.Transparent);
            }

            switch (random.Next(0, 3))
            {
                case 0:
                    s.ScaleOverLifetime = ParticleCurve.FadeInOut(0.2f, 0.3f);
                    break;
                case 1:
                    s.ScaleOverLifetime = ParticleCurve.Linear(0.3f, 1.5f);
                    break;
                case 2:
                    s.ScaleEasing = Easing.BackOut;
                    break;
            }

            if (random.Chance(0.3f))
            {
                s.Duration = random.NextFloat(0.5f, 2f);
                s.Bursts.Add(new ParticleBurst(0f, random.Next(20, 120)));
            }

            ApplySettings(s, GetPresetPosition(string.Empty));
            foreach (var button in presetButtons)
                button.BackgroundColor = PresetColor;
        }
    }
}
