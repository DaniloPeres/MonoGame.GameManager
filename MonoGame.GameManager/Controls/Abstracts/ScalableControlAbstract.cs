using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;

namespace MonoGame.GameManager.Controls.Abstracts
{
    public abstract class ScalableControlAbstract<TControl> : Control<TControl>, IScalableControl where TControl : IScalableControl
    {
        Vector2 IScalableControl.Scale {
            get => Scale;
            set => Scale = value;
        }
        public virtual Vector2 Scale
        {
            get => scale;
            set
            {
                scale = value;
                MarkAsDirty();
            }
        }
        private Vector2 scale = Vector2.One;

        public override Vector2 Size
        {
            get => base.Size * NestedScale;
            set => base.Size = value;
        }

        public Vector2 SizeWithoutScale => base.Size;

        public override Vector2 Origin
        {
            get => base.Origin * NestedScale;
            set => base.Origin = value;
        }

        Vector2 IScalableControl.OriginWithoutScale {
            get => OriginWithoutScale;
            set => OriginWithoutScale = value;
        }
        public virtual Vector2 OriginWithoutScale
        {
            get => base.Origin;
            set => base.Origin = value;
        }

        protected override Vector2 CalculateSize() => SizeWithoutScale;
        public override Vector2 CalculateNestedScale() => base.CalculateNestedScale() * Scale;
        IScalableControl IScalableControl.SetScale(float scale) => SetScale(scale);
        public TControl SetScale(float scale) => SetScale(new Vector2(scale));
        IScalableControl IScalableControl.SetScale(Vector2 scale) => SetScale(scale);
        public TControl SetScale(Vector2 scale)
        {
            Scale = scale;
            return (TControl)(object)this;
        }

        public override TControl SetOriginRate(Vector2 originRate) => SetOriginRate(originRate, SizeWithoutScale);
    }
}
