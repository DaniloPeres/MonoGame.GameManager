using Microsoft.Xna.Framework;

namespace MonoGame.GameManager.Controls.Interfaces
{
    public interface IScalableControl: IControl
    {
        Vector2 Scale { get; set; }
        Vector2 OriginWithoutScale { get; set; }
        IScalableControl SetScale(float scale);
        IScalableControl SetScale(Vector2 scale);
    }
}
