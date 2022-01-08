using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Extensions;

namespace MonoGame.GameManager.Controls
{
    public class Image : ScalableControlAbstract<Image>
    {
        private Rectangle? SourceRectangle { get; set; }

        private Texture2D texture;
        public Texture2D Texture {
            get => texture;
            set {
                texture = value;
                MarkAsDirty();
            }
        }

        public Image(Texture2D texture)
        {
            Texture = texture;
        }

        public Image SetSourceRectangle(Rectangle? sourceRectangle)
        {
            SourceRectangle = sourceRectangle;
            return this;
        }

        public override void Draw(SpriteBatch spriteBatch) => DrawTexture(spriteBatch, Texture, DestinationRectangle, SourceRectangle, OriginWithoutScale);
        protected override Vector2 CalculateSize() {
            return SourceRectangle != null
                ? SourceRectangle.Value.Size.ToVector2()
                : Texture.Size().ToVector2();
        }
    }
}
