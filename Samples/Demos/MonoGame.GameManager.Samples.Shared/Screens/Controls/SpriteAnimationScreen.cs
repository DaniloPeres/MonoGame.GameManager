using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Controls.Builders;
using MonoGame.GameManager.Controls.MouseEvent;
using MonoGame.GameManager.Controls.Sprites;
using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.GameManager.Samples.Screens.Controls
{
    public class SpriteAnimationScreen : Screen
    {
        int sectionTop = Config.ScreenContentMargin + 60;
        int sectionDivisionLeft = 550;
        int sectionHeight = 690;
        int sectionDivisionBottom = 500;
        int sectionWidth = 1170;
        SpriteAnimation spriteAnimationPreview;
        Label playStopLabel;
        Panel selectFramePanel;
        Panel selectCyclePanel;
        List<SpriteAnimationInfo> spriteAnimationsInfo;

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Sprite Animation", OpenSpriteAnimation)
            });

            spriteAnimationsInfo = new List<SpriteAnimationInfo>
            {
                ServiceProvider.ContentLoaderManager.LoadSpriteAnimationInfo("Images/Sprites/Dino/Dino.sa"),
                ServiceProvider.ContentLoaderManager.LoadSpriteAnimationInfo("Images/Sprites/Coin.sa"),
                ServiceProvider.ContentLoaderManager.LoadSpriteAnimationInfo("Images/Sprites/TorchDrippingRed.sa"),
                CreateSpriteAnimationInfo4Cycles(ContentHandler.Instance.TextureSpriteAngel, new Vector2(48)),
                CreateSpriteAnimationInfo4Cycles(ContentHandler.Instance.TextureSpriteCharacter, new Vector2(32, 48)),
                CreateSpriteAnimationInfo4Cycles(ContentHandler.Instance.TextureSpriteDeath, new Vector2(50, 48)),
                CreateSpriteAnimationInfo4Cycles(ContentHandler.Instance.TextureSpriteLeviathan, new Vector2(96))
            };

            CreatePreviewSection();
            CreateOptionsSection();
            new RectangleControl(new Rectangle(sectionDivisionLeft, sectionTop, 2, sectionDivisionBottom - sectionTop), Color.White)
                .AddToScreen();
            new RectangleControl(new Rectangle(sectionDivisionLeft, sectionDivisionBottom, sectionWidth - sectionDivisionLeft, 2), Color.White)
                .AddToScreen();

            base.OnInit();
        }

        public static void OpenSpriteAnimation()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new SpriteAnimationScreen());
        }

        private void CreatePreviewSection()
        {
            var container = new Panel(new Rectangle(sectionDivisionLeft + Config.ScreenContentMargin, sectionTop, sectionWidth - sectionDivisionLeft - Config.ScreenContentMargin, sectionDivisionBottom - sectionTop - 15))
                .AddToScreen();

            new Label(ContentHandler.Instance.SpriteFontArial, "Preview", Vector2.Zero, Color.Yellow)
                .SetAnchor(Enums.Anchor.TopCenter)
                .AddToScreen(container);

            const int spriteAnimationContainerMarginTop = 50;
            var spriteAnimationContainer = new Panel(new Rectangle(0, spriteAnimationContainerMarginTop, (int)container.Size.X, (int)(container.Size.Y - spriteAnimationContainerMarginTop)))
                .AddToScreen(container);

            new RectangleControl(Vector2.Zero, spriteAnimationContainer.Size, new Color(15, 15, 15))
                .AddToScreen(spriteAnimationContainer);


            spriteAnimationPreview = new SpriteAnimation(spriteAnimationsInfo.First())
                .AddOnAnimationPlay(UpdatePlayStopButtonLabel)
                .AddOnAnimationStop(UpdatePlayStopButtonLabel)
                .AddOnFrameChanged(UpdateSelectFramePanel)
                .SetAnchor(Enums.Anchor.Center)
                .AddToScreen(spriteAnimationContainer);
            spriteAnimationPreview.Play();
        }

        private void CreateOptionsSection()
        {
            var container = new Panel(new Rectangle(Config.ScreenContentMargin, sectionTop, sectionDivisionLeft - (Config.ScreenContentMargin * 2), sectionHeight))
                .AddToScreen();

            var labelOptions = new Label(ContentHandler.Instance.SpriteFontArial, "Options", Vector2.Zero, Color.Yellow)
                .SetAnchor(Enums.Anchor.TopCenter)
                .AddToScreen(container);

            var optionMarginTop = 10;

            var posY = labelOptions.Size.Y + optionMarginTop;

            var marginTop = 42;

            AnchorOption.CreateAnchorOption(container, posY, anchor => spriteAnimationPreview.SetAnchor(anchor));
            posY += 142;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Scale", posY, spriteAnimationPreview.Scale.X, scale => spriteAnimationPreview.SetScale(scale), 0.1f);
            posY += marginTop;
            Vector2Option.CreateVector2Option(container, "Position", posY, new Vector2(0), scale => spriteAnimationPreview.SetPosition(scale), 1);
            posY += marginTop;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Rotation", posY, 0, newRotation => spriteAnimationPreview.SetRotation(newRotation), 0.05f);
            posY += marginTop;
            Vector2Option.CreateVector2Option(container, "Origin Rate", posY, new Vector2(0), newOrigin => spriteAnimationPreview.SetOriginRate(newOrigin), 0.1f);
            posY += marginTop;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Speed", posY, 1, speed => spriteAnimationPreview.Speed = speed, 0.1f);
            posY += marginTop;
            CheckboxOption.CreateCheckboxOption(container, "Loop", posY, true, isLooping => spriteAnimationPreview.IsLooping = isLooping);
            posY += marginTop;
            CheckboxOption.CreateCheckboxOption(container, "Reverse", posY, false, isReverse => spriteAnimationPreview.IsReverse = isReverse);
            posY += marginTop;
            CheckboxOption.CreateCheckboxOption(container, "Ping-Pong", posY, false, isPingPong => spriteAnimationPreview.IsPingPong = isPingPong);
            posY += marginTop;
            LastEventsInfo.AddLastEventsInfo(container, posY, spriteAnimationPreview);

            container = new Panel(new Rectangle(sectionDivisionLeft + Config.ScreenContentMargin * 2, sectionDivisionBottom + Config.ScreenContentMargin, sectionWidth - sectionDivisionLeft, sectionHeight - sectionDivisionLeft - Config.ScreenContentMargin))
                .AddToScreen();
            posY = 0;

            var playStopButton = new Button(ContentHandler.Instance.TextureButtonBackground, Vector2.Zero)
                .AddToScreen(container)
                .SetHoverTexture(ContentHandler.Instance.TextureButtonBackgroundHover)
                .SetMousePressedTexture(ContentHandler.Instance.TextureButtonBackgroundPressed)
                .AddOnClick(PlayStopButtonClick);

            playStopLabel = new Label(ContentHandler.Instance.SpriteFontArial, "Stop", Vector2.Zero, Color.White)
                .AddToScreen(playStopButton)
                .SetScale(0.75f)
                .SetAnchor(Enums.Anchor.Center);

            var resetAnimationButton = new Button(ContentHandler.Instance.TextureButtonBackground, new Vector2(playStopButton.Size.X + 10, 0))
                .AddToScreen(container)
                .SetScale(new Vector2(1.25f, 1f))
                .SetHoverTexture(ContentHandler.Instance.TextureButtonBackgroundHover)
                .SetMousePressedTexture(ContentHandler.Instance.TextureButtonBackgroundPressed)
                .AddOnClick(ResetAnimationButtonClick);

            new Label(ContentHandler.Instance.SpriteFontArial, "Reset Animation", Vector2.Zero, Color.White)
                .AddToScreen(resetAnimationButton)
                .SetScale(0.75f)
                .SetAnchor(Enums.Anchor.Center);

            posY += playStopButton.Size.Y + 5;

            var setFrameLabel = new Label(ContentHandler.Instance.SpriteFontArial, "Set Frame: ", new Vector2(0, posY), Color.Yellow)
                .AddToScreen(container);
            selectFramePanel = new Panel()
                .AddToScreen(container)
                .SetPosition(setFrameLabel.Size.X + 5, posY);
            UpdateSelectFramePanel();
            posY += marginTop;

            var setCycleLabel = new Label(ContentHandler.Instance.SpriteFontArial, "Set Cycle: ", new Vector2(0, posY), Color.Yellow)
                .AddToScreen(container);
            selectCyclePanel = new Panel()
                .AddToScreen(container)
                .SetPosition(setFrameLabel.Size.X + 5, posY);
            UpdateSelectCyclePanel();
            posY += marginTop;

            var setSpriteAnimationInfoLabel = new Label(ContentHandler.Instance.SpriteFontArial, "Set Sprite: ", new Vector2(0, posY), Color.Yellow)
                .AddToScreen(container);
            var posX = setSpriteAnimationInfoLabel.Size.X + 5;
            for (var i = 0; i < spriteAnimationsInfo.Count; i++)
            {
                var spriteAnimationInfo = spriteAnimationsInfo[i];
                var frame = spriteAnimationInfo.GetSpriteAnimationCycleByIndex(0).Frames.First();
                var scale = 55f / frame.SourceRectangle.Height;
                var imageSprite = new Image(frame.Texture)
                    .SetSourceRectangle(frame.SourceRectangle)
                    .AddToScreen(container)
                    .SetScale(scale)
                    .SetPosition(new Vector2(posX, posY))
                    .AddOnClick(args =>
                    {
                        spriteAnimationPreview.ChangeAnimationSpriteInfo(spriteAnimationInfo);
                        UpdateSelectFramePanel();
                        UpdateSelectCyclePanel();
                    });
                posX += imageSprite.Size.X + 5;
            }


            posY += marginTop;

        }

        private SpriteAnimationInfo CreateSpriteAnimationInfo4Cycles(Texture2D texture, Vector2 size)
        {
            var cycles = new List<SpriteAnimationCycle>();

            var frameDuration = 0.08f;
            var cycleNames = new List<string>()
            {
                "walkNorth",
                "walkSouth",
                "walkWest",
                "walkEast"
            };

            for (var i = 0; i < cycleNames.Count; i++)
            {
                cycles.Add(new SpriteAnimationCycleBuilder()
                    .WithTexture(texture)
                    .WithSize(size)
                    .WithTotalOfFrames(4)
                    .WithName(cycleNames[i])
                    .WithFrameDuration(frameDuration)
                    .WithTexturePosition(new Vector2(0, size.Y * i))
                    .Build());
            }

            return new SpriteAnimationInfo(cycles);
        }

        private void PlayStopButtonClick(ControlEventArgs args)
        {
            if (playStopLabel.Text == "Stop")
                spriteAnimationPreview.Stop();
            else
                spriteAnimationPreview.Play(spriteAnimationPreview.IsLastFrame);

            UpdatePlayStopButtonLabel();
        }

        private void ResetAnimationButtonClick(ControlEventArgs args)
        {
            spriteAnimationPreview.ResetAnimation();
        }

        private void UpdatePlayStopButtonLabel()
        {
            if (playStopLabel == null)
                return;

            playStopLabel.Text = spriteAnimationPreview.IsPlaying
                ? "Stop"
                : "Play";
        }

        private void UpdateSelectFramePanel()
        {
            if (selectFramePanel == null)
                return;

            selectFramePanel.ClearChildren();

            var frames = spriteAnimationPreview.ActualCycle.Frames;
            AddNumberOptions(selectFramePanel, frames.Length, spriteAnimationPreview.FrameIndex, i => spriteAnimationPreview.SetFrame(i));
        }

        private void UpdateSelectCyclePanel()
        {
            if (selectCyclePanel == null)
                return;

            selectCyclePanel.ClearChildren();

            AddNumberOptions(
                selectCyclePanel,
                spriteAnimationPreview.SpriteAnimationInfo.CyclesCount,
                spriteAnimationPreview.SpriteAnimationInfo.FindCycleIndex(spriteAnimationPreview.ActualCycle.Name),
                ChangeCycle);
        }

        private void ChangeCycle(int cycleIndex)
        {
            spriteAnimationPreview.Play(cycleIndex);
            UpdateSelectCyclePanel();
        }

        private void AddNumberOptions(Panel container, int count, int selected, Action<int> onClick)
        {
            var recSize = 30;
            for (var i = 0; i < count; i++)
            {
                var framePanel = new Panel(new Rectangle((recSize + 5) * i, 0, recSize, recSize))
                    .AddToScreen(container);

                var recBackground = new RectangleControl(Vector2.Zero, framePanel.Size, Color.DarkRed)
                    .AddToScreen(framePanel);

                if (i != selected)
                {
                    var index = i;
                    recBackground
                        .SetColor(Color.DarkGray)
                        .SetMouseEventsColor(Color.Gray, Color.DarkSlateGray)
                        .AddOnClick(args => onClick(index));
                }

                new Label(ContentHandler.Instance.SpriteFontArial, i.ToString(), Vector2.Zero, Color.White)
                    .AddToScreen(framePanel)
                    .SetScale(0.5f)
                    .SetAnchor(Enums.Anchor.Center);
            }
        }
    }
}
