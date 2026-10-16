using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>Skips the effects of controls that a clipping parent hides (eg: scrolled out of a scroll viewer).</summary>
    internal static class EffectCulling
    {
        /// <summary>
        /// True when the control and its effects (<paramref name="marginLocal"/> around it, in local units) are
        /// completely outside the area of a parent that hides its overflow. Parents with their own transformation
        /// (cameras) are not checked.
        /// </summary>
        public static bool IsOutsideClippingParents(IControl control, Vector2 position, Vector2 sizeWithoutScale, Vector2 originWithoutScale,
            Vector2 nestedScale, float rotation, float marginLocal)
        {
            var absoluteScale = new Vector2(Math.Abs(nestedScale.X), Math.Abs(nestedScale.Y));
            var size = sizeWithoutScale * absoluteScale;
            var topLeft = position - originWithoutScale * nestedScale;
            var margin = marginLocal * Math.Max(absoluteScale.X, absoluteScale.Y);
            if (rotation != 0f)
                margin += size.Length() / 2f;

            var area = new Rectangle((int)Math.Floor(topLeft.X - margin), (int)Math.Floor(topLeft.Y - margin),
                (int)Math.Ceiling(size.X + margin * 2f), (int)Math.Ceiling(size.Y + margin * 2f));

            for (var parent = control.Parent; parent != null; parent = parent.Parent)
            {
                if (parent is CameraPanel)
                    return false;
                if (parent.HideOverflow && !parent.DestinationRectangle.Intersects(area))
                    return true;
            }

            return false;
        }
    }
}
