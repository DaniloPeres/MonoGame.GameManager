using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Controls.MouseEvent;
using MonoGame.GameManager.Extensions;

namespace MonoGame.GameManager.Controls
{
    public class Button : ScalableContainerAbstract<Button>
    {
        private Texture2D drawTexture;
        private Texture2D defaultTexture;
        private Texture2D DefaultTexture
        {
            get => defaultTexture;
            set
            {
                defaultTexture = value;
                MarkAsDirty();
            }
        }
        private Texture2D hoverTexture;
        private Texture2D mousePressedTexture;

        public Button(Texture2D defaultTexture, Vector2 position)
        {
            SetDefaultTexture(defaultTexture);
            SetPosition(position);
            AddOnMouseEnter(OnMouseEnterButton);
            AddOnMouseLeave(OnMouseLeaveButton);
            AddOnMousePressed(OnMousePressed);
            AddOnMouseReleased(OnMouseReleased);
            UpdateDrawTexture();
        }

        public Button SetDefaultTexture(Texture2D defaultTexture)
        {
            DefaultTexture = defaultTexture;
            return this;
        }

        public Button SetHoverTexture(Texture2D hoverTexture)
        {
            this.hoverTexture = hoverTexture;
            return this;
        }

        public Button SetMousePressedTexture(Texture2D mousePressedTexture)
        {
            this.mousePressedTexture = mousePressedTexture;
            return this;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawTexture(spriteBatch, drawTexture, new Rectangle(GetPosition().ToPoint(), (drawTexture.Size().ToVector2() * Scale).ToPoint()), null, OriginWithoutScale);
            base.Draw(spriteBatch);
        }

        private void OnMouseEnterButton(ControlEventArgs args) => UpdateDrawTexture();
        private void OnMouseLeaveButton(ControlEventArgs args) => UpdateDrawTexture();
        private void OnMousePressed(ControlEventArgs args) => UpdateDrawTexture();
        private void OnMouseReleased(ControlEventArgs args) => UpdateDrawTexture();

        private void UpdateDrawTexture()
        {
            drawTexture = 
                IsMousePressed && IsMouseHover && mousePressedTexture != null // use pressed texture
                    ? mousePressedTexture
                    : IsMouseHover && hoverTexture != null // use hover texture
                        ? hoverTexture
                        : DefaultTexture; // use default texture
        }

        protected override Vector2 CalculateSize() => DefaultTexture.Size().ToVector2();
    }
}
