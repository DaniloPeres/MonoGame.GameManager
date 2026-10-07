using Microsoft.Xna.Framework;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Particles;
using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Services.Inputs;
using MonoGame.GameManager.Timers;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Samples.Screens.Controls
{
    /// <summary>
    /// Scenes that combine several particle emitters with other controls, animations and timers.
    /// </summary>
    public class ParticleScenesScreen : Screen
    {
        private const int SectionTop = Config.ScreenContentMargin + 60;
        private const int SectionHeight = 690;
        private const int MenuWidth = 200;
        private const int StageLeft = Config.ScreenContentMargin + MenuWidth + 20;
        private const int StageWidth = 1200 - StageLeft - Config.ScreenContentMargin;

        private static readonly Vector2 StageOrigin = new Vector2(StageLeft, SectionTop);
        private static readonly Color MenuColor = new Color(50, 70, 90);
        private static readonly Color SelectedMenuColor = new Color(230, 150, 40);

        private static readonly (string name, string description)[] Scenes =
        {
            ("Campfire", "Fire, smoke and sparks that bounce on the ground, with a glow that breathes."),
            ("Fireworks", "Rockets with a trail that explode into a burst of their own color (sub-emitters). Click to launch one."),
            ("Rainy day", "Rain stretched by its speed, splashes when the drops hit the ground (floor + on-death emitter), lightning every 4 seconds."),
            ("Confetti cannon", "One-shot effects that remove themselves when they end. Click anywhere, or press Fire!"),
            ("Magic cursor", "An emitter that follows the pointer and emits per pixel moved."),
            ("Fountain", "Three jets of water pulled down by gravity, bouncing in the basin."),
            ("Portal", "Attraction and vortex forces around a point, with additive blending."),
            ("Spaceships", "The same exhaust in world space (the trail stays behind) and in local space (it moves with the ship).")
        };

        private readonly List<ScheduledAction> scheduled = new List<ScheduledAction>();
        private Panel stage;
        private Panel sceneRoot;
        private Button[] sceneButtons;
        private MultiLineLabel description;

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Particles", ParticlesScreen.OpenParticlesScreen),
                ("Scenes", OpenParticleScenesScreen)
            });

            CreateMenu();
            stage = new Panel(new Rectangle(StageLeft, SectionTop, StageWidth, SectionHeight))
                .SetHideOverflow(true)
                .AddToScreen();

            ShowScene(0);
            base.OnInit();
        }

        public static void OpenParticleScenesScreen()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new ParticleScenesScreen());
        }

        /// <summary>Replaces the current scene.</summary>
        public void ShowScene(int index)
        {
            foreach (var action in scheduled)
                action.Cancel();
            scheduled.Clear();

            sceneRoot?.Dispose();
            sceneRoot = new Panel(Vector2.Zero, new Vector2(StageWidth, SectionHeight))
                .SetAcceptedMouseButtons(MouseButtons.Left | MouseButtons.Right)
                .AddToScreen(stage);

            for (var i = 0; i < sceneButtons.Length; i++)
                sceneButtons[i].BackgroundColor = i == index ? SelectedMenuColor : MenuColor;
            description.SetText(Scenes[index].description);

            switch (index)
            {
                case 0: BuildCampfire(); break;
                case 1: BuildFireworks(); break;
                case 2: BuildRainyDay(); break;
                case 3: BuildConfettiCannon(); break;
                case 4: BuildMagicCursor(); break;
                case 5: BuildFountain(); break;
                case 6: BuildPortal(); break;
                case 7: BuildSpaceships(); break;
            }
        }

        private void CreateMenu()
        {
            var font = ContentHandler.Instance.SpriteFontArial;
            var menu = new Panel(new Rectangle(Config.ScreenContentMargin, SectionTop, MenuWidth, SectionHeight))
                .AddToScreen();

            sceneButtons = new Button[Scenes.Length];
            for (var i = 0; i < Scenes.Length; i++)
            {
                var index = i;
                sceneButtons[i] = new Button(new Vector2(0, i * 52), new Vector2(190, 44), MenuColor)
                    .SetBorder(new Color(110, 110, 110))
                    .SetText(font, Scenes[i].name, Color.White)
                    .AddOnClick(args => ShowScene(index))
                    .AddToScreen(menu);
                sceneButtons[i].TextLabel.SetScale(0.7f);
            }

            description = new MultiLineLabel(font, string.Empty, new Vector2(0, Scenes.Length * 52 + 10), Color.LightGray, 190)
                .SetScale(0.65f)
                .AddToScreen(menu);

            var back = new Button(new Vector2(0, SectionHeight - 44), new Vector2(190, 40), new Color(70, 110, 70))
                .SetBorder(new Color(110, 110, 110))
                .SetText(font, "< Playground", Color.White)
                .AddOnClick(args => ParticlesScreen.OpenParticlesScreen())
                .AddToScreen(menu);
            back.TextLabel.SetScale(0.7f);
        }

        // ---- Scenes

        private void BuildCampfire()
        {
            Background(new Color(10, 12, 30));
            new RectangleControl(new Rectangle(0, 600, StageWidth, SectionHeight - 600), new Color(30, 26, 22))
                .AddToScreen(sceneRoot);

            var center = new Vector2(StageWidth / 2f, 596);
            var glow = new CircleControl(center + new Vector2(0, -40), 150, new Color(255, 140, 40))
                .SetOpacity(0.05f)
                .AddToScreen(sceneRoot);
            new OpacityAnimation(glow, 0.7f, 0.1f)
                .SetParent(glow)
                .SetIsLooping(true)
                .SetIsPingPong(true)
                .Play();

            AddLog(center + new Vector2(-10, 4), -18);
            AddLog(center + new Vector2(10, 4), 18);

            new ParticleEmitter(ParticlePresets.Smoke())
                .SetPosition(center + new Vector2(0, -40))
                .AddToScreen(sceneRoot)
                .Play();
            new ParticleEmitter(ParticlePresets.Fire())
                .SetPosition(center)
                .AddToScreen(sceneRoot)
                .Play();

            var sparks = ParticlePresets.Sparks();
            sparks.EmissionRate = 18;
            sparks.SpeedMin = 80;
            sparks.SpeedMax = 260;
            sparks.Floor = StageOrigin.Y + 640;
            new ParticleEmitter(sparks)
                .SetPosition(center + new Vector2(0, -6))
                .AddToScreen(sceneRoot)
                .Play();
        }

        private void AddLog(Vector2 position, float rotationInDegrees)
        {
            new RectangleControl(new Rectangle(0, 0, 130, 18), new Color(110, 70, 40))
                .SetBorder(new Color(70, 40, 20), 2)
                .SetOriginRate(new Vector2(0.5f))
                .SetPosition(position)
                .SetRotationInDegree(rotationInDegrees)
                .AddToScreen(sceneRoot);
        }

        private void BuildFireworks()
        {
            Background(new Color(5, 5, 20));
            AddStars(70);
            new RectangleControl(new Rectangle(0, 660, StageWidth, SectionHeight - 660), new Color(20, 25, 20))
                .AddToScreen(sceneRoot);

            var fireworks = ParticlePresets.Fireworks();
            fireworks.SpawnSize = new Vector2(800, 0);
            new ParticleEmitter(fireworks)
                .SetPosition(StageWidth / 2f, 665)
                .AddToScreen(sceneRoot)
                .Play();

            var launcher = ParticlePresets.Fireworks();
            launcher.Shape = EmitterShape.Point;
            launcher.Bursts.Clear();
            launcher.Duration = 0;
            var launcherEmitter = new ParticleEmitter(launcher)
                .SetPosition(0, 665)
                .AddToScreen(sceneRoot);
            sceneRoot.AddOnMousePressed(args => launcherEmitter.Burst(1, new Vector2(args.Position.X, StageOrigin.Y + 665)));
            Hint("Click to launch a rocket");
        }

        private void BuildRainyDay()
        {
            Background(new Color(40, 45, 60));
            new RectangleControl(new Rectangle(0, 610, StageWidth, SectionHeight - 610), new Color(25, 30, 35))
                .AddToScreen(sceneRoot);

            var rain = ParticlePresets.Rain();
            rain.SpawnSize = new Vector2(1100, 0);
            rain.MaxParticles = 1200;
            rain.Floor = StageOrigin.Y + 610;
            new ParticleEmitter(rain)
                .SetPosition(StageWidth / 2f, -10)
                .AddToScreen(sceneRoot)
                .Play();

            var flash = new RectangleControl(new Rectangle(0, 0, StageWidth, SectionHeight), Color.White)
                .SetOpacity(0f)
                .AddToScreen(sceneRoot);
            scheduled.Add(Scheduler.Every(4f, () =>
            {
                flash.SetOpacity(0.55f);
                new OpacityAnimation(flash, 0.35f, 0f).SetParent(flash).Play();
            }));
            Hint("Lightning every 4 seconds");
        }

        private void BuildConfettiCannon()
        {
            Background(new Color(20, 20, 30));
            var muzzle = new Vector2(130, 590);
            new RectangleControl(new Rectangle(0, 0, 120, 30), new Color(90, 90, 100))
                .SetBorder(new Color(140, 140, 150), 2)
                .SetOriginRate(new Vector2(1f, 0.5f))
                .SetPosition(muzzle)
                .SetRotationInDegree(-40)
                .AddToScreen(sceneRoot);
            new CircleControl(muzzle + new Vector2(-80, 70), 34, new Color(60, 60, 70))
                .AddToScreen(sceneRoot);

            var fire = new Button(new Vector2(StageWidth / 2f - 70, 16), new Vector2(140, 44), new Color(200, 60, 60))
                .SetBorder(Color.White, 2)
                .SetText(ContentHandler.Instance.SpriteFontArial, "Fire!", Color.White)
                .AddOnClick(args => FireConfetti(muzzle, -40f))
                .AddToScreen(sceneRoot);
            fire.TextLabel.SetScale(0.8f);

            sceneRoot.AddOnMousePressed(args => FireConfetti(args.Position.ToVector2() - StageOrigin, -90f));
            Hint("Click anywhere to pop confetti");
        }

        private void FireConfetti(Vector2 position, float angle)
        {
            var confetti = ParticlePresets.Confetti();
            confetti.AngleMin = angle - 25;
            confetti.AngleMax = angle + 25;
            confetti.Floor = StageOrigin.Y + 672;
            ParticleEmitter.Spawn(confetti, position, sceneRoot);
        }

        private void BuildMagicCursor()
        {
            Background(new Color(15, 10, 30));
            var center = new Vector2(StageWidth / 2f, SectionHeight / 2f);
            var magic = new ParticleEmitter(ParticlePresets.Magic())
                .SetPosition(center)
                .AddToScreen(sceneRoot)
                .Play();

            var stars = ParticlePresets.Stars();
            stars.SpawnRadius = 50;
            stars.EmissionRate = 12;
            var starsEmitter = new ParticleEmitter(stars)
                .SetPosition(center)
                .AddToScreen(sceneRoot)
                .Play();

            sceneRoot.AddOnMouseMoved(args =>
            {
                var position = args.Position.ToVector2() - StageOrigin;
                magic.SetPosition(position);
                starsEmitter.SetPosition(position);
                args.ContinuePropagation();
            });
            Hint("Move the pointer over the scene");
        }

        private void BuildFountain()
        {
            Background(new Color(10, 20, 40));
            const int basinTop = 600;
            new RectangleControl(new Rectangle(StageWidth / 2 - 180, basinTop, 360, 60), new Color(30, 60, 110))
                .SetBorder(new Color(90, 130, 190), 3)
                .AddToScreen(sceneRoot);

            var floor = StageOrigin.Y + basinTop + 8;
            var main = ParticlePresets.Fountain();
            main.Floor = floor;
            new ParticleEmitter(main)
                .SetPosition(StageWidth / 2f, basinTop + 6)
                .AddToScreen(sceneRoot)
                .Play();

            AddSideJet(new Vector2(StageWidth / 2f + 120, basinTop + 6), -130, -115, floor);
            AddSideJet(new Vector2(StageWidth / 2f - 120, basinTop + 6), -65, -50, floor);
        }

        private void AddSideJet(Vector2 position, float angleMin, float angleMax, float floor)
        {
            var jet = ParticlePresets.Fountain();
            jet.AngleMin = angleMin;
            jet.AngleMax = angleMax;
            jet.SpeedMin = 380;
            jet.SpeedMax = 460;
            jet.EmissionRate = 90;
            jet.Floor = floor;
            new ParticleEmitter(jet)
                .SetPosition(position)
                .AddToScreen(sceneRoot)
                .Play();
        }

        private void BuildPortal()
        {
            Background(new Color(5, 5, 15));
            var center = new Vector2(StageWidth / 2f, SectionHeight / 2f);
            new CircleControl(center, 40, new Color(60, 30, 120))
                .SetOpacity(0.8f)
                .AddToScreen(sceneRoot);
            new CircleControl(center, 150, new Color(140, 80, 255), false)
                .SetThickness(4)
                .SetOpacity(0.6f)
                .AddToScreen(sceneRoot);

            new ParticleEmitter(ParticlePresets.Vortex())
                .SetPosition(center)
                .AddToScreen(sceneRoot)
                .Play();

            var fireflies = ParticlePresets.Fireflies();
            fireflies.SpawnSize = new Vector2(800, 560);
            fireflies.StartColor = new Color(200, 160, 255);
            fireflies.EndColor = fireflies.StartColor;
            new ParticleEmitter(fireflies)
                .SetPosition(center)
                .AddToScreen(sceneRoot)
                .Play();
        }

        private void BuildSpaceships()
        {
            Background(new Color(5, 5, 15));
            AddStars(80);
            AddShip(new Vector2(120, 230), SimulationSpace.World, "World space: the exhaust stays behind the ship");
            AddShip(new Vector2(120, 470), SimulationSpace.Local, "Local space: the exhaust moves with the ship");
        }

        private void AddShip(Vector2 position, SimulationSpace space, string text)
        {
            var ship = new Panel(position, new Vector2(70, 26))
                .SetBackgroundColor(new Color(200, 205, 220))
                .AddToScreen(sceneRoot);
            new RectangleControl(new Rectangle(70, 5, 16, 16), new Color(120, 180, 255))
                .AddToScreen(ship);

            var exhaust = ParticlePresets.Fire();
            exhaust.SimulationSpace = space;
            exhaust.AngleMin = 165;
            exhaust.AngleMax = 195;
            exhaust.SpeedMin = 60;
            exhaust.SpeedMax = 140;
            exhaust.Size = 16;
            exhaust.SpawnRadius = 4;
            exhaust.LifetimeMin = 0.3f;
            exhaust.LifetimeMax = 0.7f;
            exhaust.EmissionRate = 120;
            exhaust.Turbulence = 20;
            new ParticleEmitter(exhaust)
                .SetAnchor(Enums.Anchor.CenterLeft)
                .SetPosition(0, 0)
                .AddToScreen(ship)
                .Play();

            new MoveAnimation(ship, 3f, new Vector2(StageWidth - 200, position.Y))
                .SetParent(ship)
                .SetEasing(Easing.SineInOut)
                .SetIsLooping(true)
                .SetIsPingPong(true)
                .Play();

            new Label(ContentHandler.Instance.SpriteFontArial, text, new Vector2(20, position.Y - 60), Color.Gray)
                .SetScale(0.65f)
                .AddToScreen(sceneRoot);
        }

        // ---- Helpers

        private void Background(Color color)
        {
            new RectangleControl(new Rectangle(0, 0, StageWidth, SectionHeight), color)
                .AddToScreen(sceneRoot);
        }

        private void AddStars(int count)
        {
            var random = ServiceProvider.Random;
            for (var i = 0; i < count; i++)
            {
                new RectangleControl(new Rectangle(random.Next(0, StageWidth), random.Next(0, 600), 2, 2), Color.White)
                    .SetOpacity(random.NextFloat(0.2f, 0.9f))
                    .AddToScreen(sceneRoot);
            }
        }

        private void Hint(string text)
        {
            new Label(ContentHandler.Instance.SpriteFontArial, text, new Vector2(12, 10), Color.Gray)
                .SetScale(0.65f)
                .AddToScreen(sceneRoot);
        }
    }
}
